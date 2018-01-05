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
using System.Text;
using System.Threading;
using mtsuite.CoreFileSystem;
using mtsuite.shared.Utils;

namespace mtsuite.shared.CommandLine {
  public static class ProgramHelpers {
    public static int RunProgram(string[] args, Action program) {
      try {
        program();
        return 0;
      } catch (CommandLineReturnValueException e) {
        return e.ReturnCode;
      } catch (Exception e) {
        Console.Error.WriteLine("Unexpected error(s):");
        foreach (var error in FlattenErrors(e)) {
          Console.Error.WriteLine("  {0}", error.Message);
          Console.Error.WriteLine("  {0}", error.StackTrace);
        }
        return 255;
      }
    }

    public static FullPath MakeFullPath(string args) {
      return new FullPath(PathHelpers.MakeLongPath(PathHelpers.NormalizeUserInputPath(Environment.CurrentDirectory, args)));
    }

    public static void DisplayErrors(IEnumerable<Exception> errors) {
      foreach (var error in FlattenErrors(errors)) {
        if (IsInternalError(error)) {
          Console.Error.WriteLine("Internal error: {0}", error.Message);
          foreach (var line in error.StackTrace.Replace("\r\n", "\n").Split('\n')) {
            Console.Error.WriteLine("    {0}", line);
          }
        } else {
          Console.Error.WriteLine("Error: {0}", error.Message);
        }
      }
    }

    private static bool IsInternalError(Exception error) {
      return error is ArgumentException ||
        error is NullReferenceException ||
        error is InvalidOperationException;
    }

    public static IEnumerable<Exception> FlattenErrors(Exception error) {
      var agg = error as AggregateException;
      if (agg != null) {
        foreach (var inner in FlattenErrors(agg.InnerExceptions)) {
          foreach (var x in FlattenErrors(inner)) {
            yield return x;
          }
        }
      } else {
        for (Exception inner = error; inner != null; inner = inner.InnerException) {
          yield return inner;
        }
      }
    }

    public static IEnumerable<Exception> FlattenErrors(IEnumerable<Exception> errors) {
      return errors.SelectMany(error => FlattenErrors(error));
    }

    public static void SetWorkerThreadCount(int count) {
      if (count == -1)
        return;
      int mint, minc;
      ThreadPool.GetMinThreads(out mint, out minc);
      int maxt, maxc;
      ThreadPool.GetMaxThreads(out maxt, out maxc);
      ThreadPool.SetMinThreads(count, minc);
      ThreadPool.SetMaxThreads(count, maxc);
    }


    public static void DisplayFullStatistics(Statistics statistics) {
      Console.WriteLine();
      Console.WriteLine("Statistics:");
      Console.WriteLine("  Elapsed time:             {0}", FormatHelpers.FormatElapsedTime(statistics.ElapsedTime));
      Console.WriteLine("  CPU time:                 {0}", FormatHelpers.FormatElapsedTime(statistics.ThreadElapsedTime));
      Console.WriteLine("  # of source directories:    {0:n0}", statistics.DirectoryTraversedCount);
      Console.WriteLine("  # of source files:          {0:n0}", statistics.FileCopiedCount + statistics.FileSkippedCount);
      Console.WriteLine("  # of source symlinks:       {0:n0}", statistics.SymlinkCopiedCount + statistics.SymlinkSkippedCount);
      Console.WriteLine("  Copied entries");
      Console.WriteLine("    # of directories created: {0:n0}", statistics.DirectoryCreatedCount);
      Console.WriteLine("    # of files copied:        {0:n0}", statistics.FileCopiedCount);
      Console.WriteLine("    # of symlinks copied:     {0:n0}", statistics.SymlinkCopiedCount);
      var fileSizeTotalMb = statistics.FileCopiedTotalSize / 1024 / 1024;
      Console.WriteLine("    Total bytes copied:       {0:n0} MB", fileSizeTotalMb);
      Console.WriteLine("    Throughput:               {0:n2} MB/sec",
        fileSizeTotalMb / statistics.ElapsedTime.TotalSeconds);

      Console.WriteLine("  Deleted entries");
      Console.WriteLine("    # of directories deleted: {0:n0}", statistics.DirectoryDeletedCount);
      Console.WriteLine("    # of files deleted:       {0:n0}", statistics.FileDeletedCount);
      Console.WriteLine("    # of symlinks deleted:    {0:n0}", statistics.SymlinkDeletedCount);

      Console.WriteLine("  Skipped entries");
      var fileSkippedTotalSizeMb = statistics.FileSkippedTotalSize / 1024 / 1024;
      Console.WriteLine("    # of files skipped:       {0:n0}", statistics.FileSkippedCount);
      Console.WriteLine("    # of symlinks skipped:    {0:n0}", statistics.SymlinkSkippedCount);
      Console.WriteLine("    Total bytes skipped:      {0:n0} MB", fileSkippedTotalSizeMb);

      Console.WriteLine("  # entries/sec:            {0:n0}",
        (statistics.EntryCopiedCount + statistics.EntryDeletedCount + statistics.FileSkippedCount) /
        statistics.ElapsedTime.TotalSeconds);

      Console.WriteLine("  # of errors:              {0:n0}", statistics.Errors.Count);
      DisplayErrors(statistics.Errors);
    }

    public static void DisplayGcStatistics() {
      Console.WriteLine();
      var sb = new StringBuilder();
      sb.AppendFormat("GC Memory: {0:n0} KB", GC.GetTotalMemory(false) / 1024);
      for (var i = 0; i <= GC.MaxGeneration; i++) {
        sb.AppendFormat(", Gen{0} collections: {1:n0}", i, GC.CollectionCount(i));
      }
      Console.WriteLine(sb.ToString());
#if false
      var process = Process.GetCurrentProcess();
      DisplayCounter(process, ".NET CLR Memory", "% Time in GC", s => string.Format("{0}", s));
      DisplayCounter(process, ".NET CLR Memory", "Gen 0 heap size", s => string.Format("{0:n0} KB", s / 1024));
      DisplayCounter(process, ".NET CLR Memory", "Gen 1 heap size", s => string.Format("{0:n0} KB", s / 1024));
      DisplayCounter(process, ".NET CLR Memory", "Gen 2 heap size", s => string.Format("{0:n0} KB", s / 0124));
      DisplayCounter(process, ".NET CLR Memory", "Large Object Heap size", s => string.Format("{0:n0} KB", s / 1024));
      DisplayCounter(process, ".NET CLR Memory", "# Bytes in all heaps", s => string.Format("{0:n0} KB", s / 1024));
      Console.ReadLine();
#endif
    }

    private static void DisplayCounter(Process process, string categoryName, string counterName, Func<float, string> formatSample) {
      try {
        var c1 = new PerformanceCounter(categoryName, counterName, process.ProcessName);
        Console.WriteLine("{0}-{1}: {2}", categoryName, counterName, formatSample(c1.NextValue()));
      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }
    }
  }
}