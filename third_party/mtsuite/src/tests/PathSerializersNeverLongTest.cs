using mtsuite.CoreFileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests {
  [TestClass]
  public class PathSerializersNeverLongTest {
    [TestMethod]
    public void FullPathShouldWork() {
      var path = new FullPath(@"c:\foo");
      var serializer = new PathSerializers.NeverLongPathSerializer(); ;
      Assert.AreEqual(@"c:\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void LongPathShouldWork() {
      var path = new FullPath(@"\\?\c:\foo");
      var serializer = new PathSerializers.NeverLongPathSerializer(); ;
      Assert.AreEqual(@"c:\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void UncPathShouldWork() {
      var path = new FullPath(@"\\server\foo");
      var serializer = new PathSerializers.NeverLongPathSerializer();
      Assert.AreEqual(@"\\server\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void LongUncPathShouldWork() {
      var path = new FullPath(@"\\?\UNC\server\foo");
      var serializer = new PathSerializers.NeverLongPathSerializer();
      Assert.AreEqual(@"\\server\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }
  }
}