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
namespace mtsuite.shared.CommandLine {
  public class ParsedArgument {
    private readonly ArgDef _argDef;
    private readonly object _argValue;

    public ParsedArgument(ArgDef argDef, object argValue) {
      _argDef = argDef;
      _argValue = argValue;
    }

    public ArgDef ArgDef {
      get { return _argDef; }
    }

    public object ArgValue {
      get { return _argValue; }
    }

    public string StringValue {
      get { return (string)_argValue; }
    }

    public int IntValue {
      get { return (int)_argValue; }
    }
  }
}