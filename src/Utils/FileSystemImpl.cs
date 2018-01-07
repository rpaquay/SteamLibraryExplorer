﻿using mtsuite.CoreFileSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteamLibraryExplorer.Utils {
  public class FileSystemImpl : FileSystem {
    private readonly IFileSystem _coreFileSystem = new mtsuite.CoreFileSystem.FileSystem(new PathSerializers.LongPathAsNeededSerializer());

    protected override string ReadAllTextImpl(FullPath path) {
      using (var fileSteam = _coreFileSystem.OpenFile(path, FileAccess.Read)) {
        using (var streamReader = new StreamReader(fileSteam, Encoding.UTF8, true, 4096, false))
          return streamReader.ReadToEnd();
      }
    }

    protected override FileSystemEntry GetEntryImpl(FullPath path) {
      return _coreFileSystem.GetEntry(path);
    }

    protected override long GetFileSizeImpl(FullPath path) {
      return _coreFileSystem.GetEntry(path).FileSize;
    }

    protected override bool DirectoryExistsImpl(FullPath path) {
      FileSystemEntry entry;
      if (!_coreFileSystem.TryGetEntry(path, out entry)) {
        return false;
      }
      return entry.IsDirectory;
    }

    protected override bool FileExistsImpl(FullPath path) {
      FileSystemEntry entry;
      if (!_coreFileSystem.TryGetEntry(path, out entry)) {
        return false;
      }
      return entry.IsFile;
    }

    protected override void CreateDirectoryImpl(FullPath path) {
      _coreFileSystem.CreateDirectory(path);
    }

    protected override void DeleteDirectoryImpl(FullPath path) {
      FileSystemEntry entry;
      if (!_coreFileSystem.TryGetEntry(path, out entry)) {
        return;
      }

      _coreFileSystem.DeleteEntry(entry);
    }

    protected override void DeleteFileImpl(FullPath path) {
      FileSystemEntry entry;
      if (!_coreFileSystem.TryGetEntry(path, out entry)) {
        return;
      }

      _coreFileSystem.DeleteEntry(entry);
    }

    protected override void CopyFileImpl(FileSystemEntry sourceEntry, FullPath destinationPath, CopyFileOptions options, CopyFileCallback callback) {
      _coreFileSystem.CopyFile(sourceEntry, destinationPath, options, callback);
    }

    protected override IEnumerable<FileSystemEntry> EnumerateFilesImpl(FullPath path, string pattern = null) {
      return EnumerateEntriesImpl(path, pattern).Where(x => x.IsFile);
    }

    protected override IEnumerable<FileSystemEntry> EnumerateDirectoriesImpl(FullPath path, string pattern = null) {
      return EnumerateEntriesImpl(path, pattern).Where(x => x.IsDirectory);
    }

    protected override IEnumerable<FileSystemEntry> EnumerateEntriesImpl(FullPath path, string pattern = null) {
      return _coreFileSystem.EnumerateDirectoryEntries(path, pattern);
    }
  }
}