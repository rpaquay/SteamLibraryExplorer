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

using System.Collections.Generic;
using System.Linq;
using mtsuite.shared.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests {
  [TestClass]
  public class SmallSetTest {
    private class Entry {
      private readonly int _id;

      public Entry(int id) {
        _id = id;
      }

      public int Id {
        get { return _id; }
      }
    }

    private class EntryComparer : IEqualityComparer<Entry> {
      public bool Equals(Entry x, Entry y) {
        return x.Id == y.Id;
      }

      public int GetHashCode(Entry obj) {
        return obj.Id.GetHashCode();
      }
    }

    [TestCleanup]
    public void Setup() {
    }

    [TestMethod]
    public void SmallSetShouldWorkForSmallSets() {
      var count = SmallSet<Entry>.Threshold - 1;
      var entries = Enumerable.Range(0, count - 1).Select(x => new Entry(x)).ToList();
      var set = new SmallSet<Entry>(entries, new EntryComparer());

      Assert.IsTrue(set.TryGet(new Entry(10)).Key);
      Assert.AreSame(entries[10], set.TryGet(new Entry(10)).Value);
    }


    [TestMethod]
    public void SmallSetShouldWorkForBigSets() {
      var count = SmallSet<Entry>.Threshold * 2;
      var entries = Enumerable.Range(0, count - 1).Select(x => new Entry(x)).ToList();
      var set = new SmallSet<Entry>(entries, new EntryComparer());

      Assert.IsTrue(set.TryGet(new Entry(10)).Key);
      Assert.AreSame(entries[10], set.TryGet(new Entry(10)).Value);
    }
  }
}