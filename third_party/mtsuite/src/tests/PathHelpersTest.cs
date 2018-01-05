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
  public class PathHelpersTest {
    [TestMethod]
    public void IsPathAbsoluteTest() {
      Assert.IsTrue(PathHelpers.IsPathAbsolute(@"c:\"));
      Assert.IsTrue(PathHelpers.IsPathAbsolute(@"c:\gfdfg"));
      Assert.IsTrue(PathHelpers.IsPathAbsolute(@"\\gfdfg"));
      Assert.IsTrue(PathHelpers.IsPathAbsolute(@"\\gfdfg\fd"));
      Assert.IsTrue(PathHelpers.IsPathAbsolute(@"\\?\C:\dfdsf"));
      Assert.IsTrue(PathHelpers.IsPathAbsolute(@"\\?\UNC\fdsdf"));
      Assert.IsTrue(PathHelpers.IsPathAbsolute(@"\\?\unc\fdsdf"));

      Assert.IsFalse(PathHelpers.IsPathAbsolute(@"c:"));
      Assert.IsFalse(PathHelpers.IsPathAbsolute(@"c:fdsdf"));
      Assert.IsFalse(PathHelpers.IsPathAbsolute(@"\fdsdf"));
      Assert.IsFalse(PathHelpers.IsPathAbsolute(@"\\?\C:dfdsf"));
      Assert.IsFalse(PathHelpers.IsPathAbsolute(@"\\?\c:fdsdf"));
      Assert.IsFalse(PathHelpers.IsPathAbsolute(@"\\?\\fdsdf"));
    }

    [TestMethod]
    public void MakeLongPathTest() {
      Assert.AreEqual(@"\\?\c:\", PathHelpers.MakeLongPath(@"c:\"));
      Assert.AreEqual(@"\\?\UNC\server\share", PathHelpers.MakeLongPath(@"\\server\share"));
    }

    [TestMethod]
    public void NormalizePathTest() {
      Assert.AreEqual(@"c:\", PathHelpers.NormalizePath(@"c:\test\.."));
      Assert.AreEqual(@"\\?\c:\", PathHelpers.NormalizePath(@"\\?\c:\test\.."));
      Assert.AreEqual(@"\\server\share", PathHelpers.NormalizePath(@"\\server\share"));
      Assert.AreEqual(@"\\?\UNC\server\share", PathHelpers.NormalizePath(@"\\?\UNC\server\share"));
      Assert.AreEqual(@"\\server\share", PathHelpers.NormalizePath(@"\\server\share\."));
      Assert.AreEqual(@"\\?\UNC\server\share", PathHelpers.NormalizePath(@"\\?\UNC\server\share\."));
      Assert.AreEqual(@"\\server", PathHelpers.NormalizePath(@"\\server\share\.."));
      Assert.AreEqual(@"\\?\UNC\server", PathHelpers.NormalizePath(@"\\?\UNC\server\share\.."));
    }

    [TestMethod]
    public void StripLongPathPrefixTest() {
      Assert.AreEqual(@"c:\test", PathHelpers.StripLongPathPrefix(@"c:\test"));
      Assert.AreEqual(@"c:\test", PathHelpers.StripLongPathPrefix(@"\\?\c:\test"));
      Assert.AreEqual(@"\\server\share", PathHelpers.StripLongPathPrefix(@"\\server\share"));
      Assert.AreEqual(@"\\server\share", PathHelpers.StripLongPathPrefix(@"\\?\UNC\server\share"));
    }

    [TestMethod]
    public void GetParentTest() {
      Assert.AreEqual(@"c:\", PathHelpers.GetParent(@"c:\test"));
      Assert.AreEqual(@"\\?\c:\", PathHelpers.GetParent(@"\\?\c:\test"));
      Assert.AreEqual(@"\\server\", PathHelpers.GetParent(@"\\server\share"));
      Assert.AreEqual(@"\\?\UNC\server\", PathHelpers.GetParent(@"\\?\UNC\server\share"));

      Assert.AreEqual(@"c:\", PathHelpers.GetParent(@"c:\test\"));
      Assert.AreEqual(@"\\?\c:\", PathHelpers.GetParent(@"\\?\c:\test\"));
      Assert.AreEqual(@"\\server\", PathHelpers.GetParent(@"\\server\share\"));
      Assert.AreEqual(@"\\?\UNC\server\", PathHelpers.GetParent(@"\\?\UNC\server\share\"));

      Assert.AreEqual(@"", PathHelpers.GetParent(@"c:\"));
      Assert.AreEqual(@"", PathHelpers.GetParent(@"\\?\c:\"));
      Assert.AreEqual(@"", PathHelpers.GetParent(@"\\server\"));
      Assert.AreEqual(@"", PathHelpers.GetParent(@"\\?\UNC\server\"));
    }

    [TestMethod]
    public void GetFileNameTest() {
      Assert.AreEqual(@"test", PathHelpers.GetName(@"c:\test"));
      Assert.AreEqual(@"test", PathHelpers.GetName(@"\\?\c:\test"));
      Assert.AreEqual(@"share", PathHelpers.GetName(@"\\server\share"));
      Assert.AreEqual(@"share", PathHelpers.GetName(@"\\?\UNC\server\share"));

      Assert.AreEqual(@"test", PathHelpers.GetName(@"c:\test\"));
      Assert.AreEqual(@"test", PathHelpers.GetName(@"\\?\c:\test\"));
      Assert.AreEqual(@"share", PathHelpers.GetName(@"\\server\share\"));
      Assert.AreEqual(@"share", PathHelpers.GetName(@"\\?\UNC\server\share\"));

      Assert.AreEqual(@"", PathHelpers.GetName(@"c:\"));
      Assert.AreEqual(@"", PathHelpers.GetName(@"\\?\c:\"));
      Assert.AreEqual(@"", PathHelpers.GetName(@"\\server\"));
      Assert.AreEqual(@"", PathHelpers.GetName(@"\\?\UNC\server\"));
    }

    [TestMethod]
    public void NormalizeUserInputPathTest() {
      Assert.AreEqual(@"c:\test", PathHelpers.NormalizeUserInputPath(@"c:\", @"c:\test"));
      Assert.AreEqual(@"c:\test", PathHelpers.NormalizeUserInputPath(@"c:\", @"\test"));
      Assert.AreEqual(@"c:\test", PathHelpers.NormalizeUserInputPath(@"c:\", @"test\"));
      Assert.AreEqual(@"c:\", PathHelpers.NormalizeUserInputPath(@"c:\", @"c:"));
      Assert.AreEqual(@"d:\", PathHelpers.NormalizeUserInputPath(@"c:\", @"d:"));
    }
  }
}
