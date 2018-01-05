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
using System.IO;

namespace mtdel {
  public class AttributesFilterParser {
    /// <summary>
    /// R  Read-only files
    /// S  System files
    /// H  Hidden files
    /// A  Files ready for archiving
    /// I  Not content indexed Files
    /// L  Reparse Points
    /// -  Prefix meaning not
    /// </summary>
    public AttributesFilterParserResult Parse(string text) {
      var index = 0;
      var exclude = false;
      var includeAttributes = default(FileAttributes);
      var excludeAttributes = default(FileAttributes);
      while (index < text.Length) {
        char ch = Char.ToLowerInvariant(text[index]);
        switch (ch) {
          case '-':
            if (exclude) {
              return Error("Invalid attribute filter syntax (unexpected character '-')");
            }
            exclude = true;
            index++;
            break;
          case 'r':
            ExcludeAttributes(FileAttributes.ReadOnly, ref exclude, ref includeAttributes, ref excludeAttributes);
            index++;
            break;
          case 's':
            ExcludeAttributes(FileAttributes.System, ref exclude, ref includeAttributes, ref excludeAttributes);
            index++;
            break;
          case 'h':
            ExcludeAttributes(FileAttributes.Hidden, ref exclude, ref includeAttributes, ref excludeAttributes);
            index++;
            break;
          case 'a':
            ExcludeAttributes(FileAttributes.Archive, ref exclude, ref includeAttributes, ref excludeAttributes);
            index++;
            break;
          case 'i':
            ExcludeAttributes(FileAttributes.NotContentIndexed, ref exclude, ref includeAttributes, ref excludeAttributes);
            index++;
            break;
          case 'l':
            ExcludeAttributes(FileAttributes.ReparsePoint, ref exclude, ref includeAttributes, ref excludeAttributes);
            index++;
            break;
          default:
            return Error("Invalid attribute filter syntax (unexpected character '{0}')", ch);
        }
      }
      if (exclude) {
        return Error("A attribute filter is required after '-'");
      }

      return new AttributesFilterParserResult {
        IncludedAttributes = includeAttributes,
        ExcludedAttributes = excludeAttributes,
      };
    }

    private static void ExcludeAttributes(FileAttributes singleAttribute, ref bool exclude, ref FileAttributes includeAttributes, ref FileAttributes excludeAttributes) {
      if (exclude) {
        includeAttributes &= ~singleAttribute;
        excludeAttributes |= singleAttribute;
      } else {
        includeAttributes |= singleAttribute;
        excludeAttributes &= ~singleAttribute;
      }
      exclude = false;
    }

    AttributesFilterParserResult Error(string format, params object[] args) {
      return new AttributesFilterParserResult {
        Error = String.Format(format, args)
      };
    }
  }
}