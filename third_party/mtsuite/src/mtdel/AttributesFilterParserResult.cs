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

using System.IO;

namespace mtdel {
  public class AttributesFilterParserResult {
    public string Error { get; set; }

    public FileAttributes IncludedAttributes { get; set; }
    public FileAttributes ExcludedAttributes { get; set; }

    /// <summary>
    /// Return <code>true</code> if <paramref name="fileAttributes"/> contains
    /// any attribute defined in <see cref="IncludedAttributes"/> and does not
    /// contain any attribute define in <see cref="ExcludedAttributes"/>.
    /// </summary>
    public bool AnyIncludes(FileAttributes fileAttributes) {
      var included = (IncludedAttributes & fileAttributes) != 0;
      var excluded = (ExcludedAttributes & fileAttributes) != 0;
      return included && !excluded;
    }

    /// <summary>
    /// Return <code>true</code> if <paramref name="fileAttributes"/> contains
    /// all attributes defined in <see cref="IncludedAttributes"/> and does not
    /// contain all attributes defined in <see cref="ExcludedAttributes"/>.
    /// </summary>
    public bool AllIncludes(FileAttributes fileAttributes) {
      var included = (IncludedAttributes & fileAttributes) == IncludedAttributes;
      var excluded = (ExcludedAttributes & fileAttributes) == ExcludedAttributes;
      return included && !excluded;
    }
  }
}