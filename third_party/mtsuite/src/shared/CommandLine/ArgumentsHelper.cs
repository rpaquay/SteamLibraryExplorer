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
  public class ArgumentsHelper {
    public static string BuildUsageSummary(IList<ArgDef> argumentDefinitions) {
      var sb = new ArgumentUsageSummaryBuilder();
      foreach (var argDef in argumentDefinitions) {
        DispatchArgumentDefintion(argDef, sb);
      }
      return sb.Text;
    }

    public static void PrintArgumentUsageSummary(IList<ArgDef> argumentDefinitions) {
      foreach (var argDef in argumentDefinitions) {
        var sb = new ArgumentUsageBuilder();
        DispatchArgumentDefintion(argDef, sb);
        Console.WriteLine(sb.Text);
      }
    }

    public static void DispatchArgumentDefintion(ArgDef argDef, IArgumentDefinitionVisitor visitor) {
      var stringDef = argDef as FreeStringArgDef;
      if (stringDef != null) {
        visitor.Visit(stringDef);
        return;
      }

      var switchDef = argDef as SwitchArgDef;
      if (switchDef != null) {
        visitor.Visit(switchDef);
        return;
      }

      var intDef = argDef as IntFlagArgDef;
      if (intDef != null) {
        visitor.Visit(intDef);
        return;
      }

      var stringFlagDef = argDef as StringFlagArgDef;
      if (stringFlagDef != null) {
        visitor.Visit(stringFlagDef);
        return;
      }

      throw new ArgumentException("Unknown argument definition type", "argDef");
    }
  }
}