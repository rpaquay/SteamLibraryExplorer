// Copyright 2015 Renaud Paquay All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using mtsuite.shared.Collections;
using mtsuite.CoreFileSystem;
using mtsuite.CoreFileSystem.ObjectPool;
using mtsuite.shared.Tasks;

namespace mtsuite.shared {
  public class ParallelFileSystem : IParallelFileSystem {
    private readonly IFileSystem _fileSystem;
    private readonly ITaskFactory _taskFactory = new DefaultTaskFactory();
    private readonly IPool<List<FileSystemEntry>> _entryListPool = new ListPool<FileSystemEntry>();

    private readonly IPool<SmallSet<FileSystemEntry>> _entrySetPool =
      PoolFactory<SmallSet<FileSystemEntry>>.Create(
        () => new SmallSet<FileSystemEntry>(FileSystemEntryNameComparer.Instance),
        x => x.Clear());

    public ParallelFileSystem(IFileSystem fileSystem) {
      _fileSystem = fileSystem;
    }

    public event Action<Exception> Error;
    public event Action Pulse;
    public event Action<FileSystemEntry> EntriesDiscovering;
    public event Action<FileSystemEntry, List<FileSystemEntry>> EntriesDiscovered;
    public event Action<FileSystemEntry, List<FileSystemEntry>> EntriesProcessed;
    public event Action<FileSystemEntry> EntriesToDeleteDiscovering;
    public event Action<FileSystemEntry, List<FileSystemEntry>> EntriesToDeleteDiscovered;
    public event Action<FileSystemEntry, List<FileSystemEntry>> EntriesToDeleteProcessed;
    public event Action<Stopwatch, FileSystemEntry> EntryDeleting;
    public event Action<Stopwatch, FileSystemEntry> EntryDeleted;
    public event Action<FileSystemEntry> FileCopySkipped;
    public event Action<Stopwatch, FileSystemEntry> FileCopying;
    public event Action<Stopwatch, FileSystemEntry, long> FileCopyingProgress;
    public event Action<Stopwatch, FileSystemEntry> FileCopied;
    public event Action<FileSystemEntry> DirectoryTraversing;
    public event Action<FileSystemEntry> DirectoryTraversed;
    public event Action<FileSystemEntry> DirectoryCreated;

    private FromPool<List<FileSystemEntry>> GetDirectoryEntries(FullPath directoryPath) {
      FromPool<List<FileSystemEntry>> entries;
      try {
        entries = _fileSystem.GetDirectoryEntries(directoryPath);
      } catch (Exception e) {
        OnError(e);
        entries = _entryListPool.AllocateFrom();
      }
      return entries;
    }

    public void WaitForTask(ITask task) {
      while (true) {
        var completed = task.Wait(TimeSpan.FromMilliseconds(50));
        if (completed)
          break;
        OnPulse();
      }
    }

    public ITask<T> TraverseDirectoryAsync<T>(FileSystemEntry directoryEntry, IDirectorCollector<T> collector) {
      return TraverseDirectoryAsync<T>(directoryEntry, collector, 0, true);
    }

    private ITask<T> TraverseDirectoryAsync<T>(
      FileSystemEntry directoryEntry,
      IDirectorCollector<T> collector,
      int depth,
      bool skipNotification) {

      return _taskFactory.StartNew(() => {
        if (!skipNotification)
          OnDirectoryTraversing(directoryEntry);
        var item = collector.CreateItemForDirectory(directoryEntry, depth);
        return TraverseDirectoryEntriesAsync(
          directoryEntry,
          collector,
          item,
          depth);
      }).Then(t => {
        if (!skipNotification)
          OnDirectoryTraversed(directoryEntry);
        return t.Result;
      });
    }

    private ITask<T> TraverseDirectoryEntriesAsync<T>(
      FileSystemEntry directoryEntry,
      IDirectorCollector<T> collector,
      T item,
      int depth) {

      OnEntriesDiscovering(directoryEntry);
      var entries = GetDirectoryEntries(directoryEntry.Path);
      OnEntriesDiscovered(directoryEntry, entries.Item);

      collector.OnDirectoryEntriesEnumerated(item, directoryEntry, entries.Item);
      var directoryInfoTasks = _taskFactory.CreateCollection<T>();
      foreach (var entry in entries.Item) {
        if (entry.IsDirectory && !entry.IsReparsePoint) {
          var directoryTask = TraverseDirectoryAsync(entry, collector, depth + 1, false);
          directoryInfoTasks.Add(directoryTask);
        }
      }

      return directoryInfoTasks.ContinueWith(tasks => {
        OnEntriesProcessed(directoryEntry, entries.Item);
        entries.Dispose();

        foreach (var task in tasks) {
          collector.OnDirectoryTraversed(item, task.Result);
        }

        return item;
      });
    }

    public ITask CopyDirectoryAsync(
      FileSystemEntry sourceDirectory,
      FullPath destinationPath,
      CopyOptions options,
      IFileComparer fileComparer,
      bool destinationDirectoryIsNew) {
      return CopyDirectoryAsync(sourceDirectory, destinationPath, options, fileComparer, destinationDirectoryIsNew, true);
    }

    private ITask CopyDirectoryAsync(
      FileSystemEntry sourceDirectory,
      FullPath destinationPath,
      CopyOptions options,
      IFileComparer fileComparer,
      bool destinationDirectoryIsNew,
      bool skipNotification) {

      var t = _taskFactory.StartNew(() => {
        if (!skipNotification)
          OnDirectoryTraversing(sourceDirectory);
        // This happens if destination directory can't be created.
        return CopyDirectoryEntriesAsync(
          sourceDirectory,
          destinationPath,
          options,
          fileComparer,
          destinationDirectoryIsNew);
      }).Then(task => {
        if (!skipNotification)
          OnDirectoryTraversed(sourceDirectory);
        return task.Result;
      });

      return t;
    }

    private ITask CopyDirectoryEntriesAsync(
      FileSystemEntry sourceDirectory,
      FullPath destinationPath,
      CopyOptions options,
      IFileComparer fileComparer,
      bool destinationDirectoryIsNew) {

      // Ensure destination directory is created (or exists)
      var destinationDirectoryOpt = GetOrCreateDirectory(destinationPath, destinationDirectoryIsNew);
      if (destinationDirectoryOpt == null)
        return _taskFactory.CompletedTask;
      var destinationDirectory = destinationDirectoryOpt.Value;

      OnEntriesDiscovering(sourceDirectory);
      var sourceEntries = GetDirectoryEntries(sourceDirectory.Path);
      OnEntriesDiscovered(sourceDirectory, sourceEntries.Item);

      // Figure out what the destination contains in terms of files to skip.
      var destinationEntries = destinationDirectoryIsNew
        ? _entryListPool.AllocateFrom()
        : GetDirectoryEntries(destinationPath);
      var destinationSet = _entrySetPool.AllocateFrom();
      destinationSet.Item.SetList(destinationEntries.Item);

      OnEntriesToDeleteDiscovering(destinationDirectory);
      var entriesToDelete = ComputeDestinationEntriesToDelete(sourceEntries.Item, destinationEntries.Item, options);
      OnEntriesToDeleteDiscovered(destinationDirectory, entriesToDelete);

      var deleteTasks = _taskFactory.CreateCollection(entriesToDelete.Select(entry => DeleteEntryAsync(entry)));
      return deleteTasks
        .Then(t => {
          var copySubDirectoriesTasks = _taskFactory.CreateCollection(sourceEntries.Item
            .Where(entry => entry.IsDirectory && !entry.IsReparsePoint)
            .Select(sourceEntry => {
              var destinationEntryPath = destinationDirectory.Path.Combine(sourceEntry.Name);
              var isNewDestination = !destinationSet.Item.Contains(sourceEntry);
              return CopyDirectoryAsync(sourceEntry, destinationEntryPath, options, fileComparer, isNewDestination, false/*skipNotification*/);
            }));

          return copySubDirectoriesTasks
            .ContinueWith(_ => {
              CopyFileEntries(sourceEntries.Item, destinationDirectory, fileComparer, destinationSet.Item);
              OnEntriesProcessed(sourceDirectory, sourceEntries.Item);
              sourceEntries.Dispose();
              destinationEntries.Dispose();
              destinationSet.Dispose();
            });
        });
    }

    private FileSystemEntry? GetOrCreateDirectory(FullPath destinationPath, bool destinationDirectoryIsNew) {
      var directoryCreated = false;
      // Create destination directory (throw if error)
      if (destinationDirectoryIsNew) {
        try {
          _fileSystem.CreateDirectory(destinationPath);
          directoryCreated = true;
        } catch (Exception e) {
          OnError(e);
        }
      }

      FileSystemEntry destinationDirectory;
      try {
        destinationDirectory = _fileSystem.GetEntry(destinationPath);
      } catch (Exception e) {
        OnError(e);
        // If we can find the destination entry, give up this directory.
        return null;
      }

      if (directoryCreated)
        OnDirectoryCreated(destinationDirectory);
      return destinationDirectory;
    }

    private static List<FileSystemEntry> ComputeDestinationEntriesToDelete(
      List<FileSystemEntry> sourceEntries,
      List<FileSystemEntry> destinationEntries,
      CopyOptions options) {

      if (destinationEntries.Count == 0)
        return destinationEntries;

      var entriesToDelete = new List<FileSystemEntry>();

      // Note: DeleteExtraFiles is a strict supeset of DeleteMismatchedFiles
      if ((options & CopyOptions.DeleteExtraFiles) != 0) {
        // Delete files in destination that are either not present in source, or
        // present in source but with a different kind (e.g. file vs directory).
        var extraEntries = destinationEntries.Except(sourceEntries, FileSystemEntryNameComparer.Instance);
        entriesToDelete.AddRange(extraEntries);
      } else if ((options & CopyOptions.DeleteMismatchedFiles) != 0) {
        // Delete files in destination that are present in source (i.e. same
        // name) _and_ with a different kind (e.g. file vs directory).
        var mismatchedEntries = destinationEntries
          .Where(dst => {
            // TODO: Perf: Need a hashset?
            foreach (var src in sourceEntries) {
              // Same name, different "kind"?
              if (StringComparer.OrdinalIgnoreCase.Equals(dst.Name, src.Name)) {
                if (dst.IsFile != src.IsFile ||
                    dst.IsDirectory != src.IsDirectory ||
                    dst.IsReparsePoint != src.IsReparsePoint) {
                  return true;
                }
              }
            }
            return false;
          });
        entriesToDelete.AddRange(mismatchedEntries);
      }
      return entriesToDelete;
    }

    private void CopyFileEntries(
      List<FileSystemEntry> sourceEntries,
      FileSystemEntry destinationDirectory,
      IFileComparer fileComparer,
      SmallSet<FileSystemEntry> destinationSet) {

      var sw = new Stopwatch();
      foreach (var entry in sourceEntries) {
        CopyFileEntry(sw, entry, destinationDirectory, fileComparer, destinationSet);
      }
    }

    private void CopyFileEntry(
      Stopwatch sw,
      FileSystemEntry sourceEntry,
      FileSystemEntry destinationDirectory,
      IFileComparer fileComparer,
      SmallSet<FileSystemEntry> destinationSet) {

      FileSystemEntry destinationEntry;
      var destinationExists = destinationSet.TryGet(sourceEntry, out destinationEntry);
      var destinationPath = destinationExists
        ? destinationEntry.Path
        : destinationDirectory.Path.Combine(sourceEntry.Name);

      if (sourceEntry.IsFile || sourceEntry.IsReparsePoint) {
        if (destinationExists) {
          try {
            if (fileComparer.CompareFiles(sourceEntry, destinationEntry)) {
              OnFileCopySkipped(sourceEntry);
              return;
            }
          } catch (Exception e) {
            // If we can't compare files, log error and continue with normal copy operation.
            OnError(e);
          }
        }

        sw.Restart();
        OnFileCopying(sw, sourceEntry);
        try {
          long lastTransferred = 0;
          CopyFileCallback callback = (copiedBytes, totalBytes) => {
            var additionalTransferred = copiedBytes - lastTransferred;
            OnFileCopyingProgress(sw, sourceEntry, additionalTransferred);
            lastTransferred = copiedBytes;
          };
          if (destinationExists) {
            _fileSystem.CopyFile(sourceEntry, destinationEntry, callback);
          } else {
            _fileSystem.CopyFile(sourceEntry, destinationPath, callback);
          }
        } catch (Exception e) {
          OnError(e);
        }
        OnFileCopied(sw, sourceEntry);
      }
    }

    /// <summary>
    /// Delete a file system entry. Recurse through directories if
    /// the entry is a directory.
    /// </summary>
    public ITask DeleteEntryAsync(FileSystemEntry entry) {
      return DeleteEntryAsync(entry, _ => true);
    }

    /// <summary>
    /// Delete a file system entry. Recurse through directories if
    /// the entry is a directory.
    /// </summary>
    public ITask DeleteEntryAsync(FileSystemEntry entry, Func<FileSystemEntry, bool> includeFilter) {
      if (entry.IsFile || entry.IsReparsePoint)
        return _taskFactory.StartNew(() => DeleteSingleEntry(new Stopwatch(), entry, includeFilter));
      return DeleteDirectoryAsync(entry, includeFilter);
    }

    private ITask DeleteDirectoryAsync(FileSystemEntry directoryEntry, Func<FileSystemEntry, bool> includeFilter) {
      return DeleteDirectoryEntriesAsync(directoryEntry, includeFilter)
        .ContinueWith(t => DeleteSingleEntry(new Stopwatch(), directoryEntry, includeFilter));
    }

    private ITask DeleteDirectoryEntriesAsync(FileSystemEntry directoryEntry, Func<FileSystemEntry, bool> includeFilter) {
      OnEntriesToDeleteDiscovering(directoryEntry);
      var entries = GetDirectoryEntries(directoryEntry.Path);
      OnEntriesToDeleteDiscovered(directoryEntry, entries.Item);
      var tasks = _taskFactory.CreateCollection(entries.Item
        .Where(entry => entry.IsDirectory && !entry.IsReparsePoint)
        .Select(entry => DeleteDirectoryEntriesAsync(entry, includeFilter)));

      return tasks.ContinueWith(_ => {
        DeleteEntries(entries.Item, includeFilter);
        OnEntriesToDeleteProcessed(directoryEntry, entries.Item);
        entries.Dispose();
      });
    }

    private void DeleteEntries(List<FileSystemEntry> entries, Func<FileSystemEntry, bool> includeFilter) {
      var sw = new Stopwatch();
      // Delete all entries
      foreach (var entry in entries) {
        DeleteSingleEntry(sw, entry, includeFilter);
      }
    }

    private void DeleteSingleEntry(Stopwatch sw, FileSystemEntry entry, Func<FileSystemEntry, bool> includeFilter) {
      if (!includeFilter(entry))
        return;

      sw.Restart();
      OnEntryDeleting(sw, entry);
      try {
        _fileSystem.DeleteEntry(entry);
      } catch (Exception e) {
        OnError(e);
      }
      OnEntryDeleted(sw, entry);
    }

    protected virtual void OnError(Exception obj) {
      var handler = Error;
      if (handler != null) handler(obj);
    }

    protected virtual void OnPulse() {
      var handler = Pulse;
      if (handler != null) handler();
    }

    protected virtual void OnEntriesDiscovered(FileSystemEntry arg1, List<FileSystemEntry> arg2) {
      var handler = EntriesDiscovered;
      if (handler != null) handler(arg1, arg2);
    }

    protected virtual void OnEntriesToDeleteDiscovering(FileSystemEntry obj) {
      var handler = EntriesToDeleteDiscovering;
      if (handler != null) handler(obj);
    }

    protected virtual void OnEntriesToDeleteDiscovered(FileSystemEntry directoryEntry, List<FileSystemEntry> obj) {
      var handler = EntriesToDeleteDiscovered;
      if (handler != null) handler(directoryEntry, obj);
    }

    protected virtual void OnEntryDeleting(Stopwatch arg1, FileSystemEntry arg2) {
      var handler = EntryDeleting;
      if (handler != null) handler(arg1, arg2);
    }

    protected virtual void OnEntryDeleted(Stopwatch arg1, FileSystemEntry arg2) {
      var handler = EntryDeleted;
      if (handler != null) handler(arg1, arg2);
    }

    protected virtual void OnFileCopySkipped(FileSystemEntry obj) {
      var handler = FileCopySkipped;
      if (handler != null) handler(obj);
    }

    protected virtual void OnFileCopying(Stopwatch arg1, FileSystemEntry arg2) {
      var handler = FileCopying;
      if (handler != null) handler(arg1, arg2);
    }

    protected virtual void OnFileCopyingProgress(Stopwatch arg1, FileSystemEntry arg2, long arg3) {
      var handler = FileCopyingProgress;
      if (handler != null) handler(arg1, arg2, arg3);
    }

    protected virtual void OnFileCopied(Stopwatch arg1, FileSystemEntry arg2) {
      var handler = FileCopied;
      if (handler != null) handler(arg1, arg2);
    }

    protected virtual void OnDirectoryTraversing(FileSystemEntry obj) {
      var handler = DirectoryTraversing;
      if (handler != null) handler(obj);
    }

    protected virtual void OnDirectoryTraversed(FileSystemEntry obj) {
      var handler = DirectoryTraversed;
      if (handler != null) handler(obj);
    }

    protected virtual void OnDirectoryCreated(FileSystemEntry obj) {
      var handler = DirectoryCreated;
      if (handler != null) handler(obj);
    }

    protected virtual void OnEntriesProcessed(FileSystemEntry arg1, List<FileSystemEntry> arg2) {
      var handler = EntriesProcessed;
      if (handler != null) handler(arg1, arg2);
    }

    protected virtual void OnEntriesToDeleteProcessed(FileSystemEntry arg1, List<FileSystemEntry> arg2) {
      var handler = EntriesToDeleteProcessed;
      if (handler != null) handler(arg1, arg2);
    }

    protected virtual void OnEntriesDiscovering(FileSystemEntry obj) {
      var handler = EntriesDiscovering;
      if (handler != null) handler(obj);
    }
  }
}