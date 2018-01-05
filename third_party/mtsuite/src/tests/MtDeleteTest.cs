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
using mtdel;
using mtsuite.shared.CommandLine;
using tests.FileSystemHelpers;

namespace tests {
  [TestClass]
  public class MtDeleteTest {
    private FileSystemSetup _fileSystemSetup;

    [TestInitialize]
    public void Setup() {
      _fileSystemSetup = new FileSystemSetup();
    }

    [TestCleanup]
    public void Cleanup() {
      _fileSystemSetup.Dispose();
      _fileSystemSetup = null;
    }

    [TestMethod]
    [ExpectedException(typeof(CommandLineReturnValueException))]
    public void MtDeleteShouldThrowWithNonExistingFolder() {
      var mtdelete = new MtDelete(_fileSystemSetup.FileSystem);
      mtdelete.DoDelete(_fileSystemSetup.Root.Path.Combine("fake"), new MtDelete.Options { QuietMode = true });
    }

    [TestMethod]
    public void MtDeleteShouldWorkWithEmptyFolder() {
      var mtdelete = new MtDelete(_fileSystemSetup.FileSystem);

      var stats = mtdelete.DoDelete(_fileSystemSetup.Root.Path, new MtDelete.Options { QuietMode = true });
      Assert.IsFalse(_fileSystemSetup.Root.Exists());
      Assert.AreEqual(1, stats.DirectoryDeletedCount);
    }

    [TestMethod]
    public void MtDeleteShouldWorkWithFiles() {
      _fileSystemSetup.Root.CreateFile("a", 10);
      _fileSystemSetup.Root.CreateFile("b", 11);
      _fileSystemSetup.Root.CreateFile("c", 12);

      var mtdelete = new MtDelete(_fileSystemSetup.FileSystem);

      var stats = mtdelete.DoDelete(_fileSystemSetup.Root.Path, new MtDelete.Options { QuietMode = true });
      Assert.IsFalse(_fileSystemSetup.Root.Exists());
      Assert.AreEqual(1, stats.DirectoryDeletedCount);
      Assert.AreEqual(3, stats.FileDeletedCount);
    }

    [TestMethod]
    public void MtDeleteShouldWorkWithDirectories() {
      var dir1 = _fileSystemSetup.Root.CreateDirectory("a");
      dir1.CreateFile("a", 10);
      dir1.CreateFile("b", 11);
      dir1.CreateFile("c", 12);
      var dir2 = _fileSystemSetup.Root.CreateDirectory("b");
      dir2.CreateFile("b", 11);
      dir2.CreateFile("c", 12);

      var mtdelete = new MtDelete(_fileSystemSetup.FileSystem);

      var stats = mtdelete.DoDelete(_fileSystemSetup.Root.Path, new MtDelete.Options { QuietMode = true });
      Assert.IsFalse(_fileSystemSetup.Root.Exists());
      Assert.AreEqual(3, stats.DirectoryDeletedCount);
      Assert.AreEqual(5, stats.FileDeletedCount);
    }

    [TestMethod]
    public void MtDeleteShouldWorkWithNestedDirectories() {
      _fileSystemSetup.Root.CreateDirectory("a").CreateDirectory("b").CreateDirectory("c").CreateDirectory("d");

      var mtdelete = new MtDelete(_fileSystemSetup.FileSystem);

      var stats = mtdelete.DoDelete(_fileSystemSetup.Root.Path, new MtDelete.Options { QuietMode = true });
      Assert.IsFalse(_fileSystemSetup.Root.Exists());
      Assert.AreEqual(5, stats.DirectoryDeletedCount);
    }

    [TestMethod]
    public void MtDeleteShouldDeleteReadOnlyFiles() {
      var file = _fileSystemSetup.Root.CreateFile("a", 10);
      file.SetReadOnlyAttribute();

      var mtdelete = new MtDelete(_fileSystemSetup.FileSystem);

      var stats = mtdelete.DoDelete(_fileSystemSetup.Root.Path, new MtDelete.Options { QuietMode = true });
      Assert.IsFalse(_fileSystemSetup.Root.Exists());
      Assert.AreEqual(0, stats.Errors.Count);
      Assert.AreEqual(1, stats.DirectoryDeletedCount);
      Assert.AreEqual(1, stats.FileDeletedCount);
    }

    [TestMethod]
    public void MtDeleteShouldDeleteSystemFiles() {
      var file = _fileSystemSetup.Root.CreateFile("a", 10);
      file.SetSystemAttribute();

      var mtdelete = new MtDelete(_fileSystemSetup.FileSystem);

      var stats = mtdelete.DoDelete(_fileSystemSetup.Root.Path, new MtDelete.Options { QuietMode = true });
      Assert.IsFalse(_fileSystemSetup.Root.Exists());
      Assert.AreEqual(0, stats.Errors.Count);
      Assert.AreEqual(1, stats.DirectoryDeletedCount);
      Assert.AreEqual(1, stats.FileDeletedCount);
    }
  }
}