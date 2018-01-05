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
using mtsuite.shared.Utils;

namespace mtdel {
  public class MtDelete {
    private readonly IFileSystem _fileSystem;
    private readonly ParallelFileSystem _parallelFileSystem;
    private readonly IProgressMonitor _progressMonitor;

    public MtDelete(IFileSystem fileSystem) {
      _fileSystem = fileSystem;
      _parallelFileSystem = new ParallelFileSystem(fileSystem);
      _progressMonitor = new DeleteProgressMonitor();

      _parallelFileSystem.Error += exception => _progressMonitor.OnError(exception);
      _parallelFileSystem.Pulse += () => _progressMonitor.Pulse();
      //_parallelFileSystem.EntriesToDeleteDiscovering += entry => _progressMonitor.OnEntriesToDeleteDiscovering(entry);
      _parallelFileSystem.EntriesToDeleteDiscovered += (entry, list) => _progressMonitor.OnEntriesToDeleteDiscovered(entry, list);
      _parallelFileSystem.EntryDeleting += (stopwatch, entry) => _progressMonitor.OnEntryDeleting(stopwatch, entry);
      _parallelFileSystem.EntryDeleted += (stopwatch, entry) => _progressMonitor.OnEntryDeleted(stopwatch, entry);
    }

    public void Run(string[] args) {
      DisplayBanner();

      var argumentDefinitions = new ArgumentDefinitionBuilder()
        .WithString("directory-path", "The path of the directory to delete", true)
        .WithSwitch("quiet", "Quiet mode, do not ask if ok to delete", "q", "", "quiet")
        .WithStringFlag("attributes", @"Selects files to delete based on attributes
R  Read-only files            S  System files
H  Hidden files               A  Files ready for archiving
I  Not content indexed Files  L  Reparse Points
-  Prefix meaning not", "a", "attributes", null, value => new AttributesFilterParser().Parse(value).Error)
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

      var sourcePath = ProgramHelpers.MakeFullPath(parser["directory-path"].StringValue);
      ProgramHelpers.SetWorkerThreadCount(parser["thread-count"].IntValue);

      var options = new Options {
        QuietMode = parser.Contains("quiet"),
        Attributes = parser.Contains("attributes") ? new AttributesFilterParser().Parse(parser["attributes"].StringValue) : null,
      };
      var statistics = DoDelete(sourcePath, options);
      DisplayResults(statistics);
      if (parser.Contains("gc")) {
        ProgramHelpers.DisplayGcStatistics();
      }

      // 0 = success, 8 = fail (to match robocopy)
      if (statistics.Errors.Count > 0)
        throw new CommandLineReturnValueException(8);
    }

    private static void DisplayUsage(IList<ArgDef> argumentDefinitions) {
      Console.WriteLine("Deletes a directory recursively, i.e. including all files and sub-directories.");
      Console.WriteLine();
      Console.WriteLine("Usage: {0} {1}", Process.GetCurrentProcess().ProcessName,
        ArgumentsHelper.BuildUsageSummary(argumentDefinitions));
      Console.WriteLine();
      ArgumentsHelper.PrintArgumentUsageSummary(argumentDefinitions);
    }

    public class Options {
      public bool QuietMode { get; set; }
      public AttributesFilterParserResult Attributes { get; set; }
    }

    public Statistics DoDelete(FullPath sourcePath, Options options) {
      FileSystemEntry rootEntry;
      try {
        rootEntry = _fileSystem.GetEntry(sourcePath);
      } catch (Exception e) {
        Console.WriteLine(e.Message);
        // 8 = fail (to match robocopy)
        throw new CommandLineReturnValueException(8); // To match robocopy
      }

      if (!options.QuietMode) {
        while (true) {
          Console.Write("{0}, Are you sure (Y/N)? ", PathHelpers.StripLongPathPrefix(sourcePath.FullName));
          var result = Console.ReadLine();
          if (result == null)
            throw new CommandLineReturnValueException(10);

          result = result.ToLower();
          if (result == "y")
            break;
          if (result == "n")
            throw new CommandLineReturnValueException(10);
        }
      }

      Console.WriteLine("Deleting files and subdirectories from \"{0}\"",
        PathHelpers.StripLongPathPrefix(sourcePath.FullName));

      var includeFilter = CreateFilter(options);
      _progressMonitor.Start();
      // HACK: Notify of an extra directory discovered to get counters right.
      _progressMonitor.OnEntriesToDeleteDiscovered(rootEntry, new List<FileSystemEntry>(new[] {rootEntry}));
      var task = _parallelFileSystem.DeleteEntryAsync(rootEntry, includeFilter);
      _parallelFileSystem.WaitForTask(task);
      _progressMonitor.Stop();
      return _progressMonitor.GetStatistics();
    }

    private static Func<FileSystemEntry, bool> CreateFilter(Options options) {
      Func<FileSystemEntry, bool> includeFilter = _ => true;
      if (options.Attributes == null) {
        return _ => true;
      }

      return entry => {
        if (entry.IsReparsePoint || entry.IsFile) {
          return options.Attributes.AnyIncludes(entry.FileAttributes);
        } else if (entry.IsDirectory) {
          // Never delete directories when "/a" option is used
          return false;
        }
        return false;
      };
    }

    private static void DisplayBanner() {
      Console.WriteLine();
      Console.WriteLine("-------------------------------------------------------------------------------");
      Console.WriteLine("MTDEL :: Multi-threaded Delete for Windows - version {0}",
        Assembly.GetExecutingAssembly().GetName().Version.ToString());
      Console.WriteLine("-------------------------------------------------------------------------------");
      Console.WriteLine();
    }

    private static void DisplayResults(Statistics statistics) {
      Console.WriteLine();
      Console.WriteLine("Statistics:");
      Console.WriteLine("  Elapsed time:             {0}", FormatHelpers.FormatElapsedTime(statistics.ElapsedTime));
      Console.WriteLine("  CPU time:                 {0}", FormatHelpers.FormatElapsedTime(statistics.ThreadElapsedTime));
      Console.WriteLine("  # entries deleted/sec:    {0:n0}", statistics.EntryDeletedCount / statistics.ElapsedTime.TotalSeconds);
      Console.WriteLine("  # of directories deleted: {0:n0}", statistics.DirectoryDeletedCount);
      Console.WriteLine("  # of files deleted:       {0:n0}", statistics.FileDeletedCount);
      Console.WriteLine("  # of symlinks deleted:    {0:n0}", statistics.SymlinkDeletedCount);
      var fileSizeTotalMb = statistics.FileDeletedTotalSize / 1024 / 1024;
      Console.WriteLine("    Total bytes deleted:    {0:n0} MB", fileSizeTotalMb);
      Console.WriteLine("  # of errors:              {0:n0}", statistics.Errors.Count);
      ProgramHelpers.DisplayErrors(statistics.Errors);
    }
  }
}
