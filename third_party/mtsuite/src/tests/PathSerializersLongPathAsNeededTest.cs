using System.Linq;
using mtsuite.CoreFileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests {
  [TestClass]
  public class PathSerializersLongPathAsNeededTest {
    public static string LongPath = string.Concat(Enumerable.Repeat(@"\bar\blah", 50));

    [TestMethod]
    public void FullPathShouldWork() {
      var path = new FullPath(@"c:\foo");
      var serializer = new PathSerializers.LongPathAsNeededSerializer(); ;
      Assert.AreEqual(@"c:\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void FullPathShouldWork2() {
      var path = new FullPath(@"c:\foo" + LongPath);
      var serializer = new PathSerializers.LongPathAsNeededSerializer(); ;
      Assert.AreEqual(@"\\?\c:\foo" + LongPath, serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void LongPathShouldWork1() {
      var path = new FullPath(@"\\?\c:\foo");
      var serializer = new PathSerializers.LongPathAsNeededSerializer(); ;
      Assert.AreEqual(@"c:\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void LongPathShouldWork2() {
      var path = new FullPath(@"\\?\c:\foo" + LongPath);
      var serializer = new PathSerializers.LongPathAsNeededSerializer(); ;
      Assert.AreEqual(@"\\?\c:\foo" + LongPath, serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void UncPathShouldWork1() {
      var path = new FullPath(@"\\server\foo");
      var serializer = new PathSerializers.LongPathAsNeededSerializer();
      Assert.AreEqual(@"\\server\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void UncPathShouldWork2() {
      var path = new FullPath(@"\\server\foo" + LongPath);
      var serializer = new PathSerializers.LongPathAsNeededSerializer();
      Assert.AreEqual(@"\\?\UNC\server\foo" + LongPath, serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void LongUncPathShouldWork1() {
      var path = new FullPath(@"\\?\UNC\server\foo");
      var serializer = new PathSerializers.LongPathAsNeededSerializer();
      Assert.AreEqual(@"\\server\foo", serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }

    [TestMethod]
    public void LongUncPathShouldWork2() {
      var path = new FullPath(@"\\?\UNC\server\foo" + LongPath);
      var serializer = new PathSerializers.LongPathAsNeededSerializer();
      Assert.AreEqual(@"\\?\UNC\server\foo" + LongPath, serializer.GetText(path));
      Assert.AreEqual(serializer.GetText(path).Length, serializer.GetLength(path));
    }
  }
}