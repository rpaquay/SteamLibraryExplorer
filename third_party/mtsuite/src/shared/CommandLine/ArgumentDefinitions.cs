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

namespace mtsuite.shared.CommandLine {
  /// <summary>
  /// Base class of all command line argument definitions.
  /// </summary>
  public abstract class ArgDef {
    public string Id { get; set; }
    public string Description { get; set; }
    public bool IsMandatory { get; set; }
  }

  /// <summary>
  /// Base class for arguments which have a name (e.g. "/h", "/d:2").
  /// </summary>
  public abstract class NameArgDef : ArgDef {
    public string ShortName { get; set; }
    public string AltShortName { get; set; }
    public string LongName { get; set; }
  }

  /// <summary>
  /// Base class for arguments which have a name and a value (e.g. "/d:2").
  /// </summary>
  public abstract class NameValueArgDef : NameArgDef {
    public string ValueName { get; set; }
    public object DefaultValue { get; set; }
  }

  /// <summary>
  /// Definition for arguments specified as "free" string (e.g. a filename).
  /// </summary>
  public class FreeStringArgDef : ArgDef {
    public string DefaultValue { get; set; }
  }

  /// <summary>
  /// Definition for arguments which have a name and no value (e.g. "/h").
  /// </summary>
  public class SwitchArgDef : NameArgDef {
    public static object ValueMarker = new object();
  }

  /// <summary>
  /// Definition for arguments which have a name and an integer value (e.g. "/d:2").
  /// </summary>
  public class IntFlagArgDef : NameValueArgDef {
    public Func<int, string> Validator { get; set; }
  }

  /// <summary>
  /// Definition for arguments which have a name and a string value (e.g. "/a:foo").
  /// </summary>
  public class StringFlagArgDef : NameValueArgDef {
    public Func<string, string> Validator { get; set; }
  }
}