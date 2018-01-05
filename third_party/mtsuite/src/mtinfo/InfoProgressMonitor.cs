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
using mtsuite.shared;
using mtsuite.shared.Utils;

namespace mtinfo {
  public class InfoProgressMonitor : ProgressMonitor {
    protected override void DisplayStatus(Statistics statistics) {
      var totalSeconds = statistics.ElapsedTime.TotalSeconds;

      Console.Write(
        "\rCollected {0:n0}/{1:n0} directories, " +
        "{2:n0}/{3:n0} files, {4:n0} entries/sec, {5:n0} errors, " +
        "in {6}",
        statistics.DirectoryTraversedCount,
        statistics.DirectoryEnumeratedCount,
        statistics.FileCopiedCount + statistics.SymlinkCopiedCount,
        statistics.FileEnumeratedCount,
        statistics.EntryCopiedCount / totalSeconds,
        statistics.Errors.Count,
        FormatHelpers.FormatElapsedTime(statistics.ElapsedTime));
    }
  }
}