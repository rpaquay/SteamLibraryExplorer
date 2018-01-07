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
  public class PathSerializersAlwaysLongPathTest {
    [TestMethod]
    public void FullPathShouldWork() {
      var path = new FullPath(@"c:\foo");
      var serializer = new PathSerializers.AlwaysLongPathSerializer();;
      Assert.AreEqual(@"\\?\c:\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void LongPathShouldWork() {
      var path = new FullPath(@"\\?\c:\foo");
      var serializer = new PathSerializers.AlwaysLongPathSerializer(); ;
      Assert.AreEqual(@"\\?\c:\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void UncPathShouldWork() {
      var path = new FullPath(@"\\server\foo");
      var serializer = new PathSerializers.AlwaysLongPathSerializer();
      Assert.AreEqual(@"\\?\UNC\server\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void LongUncPathShouldWork() {
      var path = new FullPath(@"\\?\UNC\server\foo");
      var serializer = new PathSerializers.AlwaysLongPathSerializer();
      Assert.AreEqual(@"\\?\UNC\server\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }
  }
}
