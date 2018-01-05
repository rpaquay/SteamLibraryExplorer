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

    private ICollection GetData(StringBuffer sb) {
      var items = new char[sb.Length + 1];
      Array.Copy(sb.Data, 0, items, 0, sb.Length + 1);
      return items;
    }
  }
}