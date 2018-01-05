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
using mtsuite.CoreFileSystem;

namespace mtsuite.shared {
  public interface IProgressMonitor {
    #region single threaded methods
    void Start();
    void Pulse();
    void Stop();

    Statistics GetStatistics();
    #endregion

    #region multi-threaded methods
    void OnEntriesDiscovered(FileSystemEntry directory, List<FileSystemEntry> entries);
    void OnEntriesToDeleteDiscovered(FileSystemEntry directory, List<FileSystemEntry> entries);

    void OnEntriesProcessed(FileSystemEntry directory, List<FileSystemEntry> entries);

    void OnDirectoryTraversing(FileSystemEntry directory);
    void OnDirectoryTraversed(FileSystemEntry directory);

    void OnEntryDeleting(Stopwatch stopwatch, FileSystemEntry entry);
    void OnEntryDeleted(Stopwatch stopwatch, FileSystemEntry entry);

    void OnDirectoryCreated(FileSystemEntry entry);

    void OnFileSkipped(FileSystemEntry sourceEntry, long size);

    void OnFileCopying(Stopwatch stopwatch, FileSystemEntry entry);
    void OnFileCopyingProgress(Stopwatch stopwatch, FileSystemEntry entry, long size);
    void OnFileCopied(Stopwatch stopwatch, FileSystemEntry entry);

    void OnError(Exception e);
    #endregion
  }
}