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
using System.Text;

namespace mtsuite.shared.CommandLine {
  public class ArgumentUsageBuilder : IArgumentDefinitionVisitor {
    private readonly StringBuilder _sb = new StringBuilder();

    public string Text {
      get { return _sb.ToString(); }
    }

    public override string ToString() {
      return Text;
    }

    private void Append(string format, params object[] args) {
      if (_sb.Length > 0)
        _sb.Append(' ');
      _sb.Append(string.Format(format, args));
    }

    public void Visit(FreeStringArgDef arg) {
      Append("  {0,-20} {1}", arg.Id, FormatMultiLine(arg.Description, 23));
    }

    public void Visit(SwitchArgDef arg) {
      Append("  /{0,-19} {1}", arg.ShortName, FormatMultiLine(arg.Description, 23));
    }

    public void Visit(IntFlagArgDef arg) {
      var valueSummary = string.Format("/{0}:{1}", arg.ShortName, arg.ValueName);
      Append("  {0,-20} {1}", valueSummary, FormatMultiLine(arg.Description, 23));
    }

    public void Visit(StringFlagArgDef arg) {
      var valueSummary = string.Format("/{0}:{1}", arg.ShortName, arg.ValueName);
      Append("  {0,-20} {1}", valueSummary, FormatMultiLine(arg.Description, 23));
    }

    private static string FormatMultiLine(string arg, int indent) {
      var sb = new StringBuilder();
      int index = 0;
      foreach (var line in SplitLines(arg)) {
        if (index > 0) {
          sb.AppendLine();
          sb.Append(new string(' ', indent));
        }
        sb.Append(line);
        index++;
      }
      return sb.ToString();
    }

    private static IEnumerable<string> SplitLines(string value) {
      while (true) {
        int index = value.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase);
        if (index < 0) {
          yield return value;
          yield break;
        }

        var current = value.Substring(0, index);
        yield return current;
        value = value.Substring(index + Environment.NewLine.Length);
      }
    }
  }
}