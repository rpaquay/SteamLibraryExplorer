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

namespace mtsuite.shared {
  public class Statistics {
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan ThreadElapsedTime { get; set; }

    public long DirectoryEnumeratedCount { get; set; }
    public long FileEnumeratedCount { get; set; }
    public long DirectoryToDeleteCount { get; set; }
    public long FileToDeleteCount { get; set; }

    public long DirectoryTraversedCount { get; set; }

    public long FileCopiedCount { get; set; }
    public long SymlinkCopiedCount { get; set; }
    public long EntryCopiedCount { get { return FileCopiedCount + SymlinkCopiedCount; } }
    public long FileCopiedTotalSize { get; set; }

    public long DirectoryDeletedCount { get; set; }
    public long FileDeletedCount { get; set; }
    public long SymlinkDeletedCount { get; set; }
    public long EntryDeletedCount { get { return FileDeletedCount + DirectoryDeletedCount + SymlinkDeletedCount; } }
    public long FileDeletedTotalSize { get; set; }

    public long DirectoryCreatedCount { get; set; }

    public long FileSkippedCount { get; set; }
    public long SymlinkSkippedCount { get; set; }
    public long FileSkippedTotalSize { get; set; }

    public List<Exception> Errors { get; set; }
  }
}