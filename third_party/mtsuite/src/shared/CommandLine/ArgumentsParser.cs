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
using System.Linq;

namespace mtsuite.shared.CommandLine {
  public class ArgumentsParser {
    private readonly IList<ArgDef> _argumentDefinitions;
    private readonly IList<string> _args;
    private readonly IList<ParsedArgument> _parserArguments = new List<ParsedArgument>();
    private readonly IList<string> _errors = new List<string>();

    public ArgumentsParser(IList<ArgDef> argumentDefinitions, string[] args) {
      _argumentDefinitions = argumentDefinitions;
      _args = args;
    }

    public bool IsValid {
      get { return _errors.Count == 0; }
    }

    public IList<string> Errors {
      get { return _errors; }
    }

    public void Parse() {
      for (var index = 0; index < _args.Count; ) {
        // If the argument a named argument?
        var argString = _args[index];
        if (argString.StartsWith("/") || argString.StartsWith("-") || argString.StartsWith("--")) {
          var prefixCount = argString.StartsWith("--") ? 2 : 1;
          var valueIndex = argString.IndexOf(':', prefixCount);
          if (valueIndex == argString.Length - 1) {
            _errors.Add("An argument value is required after ':'");
            index++;
            continue;
          }
          var argName = (valueIndex < 0
            ? argString.Substring(prefixCount)
            : argString.Substring(prefixCount, valueIndex - prefixCount));
          var argStringValue = (valueIndex < 0 ? "" : argString.Substring(valueIndex + 1));
          var argDef = FindNamedArgument(argName);
          if (argDef == null) {
            _errors.Add(string.Format("Unknown argument \"{0}\"", argString));
            index++;
            continue;
          }
          object argValue = FindArgumentValue(argString, argDef, argStringValue);
          if (argValue == null) {
            index++;
            continue;
          }

          var parsedArg = new ParsedArgument(argDef, argValue);
          _parserArguments.Add(parsedArg);
          index++;
        } else {
          // Handle free string arguments
          var freeArgCount = _parserArguments.Count(x => x.ArgDef is FreeStringArgDef);
          var argDef = FindFreeArgDef(freeArgCount);
          if (argDef == null) {
            _errors.Add(string.Format("Extra argument \"{0}\"", argString));
            index++;
            continue;
          }
          var parsedArg = new ParsedArgument(argDef, argString);
          _parserArguments.Add(parsedArg);
          index++;
        }
      }

      if (_errors.Count == 0) {
        AddMissingDefaultValues();
        CheckMissingManadatoryArguments();
      }
    }

    private void CheckMissingManadatoryArguments() {
      foreach (var argDef in _argumentDefinitions.Where(x => x.IsMandatory)) {
        if (!Contains(argDef.Id)) {
          _errors.Add(string.Format("Missing argument \"{0}\"", argDef.Id));
        }
      }
    }

    private void AddMissingDefaultValues() {
      var namedDefaults = _argumentDefinitions.OfType<NameValueArgDef>()
        .Where(x => !Contains(x.Id) && x.DefaultValue != null);
      foreach (var x in namedDefaults) {
        _parserArguments.Add(new ParsedArgument(x, x.DefaultValue));
      }

      var stringDefaults = _argumentDefinitions.OfType<FreeStringArgDef>()
        .Where(x => !Contains(x.Id) && x.DefaultValue != null);
      foreach (var x in stringDefaults) {
        _parserArguments.Add(new ParsedArgument(x, x.DefaultValue));
      }
    }

    private FreeStringArgDef FindFreeArgDef(int index) {
      int current = 0;
      foreach (var argDef in _argumentDefinitions.OfType<FreeStringArgDef>()) {
        if (index == current)
          return argDef;
        current++;
      }
      return null;
    }

    private object FindArgumentValue(string argString, NameArgDef argDef, string argValue) {
      var swtichDef = argDef as SwitchArgDef;
      if (swtichDef != null) {
        if (!string.IsNullOrEmpty(argValue)) {
          _errors.Add(String.Format("Argument \"{0}\" does not take values", argString));
          return null;
        }

        return SwitchArgDef.ValueMarker;
      }

      var valueArgDef = argDef as NameValueArgDef;
      if (valueArgDef != null) {
        if (string.IsNullOrEmpty(argValue)) {
          // Note: If there is no explicit value, we'll use the default value.
          if (valueArgDef.DefaultValue == null) {
            _errors.Add(String.Format("Argument \"{0}\" requires a value", argString));
            return null;
          }
        }

        var intDef = valueArgDef as IntFlagArgDef;
        if (intDef != null) {
          // Parse argument value (or use default value)
          int value;
          if (string.IsNullOrEmpty(argValue)) {
            value = (int)intDef.DefaultValue;
          } else if (!int.TryParse(argValue, out value)) {
            _errors.Add(String.Format("Argument \"{0}\" requires a interger value", argString));
            return null;
          }
          if (intDef.Validator != null) {
            var error = intDef.Validator(value);
            if (!string.IsNullOrEmpty(error)) {
              _errors.Add(error);
              return null;
            }
          }
          return value;
        }

        var stringDef = valueArgDef as StringFlagArgDef;
        if (stringDef != null) {
          if (string.IsNullOrEmpty(argValue)) {
            argValue = (string)stringDef.DefaultValue;
          }
          if (stringDef.Validator != null) {
            var error = stringDef.Validator(argValue);
            if (!string.IsNullOrEmpty(error)) {
              _errors.Add(error);
              return null;
            }
          }
          return argValue;
        }

        return argValue;
      }

      _errors.Add(String.Format("Argument \"{0}\" not recognized", argString));
      return null;
    }

    private NameArgDef FindNamedArgument(string name) {
      return _argumentDefinitions
        .OfType<NameArgDef>()
        .Where(x => {
          return (x.ShortName == name || x.AltShortName == name || x.LongName == name);
        }).SingleOrDefault();
    }

    public bool Contains(string id) {
      return _parserArguments.Any(x => x.ArgDef.Id == id);
    }

    public ParsedArgument this[string id] {
      get {
        return _parserArguments.Single(x => x.ArgDef.Id == id);
      }
    }
  }
}