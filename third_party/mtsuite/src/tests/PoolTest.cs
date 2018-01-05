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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using mtsuite.CoreFileSystem.ObjectPool;
using mtsuite.shared.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests {
  [TestClass]
  public class PoolTest {
    private class Entry {
      public static int GlobalId;
      /// <summary>
      /// Big element to create GC pressure
      /// </summary>
      //public byte[] _elements = new byte[1024 * 1];

      public Entry() {
        Interlocked.Increment(ref GlobalId);
      }
    }

    [TestMethod]
    public void FixedSizeArrayPoolShouldNotAllocateTooMuch() {
      var pool = new ConcurrentFixedSizeArrayPool<Entry>(() => new Entry(), _ => { }, Environment.ProcessorCount);
      RunPoolTest(pool, _ => Entry.GlobalId <= Environment.ProcessorCount);
    }

    private void RunPoolTest<T>(IPool<T> pool, Func<long, bool> verify) where T : class {
      Enumerable.Range(0, 3).ToList().ForEach(_ => {
        Entry.GlobalId = 0;
        var threadCount = Environment.ProcessorCount;
        var waitHandles = new WaitHandle[threadCount];
        const int allocCount = 500;
        var threads = Enumerable.Range(0, threadCount).Select(x => {
          var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
          waitHandles[x] = waitHandle;
          return new Thread(() => {
            for (var i = 0; i < allocCount; i++) {
              var item = pool.Allocate();
              pool.Recycle(item);
            }
            waitHandle.Set();
          });
        }).ToList();
        var sw = Stopwatch.StartNew();
        foreach (var x in threads) {
          x.Start();
        }
        foreach (var x in threads) {
          x.Join();
        }
        Console.WriteLine("Pool allocated {0:n0} objects over {1:n0} allocation calls from {2:n0} threads in {3} msec",
          Entry.GlobalId, allocCount * threadCount, threadCount, sw.Elapsed.TotalMilliseconds);
        Console.WriteLine();
        Assert.IsTrue(verify(allocCount * threadCount));
      });
    }
  }
}