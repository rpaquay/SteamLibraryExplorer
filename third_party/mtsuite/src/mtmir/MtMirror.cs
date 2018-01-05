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
using System.Reflection;
using mtsuite.shared;
using mtsuite.shared.CommandLine;
using mtsuite.CoreFileSystem;

namespace mtmir {
  public class MtMirror {
    private readonly IFileSystem _fileSystem;
    private readonly ParallelFileSystem _parallelFileSystem;
    private readonly IProgressMonitor _progressMonitor;
    private readonly IFileComparer _fileComparer;

    public MtMirror(IFileSystem fileSystem) {
      _fileSystem = fileSystem;
      _parallelFileSystem = new ParallelFileSystem(fileSystem);
      _progressMonitor = new ProgressMonitor();
      _fileComparer = new LastWriteTimeFileComparer(fileSystem);

      _parallelFileSystem.Error += exception => _progressMonitor.OnError(exception);
      _parallelFileSystem.Pulse += () => _progressMonitor.Pulse();

      _parallelFileSystem.EntriesToDeleteDiscovered +=
        (entry, list) => _progressMonitor.OnEntriesToDeleteDiscovered(entry, list);
      _parallelFileSystem.EntryDeleting += (stopwatch, entry) => _progressMonitor.OnEntryDeleting(stopwatch, entry);
      _parallelFileSystem.EntryDeleted += (stopwatch, entry) => _progressMonitor.OnEntryDeleted(stopwatch, entry);

      _parallelFileSystem.FileCopying += (stopwatch, entry) => _progressMonitor.OnFileCopying(stopwatch, entry);
      _parallelFileSystem.FileCopyingProgress +=
        (stopwatch, entry, bytes) => _progressMonitor.OnFileCopyingProgress(stopwatch, entry, bytes);
      _parallelFileSystem.FileCopied += (stopwatch, entry) => _progressMonitor.OnFileCopied(stopwatch, entry);


      _parallelFileSystem.DirectoryTraversing += (entry) => _progressMonitor.OnDirectoryTraversing(entry);
      _parallelFileSystem.DirectoryTraversed += (entry) => _progressMonitor.OnDirectoryTraversed(entry);
      _parallelFileSystem.DirectoryCreated += (entry) => _progressMonitor.OnDirectoryCreated(entry);
      _parallelFileSystem.EntriesDiscovered += (entry, list) => _progressMonitor.OnEntriesDiscovered(entry, list);
      _parallelFileSystem.FileCopySkipped += (entry) => _progressMonitor.OnFileSkipped(entry, entry.FileSize);
    }

    public void Run(string[] args) {
      DisplayBanner();

      var argumentDefinitions = new ArgumentDefinitionBuilder()
        .WithString("source-path", "The path of the source directory", true)
        .WithString("destination-path", "The path of the destination directory", true)
        .WithThreadCountSwitch()
        .WithGcSwitch()
        .WithHelpSwitch()
        .Build();

      var parser = new ArgumentsParser(argumentDefinitions, args);
      parser.Parse();
      if (!parser.IsValid || parser.Contains("help")) {
        if (!parser.Contains("help")) {
          foreach (var error in parser.Errors) {
            Console.WriteLine("ERROR: {0}", error);
          }
          Console.WriteLine();
        }
        DisplayUsage(argumentDefinitions);
        throw new CommandLineReturnValueException(16); // To match robocopy
      }

      var sourcePath = ProgramHelpers.MakeFullPath(parser["source-path"].StringValue);
      var destinationPath = ProgramHelpers.MakeFullPath(parser["destination-path"].StringValue);
      ProgramHelpers.SetWorkerThreadCount(parser["thread-count"].IntValue);

      var statistics = DoMirror(sourcePath, destinationPath);
      DisplayResults(statistics);
      if (parser.Contains("gc")) {
        ProgramHelpers.DisplayGcStatistics();
      }

      if (statistics.Errors.Count > 0)
        throw new CommandLineReturnValueException(8); // 8 = fail (to match robocopy)
    }

    private static void DisplayUsage(IList<ArgDef> argumentDefinitions) {
      Console.WriteLine("Mirrors all files and directories from one location to another location.");
      Console.WriteLine("Extra files and directories in the destination location are deleted.");
      Console.WriteLine();
      Console.WriteLine("Usage: {0} {1}", Process.GetCurrentProcess().ProcessName,
        ArgumentsHelper.BuildUsageSummary(argumentDefinitions));
      Console.WriteLine();
      ArgumentsHelper.PrintArgumentUsageSummary(argumentDefinitions);
    }

    public Statistics DoMirror(FullPath sourcePath, FullPath destinationPath) {
      // Check source exists
      FileSystemEntry sourceDirectory;
      try {
        sourceDirectory = _fileSystem.GetEntry(sourcePath);
      } catch (Exception e) {
        Console.WriteLine(e.Message);
        // 8 = fail (to match robocopy)
        throw new CommandLineReturnValueException(8);
      }

      Console.WriteLine("Mirroring files and directories from \"{0}\" to \"{1}\"",
        PathHelpers.StripLongPathPrefix(sourcePath.Path), PathHelpers.StripLongPathPrefix(destinationPath.Path));
      _progressMonitor.Start();

      bool destinationIsNew;
      try {
        _fileSystem.GetEntry(destinationPath);
        destinationIsNew = false;
      } catch {
        destinationIsNew = true;
      }

      var task = _parallelFileSystem.CopyDirectoryAsync(
        sourceDirectory,
        destinationPath,
        CopyOptions.DeleteMismatchedFiles | CopyOptions.SkipIdenticalFiles | CopyOptions.DeleteExtraFiles,
        _fileComparer,
        destinationIsNew);
      _parallelFileSystem.WaitForTask(task);
      _progressMonitor.Stop();
      return _progressMonitor.GetStatistics();
    }

    private static void DisplayBanner() {
      Console.WriteLine();
      Console.WriteLine("-------------------------------------------------------------------------------");
      Console.WriteLine("MTMIR :: Multi-Threaded Directory Mirroring for Windows - version {0}",
        Assembly.GetExecutingAssembly().GetName().Version.ToString());
      Console.WriteLine("-------------------------------------------------------------------------------");
      Console.WriteLine();
    }

    private static void DisplayResults(Statistics statistics) {
      ProgramHelpers.DisplayFullStatistics(statistics);
    }
  }
}
