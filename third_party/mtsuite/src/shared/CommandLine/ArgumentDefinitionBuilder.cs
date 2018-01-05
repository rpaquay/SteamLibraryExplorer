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

namespace mtsuite.shared.CommandLine {
  public class ArgumentDefinitionBuilder {
    private readonly List<ArgDef> _definitions = new List<ArgDef>();

    public IList<ArgDef> Build() {
      return _definitions;
    }

    public ArgumentDefinitionBuilder WithString(string id, string description, bool isMandatory, string defaultValue = null) {
      var def = new FreeStringArgDef {
        Id = id,
        Description = description,
        IsMandatory = isMandatory,
        DefaultValue = defaultValue,
      };
      _definitions.Add(def);
      return this;
    }

    public ArgumentDefinitionBuilder WithSwitch(string id, string description, string shortName, string altShortName = "", string longName = "") {
      var def = new SwitchArgDef {
        Id = id,
        Description = description,
        ShortName = shortName,
        AltShortName = altShortName,
        LongName = longName,
      };
      _definitions.Add(def);
      return this;
    }

    public ArgumentDefinitionBuilder WithIntFlag(string id, string description, string shortName, string valueName, int defaultValue, Func<int, string> validator = null, string altShortName = "", string longName = "") {
      var def = new IntFlagArgDef {
        Id = id,
        Description = description,
        ShortName = shortName,
        ValueName = valueName,
        DefaultValue = defaultValue,
        Validator = validator,
        AltShortName = altShortName,
        LongName = longName,
      };
      _definitions.Add(def);
      return this;
    }

    public ArgumentDefinitionBuilder WithStringFlag(string id, string description, string shortName, string valueName, string defaultValue, Func<string, string> validator = null, string altShortName = "", string longName = "") {
      var def = new StringFlagArgDef {
        Id = id,
        Description = description,
        ShortName = shortName,
        ValueName = valueName,
        DefaultValue = defaultValue,
        Validator = validator,
        AltShortName = altShortName,
        LongName = longName,
      };
      _definitions.Add(def);
      return this;
    }

    public ArgumentDefinitionBuilder WithHelpSwitch() {
      return WithSwitch("help", "Display help", "h", "?", "help");
    }

    public ArgumentDefinitionBuilder WithGcSwitch() {
      return WithSwitch("gc", "Display .NET Garbage Collector statistics", "gc");
    }

    public ArgumentDefinitionBuilder WithThreadCountSwitch() {
      return WithIntFlag(
        "thread-count",
        "Determine the # of concurrent threads (minimum=1, default=# of CPU cores)", "t", "count",
        -1,
        value => {
          if (value < 1)
            return "Thread count must be greater or equal to 1";
          return null;
        },
        "",
        "threads");
    }
  }
}