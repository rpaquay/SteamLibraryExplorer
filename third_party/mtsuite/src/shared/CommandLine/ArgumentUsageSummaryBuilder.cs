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

using System.Text;

namespace mtsuite.shared.CommandLine {
  public class ArgumentUsageSummaryBuilder : IArgumentDefinitionVisitor {
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

    private void Append(bool isMandatory, string format, params object[] args) {
      if (_sb.Length > 0)
        _sb.Append(' ');
      if (!isMandatory)
        _sb.Append('[');
      _sb.Append(string.Format(format, args));
      if (!isMandatory)
        _sb.Append(']');
    }

    public void Visit(FreeStringArgDef arg) {
      Append(arg.IsMandatory, "{0}", arg.Id);
    }

    public void Visit(SwitchArgDef arg) {
      Append(arg.IsMandatory, "/{0}", arg.ShortName);
    }

    public void Visit(IntFlagArgDef arg) {
      var valueSummary = string.Format(":{0}", arg.ValueName);
      Append(arg.IsMandatory, "/{0}{1}", arg.ShortName, valueSummary);
    }

    public void Visit(StringFlagArgDef arg) {
      var valueSummary = string.Format(":{0}", arg.ValueName);
      Append(arg.IsMandatory, "/{0}{1}", arg.ShortName, valueSummary);
    }
  }
}