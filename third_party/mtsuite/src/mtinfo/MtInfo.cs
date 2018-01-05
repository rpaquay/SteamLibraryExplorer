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
using System.Reflection;
using System.Text;
using mtsuite.shared;
using mtsuite.shared.CommandLine;
using mtsuite.CoreFileSystem;
using mtsuite.shared.Utils;

namespace mtinfo {
  public class MtInfo {
    private readonly IFileSystem _fileSystem;
    private readonly ParallelFileSystem _parallelFileSystem;
    private readonly IProgressMonitor _progressMonitor;

    public MtInfo(IFileSystem fileSystem) {
      _fileSystem = fileSystem;
      _parallelFileSystem = new ParallelFileSystem(fileSystem);
      _progressMonitor = new InfoProgressMonitor();

      _parallelFileSystem.Error += exception => _progressMonitor.OnError(exception);
      _parallelFileSystem.Pulse += () => _progressMonitor.Pulse();

      _parallelFileSystem.EntriesDiscovered += (entry, list) => _progressMonitor.OnEntriesDiscovered(entry, list);
      _parallelFileSystem.EntriesProcessed += (entry, list) => _progressMonitor.OnEntriesProcessed(entry, list);
      _parallelFileSystem.DirectoryTraversing += (entry) => _progressMonitor.OnDirectoryTraversing(entry);
      _parallelFileSystem.DirectoryTraversed += (entry) => _progressMonitor.OnDirectoryTraversed(entry);
    }

    public void Run(string[] args) {
      DisplayBanner();

      var argumentDefinitions = new ArgumentDefinitionBuilder()
        .WithString("directory-path", "The path of the directory to examine", false, Environment.CurrentDirectory)
        .WithIntFlag("depth",
          "Determine the # of levels of sub-directories to display in the final summary (default=2, minimum=1)",
          "d",
          "count",
          2,
          value => {
            if (value < 1)
              return "Level count must be greater or equal to 1";
            return null;
          },
          "",
          "depth")
        .WithIntFlag("longest-path",
          "Display the longest path encountered during traversal (default=0, minimum=0)",
          "lp",
          "count",
          0,
          value => {
            if (value < 0)
              return "Count must be greater or equal to 0";
            return null;
          },
          "",
          "longestpath")
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
      var levelCount = parser["depth"].IntValue;
      ProgramHelpers.SetWorkerThreadCount(parser["thread-count"].IntValue);

      var options = new CollectOptions {
        LevelCount = levelCount,
        LongestPathsToCollect = parser["longest-path"].IntValue,
      };
      var summaryRoot = DoCollect(sourcePath, options);

      var statistics = _progressMonitor.GetStatistics();
      DisplayResults(statistics);

      Console.WriteLine();
      DisplayDirectoryEntries(summaryRoot, x => x.Stats.FileBytesTotal);

      if (parser.Contains("gc")) {
        ProgramHelpers.DisplayGcStatistics();
      }

      // 0 = success, 8 = fail (to match robocopy)
      if (statistics.Errors.Count > 0)
        throw new CommandLineReturnValueException(8);
    }

    private static void DisplayBanner() {
      Console.WriteLine();
      Console.WriteLine("-------------------------------------------------------------------------------");
      Console.WriteLine("MTINFO :: Multi-Threaded Directory Information for Windows - version {0}",
        Assembly.GetExecutingAssembly().GetName().Version);
      Console.WriteLine("-------------------------------------------------------------------------------");
      Console.WriteLine();
    }

    private static void DisplayUsage(IList<ArgDef> argumentDefinitions) {
      Console.WriteLine("Displays disk and file usage information of a directory.");
      Console.WriteLine();
      Console.WriteLine("Usage: {0} {1}", Process.GetCurrentProcess().ProcessName,
        ArgumentsHelper.BuildUsageSummary(argumentDefinitions));
      Console.WriteLine();
      ArgumentsHelper.PrintArgumentUsageSummary(argumentDefinitions);
    }

    public class CollectOptions {
      public int LevelCount { get; set; }
      public int LongestPathsToCollect { get; set; }
    }

    public DirectorySummaryRoot DoCollect(FullPath sourcePath, CollectOptions options) {
      // Check source exists
      FileSystemEntry sourceDirectory;
      try {
        sourceDirectory = _fileSystem.GetEntry(sourcePath);
      } catch (Exception e) {
        Console.WriteLine(e.Message);
        // 8 = fail (to match robocopy)
        throw new CommandLineReturnValueException(8);
      }

      Console.WriteLine("Collecting info from \"{0}\"", PathHelpers.StripLongPathPrefix(sourcePath.FullName));
      _progressMonitor.Start();
      var directorySummaryCollector = new DirectorySummaryCollector(options);
      var task = _parallelFileSystem.TraverseDirectoryAsync(sourceDirectory, directorySummaryCollector);
      _parallelFileSystem.WaitForTask(task);
      _progressMonitor.Stop();
      return directorySummaryCollector.Root;
    }

    private class DirectorySummaryCollector : IDirectorCollector<DirectorySummary> {
      private readonly CollectOptions _options;
      private readonly DirectorySummaryRoot _root;

      public DirectorySummaryCollector(CollectOptions options) {
        _options = options;
        _root = new DirectorySummaryRoot();
      }

      public DirectorySummaryRoot Root {
        get { return _root; }
      }

      public DirectorySummary CreateItemForDirectory(FileSystemEntry directory, int depth) {
        SetLongestPath(directory);

        var result = new DirectorySummary(directory, depth);
        if (_root.Summary == null)
          _root.Summary = result;
        return result;
      }

      public void OnDirectoryEntriesEnumerated(DirectorySummary summary, FileSystemEntry directory, List<FileSystemEntry> entries) {
        foreach (var entry in entries) {
          SetLongestPath(entry);
          if (entry.IsFile && !entry.IsReparsePoint) {
            summary.Stats.FileCount++;
            summary.Stats.FileBytesTotal += entry.FileSize;
          } else if (entry.IsDirectory && !entry.IsReparsePoint) {
            summary.Stats.DirectoryCount++;
          } else if (entry.IsReparsePoint) {
            summary.Stats.SymlinkCount++;
          }
        }
      }

      private void SetLongestPath(FileSystemEntry entry) {
        if (_options.LongestPathsToCollect == 0)
          return;

        var entryPathLength = entry.Path.Length;
        lock (_root.LongestPaths) {
          if (_root.LongestPaths.Count < _options.LongestPathsToCollect) {
            _root.LongestPaths.Add(new LongestPathInfo {
              Path = entry.Path,
              Length = entryPathLength
            });
            return;
          }

          var minPath = _root.LongestPaths.Root;
          if (minPath.Length > entryPathLength) {
            return;
          }

          _root.LongestPaths.Remove();
          _root.LongestPaths.Add(new LongestPathInfo {
            Path = entry.Path,
            Length = entry.Path.Length
          });
        }
      }

      public void OnDirectoryTraversed(DirectorySummary parentSummary, DirectorySummary childSummary) {
        if (parentSummary.Depth + 1 < _options.LevelCount) {
          parentSummary.Children.Add(childSummary);
        }
        parentSummary.Stats.DirectoryCount += childSummary.Stats.DirectoryCount;
        parentSummary.Stats.FileCount += childSummary.Stats.FileCount;
        parentSummary.Stats.SymlinkCount += childSummary.Stats.SymlinkCount;
        parentSummary.Stats.FileBytesTotal += childSummary.Stats.FileBytesTotal;
      }
    }

    private static void DisplayResults(Statistics statistics) {
      Console.WriteLine();
      Console.WriteLine("Statistics:");
      Console.WriteLine("  Elapsed time:             {0}", FormatHelpers.FormatElapsedTime(statistics.ElapsedTime));
      Console.WriteLine("  CPU time:                 {0}", FormatHelpers.FormatElapsedTime(statistics.ThreadElapsedTime));
      Console.WriteLine("  # of directories:         {0:n0}", statistics.DirectoryTraversedCount);
      Console.WriteLine("  # of files:               {0:n0}", statistics.FileCopiedCount);
      Console.WriteLine("  # of symlinks:            {0:n0}", statistics.SymlinkCopiedCount);
      Console.WriteLine("  # entries/sec:            {0:n0}",
        statistics.EntryCopiedCount / statistics.ElapsedTime.TotalSeconds);

      Console.WriteLine("  # of errors:              {0:n0}", statistics.Errors.Count);
      ProgramHelpers.DisplayErrors(statistics.Errors);
    }

    private static void DisplayDirectoryEntries(
      DirectorySummaryRoot summaryRoot,
      Func<DirectorySummary, long> sortingFunc) {
      const string directoryHeader = "Directory name";
      const string sizeHeader = "Size";
      const string directoriesHeader = "Directories";
      const string filesHeader = "Files";
      const string symlinksHeader = "Symlinks";

      var directorySummary = summaryRoot.Summary;
      var treeInfo = GetTreeInfo(directorySummary);
      var displayInfo = new DisplayInfo {
        NameWidth = Math.Max(treeInfo.MaxNameLength, directoryHeader.Length),
        SizeWidth = Math.Max(string.Format("{0:n0}", treeInfo.MaxFileBytes).Length, sizeHeader.Length),
        DirectoryCountWidth =
          Math.Max(string.Format("{0:n0}", treeInfo.MaxDirectoryCount).Length, directoriesHeader.Length),
        FileCountWidth = Math.Max(string.Format("{0:n0}", treeInfo.MaxFileCount).Length, filesHeader.Length),
        SymlinkCountWidth = Math.Max(string.Format("{0:n0}", treeInfo.MaxSymlinkCount).Length, symlinksHeader.Length),
      };

      Console.WriteLine("{0}─┬─{1}─┬─{2}─┬─{3}─┬─{4}",
        new String('─', displayInfo.NameWidth),
        new String('─', displayInfo.SizeWidth),
        new String('─', displayInfo.DirectoryCountWidth),
        new String('─', displayInfo.FileCountWidth),
        new String('─', displayInfo.SymlinkCountWidth));

      var formatString = string.Format(
        "{{0,-{0}}} │ {{1,-{1}}} │ {{2,-{2}}} │ {{3,-{3}}} │ {{4,-{4}}}",
        displayInfo.NameWidth,
        displayInfo.SizeWidth,
        displayInfo.DirectoryCountWidth,
        displayInfo.FileCountWidth,
        displayInfo.SymlinkCountWidth);
      Console.WriteLine(formatString, directoryHeader, sizeHeader, directoriesHeader, filesHeader, symlinksHeader);

      Console.WriteLine("{0}─┼─{1}─┼─{2}─┼─{3}─┼─{4}",
        new String('─', displayInfo.NameWidth),
        new String('─', displayInfo.SizeWidth),
        new String('─', displayInfo.DirectoryCountWidth),
        new String('─', displayInfo.FileCountWidth),
        new String('─', displayInfo.SymlinkCountWidth));

      DisplayDirectoryEntries(displayInfo,
        directorySummary,
        PathHelpers.StripLongPathPrefix(directorySummary.DirectoryEntry.Path.FullName),
        sortingFunc,
        new List<bool>());

      Console.WriteLine("{0}─┴─{1}─┴─{2}─┴─{3}─┴─{4}",
        new String('─', displayInfo.NameWidth),
        new String('─', displayInfo.SizeWidth),
        new String('─', displayInfo.DirectoryCountWidth),
        new String('─', displayInfo.FileCountWidth),
        new String('─', displayInfo.SymlinkCountWidth));

      if (summaryRoot.LongestPaths.Count > 0) {
        Console.WriteLine("Longest paths (by increasing length): {0}", summaryRoot.LongestPaths.Count);

        while (summaryRoot.LongestPaths.Count > 0) {
          var lp = summaryRoot.LongestPaths.Remove();
          var path = PathHelpers.StripLongPathPrefix(lp.Path.ToString());
          Console.WriteLine("{0}: {1:n0}", path, path.Length);
        }
      }
    }

    private static void DisplayDirectoryEntries(
      DisplayInfo displayInfo,
      DirectorySummary directoryInfo,
      string directoryName,
      Func<DirectorySummary, long> sortingFunc,
      List<bool> isLastList) {

      var sb = new StringBuilder();
      for (var i = 0; i < isLastList.Count; i++) {
        var isLast = isLastList[i];
        if (i < isLastList.Count - 1)
          sb.Append(isLast ? @"    " : @"│   ");
        else
          sb.Append(isLast ? @"└───" : @"├───");
      }
      var indentText = sb.ToString();

      var formatString = string.Format(
        "{{0}}{{1,-{0}}} │ {{2,{1}:n0}} │ {{3,{2}:n0}} │ {{4,{3}:n0}} │ {{5,{4}:n0}}",
        displayInfo.NameWidth - indentText.Length,
        displayInfo.SizeWidth,
        displayInfo.DirectoryCountWidth,
        displayInfo.FileCountWidth,
        displayInfo.SymlinkCountWidth);

      var stats = directoryInfo.Stats;
      Console.WriteLine(formatString,
        indentText,
        directoryName,
        stats.FileBytesTotal,
        stats.DirectoryCount,
        stats.FileCount,
        stats.SymlinkCount);

      var childDirectories = directoryInfo.Children.OrderByDescending(sortingFunc).ToList();

      for (var i = 0; i < childDirectories.Count; i++) {
        var entry = childDirectories[i];

        var isLast = (i == childDirectories.Count - 1);
        isLastList.Add(isLast); // push
        DisplayDirectoryEntries(displayInfo, entry, entry.DirectoryEntry.Name, sortingFunc, isLastList);
        isLastList.RemoveAt(isLastList.Count - 1); // pop
      }
    }

    private class DisplayInfo {
      public int NameWidth { get; set; }
      public int SizeWidth { get; set; }
      public int DirectoryCountWidth { get; set; }
      public int FileCountWidth { get; set; }
      public int SymlinkCountWidth { get; set; }
    }

    private static TreeDisplayInfo GetTreeInfo(DirectorySummary root) {
      var rootStats = root.Stats;

      var result = new TreeDisplayInfo {
        IndentSize = 4,
        MaxDirectoryCount = rootStats.DirectoryCount,
        MaxFileBytes = rootStats.FileBytesTotal,
        MaxFileCount = rootStats.FileCount,
        MaxSymlinkCount = rootStats.SymlinkCount
      };

      result.MaxNameLength = EnumerateDirectories(root).Max(node => {
        // Ignore nodes deeper than given depth
        //if (node.Depth >= levelCount)
        //  return 0;

        // Root node is displayed as full path
        if (node.Depth == 0) {
          return PathHelpers.StripLongPathPrefix(node.DirectorySummary.DirectoryEntry.Path.FullName).Length;
        }

        // Other nodes are displayed as indent + file name
        return node.Depth * result.IndentSize + node.DirectorySummary.DirectoryEntry.Name.Length;
      });

      return result;
    }

    private class TreeDisplayInfo {
      public int IndentSize { get; set; }
      public int MaxNameLength { get; set; }
      public int MaxDirectoryCount { get; set; }
      public int MaxFileCount { get; set; }
      public int MaxSymlinkCount { get; set; }
      public long MaxFileBytes { get; set; }
    }

    /// <summary>
    /// Flatten the list of directories starting at <paramref name="root"/>. Returns
    /// an enumeration of (node, depth) tuples.
    /// </summary>
    private static IEnumerable<DirectorySummaryItem> EnumerateDirectories(DirectorySummary root) {
      var stack = new Stack<DirectorySummaryItem>();
      stack.Push(new DirectorySummaryItem(root, 0));
      while (stack.Count > 0) {
        var item = stack.Pop();
        yield return item;
        foreach (var child in item.DirectorySummary.Children) {
          stack.Push(new DirectorySummaryItem(child, item.Depth + 1));
        }
      }
    }

    private struct DirectorySummaryItem {
      private readonly DirectorySummary _directorySummary;
      private readonly int _depth;

      public DirectorySummaryItem(DirectorySummary directorySummary, int depth) {
        _directorySummary = directorySummary;
        _depth = depth;
      }

      public DirectorySummary DirectorySummary {
        get { return _directorySummary; }
      }

      public int Depth {
        get { return _depth; }
      }
    }
  }
}
