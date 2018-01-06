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

using mtsuite.CoreFileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests {
  [TestClass]
  public class EnsureLongPathStringSourceFormatterTest {
    [TestMethod]
    public void DiskPathShouldWork() {
      var path = new FullPath(@"c:\foo");
      var formatter = new EnsureLongPathStringSourceFormatter();
      Assert.AreEqual(@"\\?\c:\foo", formatter.GetText(path));
    }

    [TestMethod]
    public void UncPathShouldWork() {
      var path = new FullPath(@"\\server\foo");
      var formatter = new EnsureLongPathStringSourceFormatter();
      Assert.AreEqual(@"\\?\UNC\server\foo", formatter.GetText(path));
    }
  }
}
