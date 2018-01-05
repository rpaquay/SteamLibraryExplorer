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
using mtsuite.CoreFileSystem.Win32;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests {
  [TestClass]
  public class Win32Test {
    [TestMethod]
    public void LongToHighLowTest() {
      uint high;
      uint low;
        
      NativeMethods.LongToHighLow(0x1122334455667788, out high, out low);
      Assert.AreEqual<uint>(0x11223344, high);
      Assert.AreEqual<uint>(0x55667788, low);

      NativeMethods.LongToHighLow(unchecked((long)0xfffefdfc00000000), out high, out low);
      Assert.AreEqual<uint>(0xfffefdfc, high);
      Assert.AreEqual<uint>(0x00000000, low);
    }

    [TestMethod]
    public void HighLowToLongTest() {
      Assert.AreEqual(0x1122334455667788, NativeMethods.HighLowToLong(0x11223344, 0x55667788));
      Assert.AreEqual(unchecked((long)0xfffefdfc00100000), NativeMethods.HighLowToLong(0xfffefdfc, 0x00100000));
    }

    [TestMethod]
    public void DateTimeTest() {
      var dateTime = DateTime.UtcNow;
      NativeMethods.FILETIME ft = NativeMethods.FILETIME.FromDateTime(dateTime);
      Assert.AreEqual(dateTime, ft.ToDateTimeUtc());
    }
  }
}