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
using System.Collections;
using System.Collections.Generic;

namespace mtsuite.CoreFileSystem.Win32 {
  /// <summary>
  /// Enumerator for the list of entries of a directory.  Implements of 
  /// <see cref="IEnumerator{DirectoryEntry}"/> as a high-performance
  /// value type. No heap allocation is performeed, except for a <see cref="SafeFindHandle"/>
  /// 
  /// <para>
  /// Note: This is a mutable value type, so copying the value and re-using it
  /// in different context may lead to unspecified behavior. The intent is to use this
  /// value type as local variables used in simple <see cref="MoveNext"/>-<see cref="CurrentEntry"/>
  /// loops.
  /// </para>
  /// </summary>
  public struct DirectoryEntriesEnumerator<TPath> : IEnumerator<DirectoryEntry> {
    private readonly Win32<TPath> _win32;
    private readonly TPath _directoryPath;
    private readonly SafeFindHandle _findHandle;
    private bool _isFirst;

    public DirectoryEntriesEnumerator(Win32<TPath> win32, TPath directoryPath, string pattern) {
      _win32 = win32;
      _directoryPath = directoryPath;
      _findHandle = _win32.FindFirstFile(_directoryPath, pattern, out CurrentEntry.Data);
      _isFirst = true;
    }

    /// <summary>
    /// The current entry, exposed without any copying
    /// </summary>
    public DirectoryEntry CurrentEntry;

    public DirectoryEntry Current {
      get {
        return CurrentEntry;
      }
    }

    public void Dispose() {
      _findHandle?.Close();
    }

    public bool MoveNext() {
      if (_findHandle == null || _findHandle.IsClosed) {
        return false;
      }

      while (true) {
        if (_isFirst) {
          _isFirst = false;
        }
        else {
          if (!_win32.FindNextFile(_findHandle, _directoryPath, out CurrentEntry.Data)) {
            return false;
          }
        }

        if (!Win32<TPath>.SkipSpecialEntry(ref CurrentEntry.Data)) {
          return true;
        }
      }
    }

    public void Reset() {
      throw new NotImplementedException();
    }

    object IEnumerator.Current {
      get { return Current; }
    }
  }
}
