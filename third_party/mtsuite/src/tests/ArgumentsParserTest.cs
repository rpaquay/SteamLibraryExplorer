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
using mtsuite.shared.CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests {
  [TestClass]
  public class ArgumentsParserTest {
    [TestMethod]
    public void ArgumentsParserShouldWorkWithNoArgs() {
      var argumentDefinitions = new ArgDef[] {
        new SwitchArgDef {
          Id = "help",
          ShortName = "h",
          AltShortName = "?",
          LongName = "help",
        },
        new IntFlagArgDef {
          Id = "depth",
          ShortName = "d",
          LongName = "depth",
          DefaultValue = 2,
        },
        new StringFlagArgDef {
          Id = "attr",
          ShortName = "a",
          LongName = "attributes",
          DefaultValue = "foo",
        },
        new FreeStringArgDef {
          Id = "directory",
          DefaultValue = Environment.CurrentDirectory,
        }
      };

      var args = new string[] {
      };
      var parser = new ArgumentsParser(argumentDefinitions, args);
      parser.Parse();
      Assert.IsTrue(parser.IsValid);
    }

    [TestMethod]
    public void ArgumentsParserShouldWorkWithSwitch() {
      var argumentDefinitions = new ArgDef[] {
        new SwitchArgDef {
          Id = "help",
          ShortName = "h",
          AltShortName = "?",
          LongName = "help",
        },
        new IntFlagArgDef {
          Id = "depth",
          ShortName = "d",
          LongName = "depth",
          DefaultValue = 2,
        },
        new StringFlagArgDef {
          Id = "attr",
          ShortName = "a",
          LongName = "attributes",
          DefaultValue = "foo",
        },
        new FreeStringArgDef {
          Id = "directory",
          DefaultValue = Environment.CurrentDirectory,
        }
      };

      var args = new[] { "/?" };
      var parser = new ArgumentsParser(argumentDefinitions, args);
      parser.Parse();
      Assert.IsTrue(parser.IsValid);
      Assert.IsTrue(parser.Contains("help"));
    }

    [TestMethod]
    public void ArgumentsParserShouldWorkWithDefaultValues() {
      var argumentDefinitions = new ArgDef[] {
        new SwitchArgDef {
          Id = "help",
          ShortName = "h",
          AltShortName = "?",
          LongName = "help",
        },
        new IntFlagArgDef {
          Id = "depth",
          ShortName = "d",
          LongName = "depth",
          DefaultValue = 2,
        },
        new StringFlagArgDef {
          Id = "attr",
          ShortName = "a",
          LongName = "attributes",
          DefaultValue = "foo",
        },
        new FreeStringArgDef {
          Id = "directory",
          DefaultValue = "defaultDir",
        }
      };

      var args = new string[] { };
      var parser = new ArgumentsParser(argumentDefinitions, args);
      parser.Parse();
      Assert.IsTrue(parser.IsValid);
      Assert.IsTrue(parser.Contains("directory"));
      Assert.IsTrue(parser.Contains("depth"));
      Assert.AreEqual(2, parser["depth"].IntValue);
      Assert.IsTrue(parser.Contains("attr"));
      Assert.AreEqual("foo", parser["attr"].StringValue);
      Assert.AreEqual("defaultDir", parser["directory"].StringValue);
    }

    [TestMethod]
    public void ArgumentsParserShouldWorkWithDefaultValueForMissingValue() {
      var argumentDefinitions = new ArgDef[] {
        new IntFlagArgDef {
          Id = "depth",
          ShortName = "d",
          LongName = "depth",
          DefaultValue = 2,
        },
      };

      var args = new string[] { "/d" };
      var parser = new ArgumentsParser(argumentDefinitions, args);
      parser.Parse();
      Assert.IsTrue(parser.IsValid);
      Assert.IsTrue(parser.Contains("depth"));
      Assert.AreEqual(2, parser["depth"].IntValue);
    }

    [TestMethod]
    public void ArgumentsParserShouldWorkWithDefaultMandatoryArguments() {
      var argumentDefinitions = new ArgDef[] {
        new FreeStringArgDef {
          Id = "directory",
          DefaultValue = "defaultDir",
          IsMandatory = true
        }
      };

      var args = new string[] { };
      var parser = new ArgumentsParser(argumentDefinitions, args);
      parser.Parse();
      Assert.IsTrue(parser.IsValid);
      Assert.IsTrue(parser.Contains("directory"));
      Assert.AreEqual("defaultDir", parser["directory"].StringValue);
    }

    [TestMethod]
    public void ArgumentsParserShouldWorkWithMissingMandatoryArguments() {
      var argumentDefinitions = new ArgDef[] {
        new FreeStringArgDef {
          Id = "directory",
          IsMandatory = true
        }
      };

      var args = new string[] { };
      var parser = new ArgumentsParser(argumentDefinitions, args);
      parser.Parse();
      Assert.IsFalse(parser.IsValid);
    }
  }
}
