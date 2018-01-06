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

using mtsuite.CoreFileSystem.Utils;

namespace mtsuite.CoreFileSystem.Win32 {
  /// <summary>
  /// Provide efficient/copy-free text representation of <see cref="IStringSource"/> objects.
  /// </summary>
  public interface IStringSourceFormatter {
    int GetLength(IStringSource source);
    void CopyTo(IStringSource source, StringBuffer destination);
  }

  public abstract class StringSourceFormatter<T> : IStringSourceFormatter where T: IStringSource {
    public abstract int GetLengthImpl(T source);
    public abstract void CopyToImpl(T source, StringBuffer destination);

    public string GetText(T source) {
      var sb = new StringBuffer();
      CopyToImpl(source, sb);
      return sb.ToString();
    }

    int IStringSourceFormatter.GetLength(IStringSource source) {
      return GetLengthImpl((T) source);
    }

    void IStringSourceFormatter.CopyTo(IStringSource source, StringBuffer destination) {
      CopyToImpl((T) source, destination);
    }
  }

  /// <summary>
  /// Marker interface for objects that can be formatted with a <see cref="IStringSourceFormatter"/>.
  /// </summary>
  public interface IStringSource {
  }
}