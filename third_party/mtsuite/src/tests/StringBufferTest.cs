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
using mtsuite.CoreFileSystem.Utils;
using System;
using System.Collections;

namespace tests {
  [TestClass]
  public class StringBufferTest {
    [TestMethod]
    public void LengthTest1() {
      var sb = new StringBuffer(10);
      Assert.AreEqual(0, sb.Length);
    }

    [TestMethod]
    public void LengthTest2() {
      var sb = new StringBuffer(10);
      sb.Append('a');
      Assert.AreEqual(1, sb.Length);
    }

    [TestMethod]
    public void AppendTest() {
      var sb = new StringBuffer(10);
      sb.Append('a');
      Assert.AreEqual("a", sb.ToString());
    }

    [TestMethod]
    public void Append2Test() {
      var sb = new StringBuffer(10);
      sb.Append('a');
      sb.Append("abc");
      Assert.AreEqual("aabc", sb.ToString());
    }

    [TestMethod]
    public void Append3Test() {
      var sb = new StringBuffer(10);
      Assert.AreEqual("", sb.ToString());
    }

    [TestMethod]
    public void Append4Test() {
      var sb = new StringBuffer(10);
      sb.Append("");
      Assert.AreEqual("", sb.ToString());
    }

    [TestMethod]
    public void Append5Test() {
      var sb = new StringBuffer(10);
      CollectionAssert.AreEqual(new[] { '\0' }, GetData(sb));
    }

    [TestMethod]
    public void Append6Test() {
      var sb = new StringBuffer(10);
      sb.Append("abc");
      CollectionAssert.AreEqual(new[] { 'a', 'b', 'c', '\0' }, GetData(sb));
    }

    [TestMethod]
    public void Insert1Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.InsertAt(0, 'A');
      Assert.AreEqual("Aabcde", sb.ToString());
      Assert.AreEqual(6, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Insert2Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.InsertAt(2, 'A');
      Assert.AreEqual("abAcde", sb.ToString());
      Assert.AreEqual(6, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Insert3Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.InsertAt(5, 'A');
      Assert.AreEqual("abcdeA", sb.ToString());
      Assert.AreEqual(6, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Insert4Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.InsertAt(0, "FOO");
      Assert.AreEqual("FOOabcde", sb.ToString());
      Assert.AreEqual(8, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Insert5Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.InsertAt(2, "FOO");
      Assert.AreEqual("abFOOcde", sb.ToString());
      Assert.AreEqual(8, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Insert6Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.InsertAt(5, "FOO");
      Assert.AreEqual("abcdeFOO", sb.ToString());
      Assert.AreEqual(8, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Delete1Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.DeleteAt(0, 1);
      Assert.AreEqual("bcde", sb.ToString());
      Assert.AreEqual(4, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Delete2Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.DeleteAt(0, 5);
      Assert.AreEqual("", sb.ToString());
      Assert.AreEqual(0, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Delete3Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.DeleteAt(4, 1);
      Assert.AreEqual("abcd", sb.ToString());
      Assert.AreEqual(4, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    [TestMethod]
    public void Delete4Test() {
      var sb = new StringBuffer(10);
      sb.Append("abcde");
      sb.DeleteAt(5, 0);
      Assert.AreEqual("abcde", sb.ToString());
      Assert.AreEqual(5, sb.Length);
      Assert.AreEqual(0, sb.Data[sb.Length]);
    }

    private ICollection GetData(StringBuffer sb) {
      var items = new char[sb.Length + 1];
      Array.Copy(sb.Data, 0, items, 0, sb.Length + 1);
      return items;
    }
  }
}