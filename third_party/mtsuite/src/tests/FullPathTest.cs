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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using mtsuite.CoreFileSystem;
using mtsuite.CoreFileSystem.Utils;

namespace tests {
  [TestClass]
  public class FullPathTest {
    [TestMethod]
    public void FullPathWithAbsolutePathShouldWork() {
      var pathString = @"c:\test\test2";
      var path = new FullPath(pathString);
      Assert.AreEqual(pathString, path.FullName);
    }

    [TestMethod]
    public void FullPathWithTrailingSeparatorShouldWork() {
      var pathString = @"c:\test\test2\";
      var path = new FullPath(pathString);
      Assert.AreEqual(pathString, path.FullName);
    }

    [TestMethod]
    public void FullPathCombineShouldWork() {
      var pathString = @"c:\test\test2";
      var path = new FullPath(pathString).Combine("a");
      Assert.AreEqual(pathString + @"\a", path.FullName);
    }

    [TestMethod]
    public void FullPathWithTrailingSeparatorCombineShouldWork() {
      var pathString = @"c:\test\test2\";
      var path = new FullPath(pathString).Combine("a");
      Assert.AreEqual(pathString + "a", path.FullName);
    }

    [TestMethod]
    public void FullPathMultipleCombineShouldWork() {
      var pathString = @"c:\test\test2";
      var path = new FullPath(pathString).Combine("a").Combine("tt");
      Assert.AreEqual(pathString + @"\a" + @"\tt", path.FullName);
    }

    [TestMethod]
    public void FullPathGetNameShouldWork() {
      var pathString = @"c:\test\test2";
      var path = new FullPath(pathString);
      Assert.AreEqual(@"test2", path.Name);
    }

    [TestMethod]
    public void FullPathParentShouldWork() {
      var pathString = @"c:\test\test2";
      var path = new FullPath(pathString);
      Assert.IsNotNull(path.Parent);
      Assert.AreEqual(@"c:\test\", path.Parent.FullName);
      Assert.IsNotNull(path.Parent.Parent);
      Assert.AreEqual(@"c:\", path.Parent.Parent.FullName);
      Assert.IsNull(path.Parent.Parent.Parent);
    }

    [TestMethod]
    public void FullPathWithTrailingSeparatorParentShouldWork() {
      var pathString = @"c:\test\test2\";
      var path = new FullPath(pathString);
      Assert.IsNotNull(path.Parent);
      Assert.AreEqual(@"c:\test\", path.Parent.FullName);
      Assert.IsNotNull(path.Parent.Parent);
      Assert.AreEqual(@"c:\", path.Parent.Parent.FullName);
      Assert.IsNull(path.Parent.Parent.Parent);
    }

    [TestMethod]
    public void FullPathLengthShouldWork() {
      var pathString = @"c:\test\test2";
      var path = new FullPath(pathString).Combine("a").Combine("tt");
      Assert.AreEqual(pathString + @"\a" + @"\tt", path.FullName);
      Assert.AreEqual(path.FullName.Length, path.Length);
      Assert.AreEqual(18, path.Length);
    }

    [TestMethod]
    public void FullPathCopyToShouldWork() {
      var pathString = @"c:\test\test2";
      var path = new FullPath(pathString).Combine("a").Combine("tt");
      var sb = new StringBuffer();
      path.CopyTo(sb);
      Assert.AreEqual(pathString + @"\a" + @"\tt", sb.Text);
    }

    [TestMethod]
    public void FullPathToStringShouldWork() {
      var pathString = @"c:\test\test2";
      var path = new FullPath(pathString).Combine("a").Combine("tt");
      Assert.AreEqual(pathString + @"\a" + @"\tt", path.ToString());
    }
  }
}
