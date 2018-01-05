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
using System.Linq;
using System.Threading;
using mtsuite.shared.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests {
  [TestClass]
  public class TaskTest {
    private ITaskFactory _factory;
    private TimeSpan _timeOut = TimeSpan.FromSeconds(30);
    private TimeSpan _taskSleep = TimeSpan.FromMilliseconds(5);

    [TestInitialize]
    public void Setup() {
      // Switch to test other implementations of ITaskFactory.
      _factory = new DefaultTaskFactory();
      //_factory = new CustomTaskFactory();

      // Switch when debugging to avoid early failure.
      _timeOut = TimeSpan.FromSeconds(1);
      //_timeOut = TimeSpan.FromSeconds(100);
    }

    [TestCleanup]
    public void Cleanup() {
      _factory.Dispose();
    }

    [TestMethod]
    public void StartNewShouldWork() {
      var factory = _factory;

      var a = 0;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      Assert.AreEqual(1, a);
    }

    [TestMethod]
    public void StartNewWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
        return "test";
      });
      Assert.IsTrue(longTask.Wait(TimeSpan.FromSeconds(3)));

      Assert.AreEqual("test", longTask.Result);
      Assert.AreEqual(1, a);
    }

    [TestMethod]
    public void TaskWithContinuationShouldWork() {
      var factory = _factory;

      var a = 0;
      bool seen1 = false;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
      }).ContinueWith(_ => {
        if (a == 1)
          seen1 = true;
        a = 2;
      });
      Assert.IsTrue(longTask.Wait(_timeOut));
      Assert.IsTrue(seen1);
      Assert.AreEqual(2, a);
    }

    [TestMethod]
    public void TaskWithContinuationWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      bool seen1 = false;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
      }).ContinueWith(_ => {
        if (a == 1)
          seen1 = true;
        a = 2;
        return "test";
      });
      Assert.IsTrue(longTask.Wait(_timeOut));
      Assert.IsTrue(seen1);
      Assert.AreEqual(2, a);
      Assert.AreEqual("test", longTask.Result);
    }

    [TestMethod]
    public void TaskThenShouldWork() {
      var factory = _factory;

      var a = 0;
      bool seen1 = false;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
      }).Then(_ => {
        return factory.StartNew(() => {
          if (a == 1)
            seen1 = true;
          a = 2;
        });
      });
      Assert.IsTrue(longTask.Wait(TimeSpan.FromSeconds(3)));

      Assert.IsTrue(seen1);
      Assert.AreEqual(2, a);
    }

    [TestMethod]
    public void TaskThenWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      bool seen1 = false;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
      }).Then(_ => {
        return factory.StartNew(() => {
          if (a == 1)
            seen1 = true;
          a = 2;
          return "test2";
        });
      });
      Assert.IsTrue(longTask.Wait(TimeSpan.FromSeconds(3)));

      Assert.AreEqual("test2", longTask.Result);
      Assert.IsTrue(seen1);
      Assert.AreEqual(2, a);
    }

    [TestMethod]
    public void TaskWithResultWithContinuationShouldWork() {
      var factory = _factory;

      var a = 0;
      bool seen1 = false;
      string taskResult = null;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
        return "test";
      }).ContinueWith(_ => {
        taskResult = _.Result;
        if (a == 1)
          seen1 = true;
        a = 2;
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      Assert.AreEqual("test", taskResult);
      Assert.IsTrue(seen1);
      Assert.AreEqual(2, a);
    }

    [TestMethod]
    public void TaskWithResultWithContinuationWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      bool seen1 = false;
      string taskResult = null;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
        return "test";
      }).ContinueWith(_ => {
        taskResult = _.Result;
        if (a == 1)
          seen1 = true;
        a = 2;
        return "test";
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      Assert.AreEqual("test", taskResult);
      Assert.IsTrue(seen1);
      Assert.AreEqual(2, a);
      Assert.AreEqual("test", longTask.Result);
    }

    [TestMethod]
    public void TaskWithResultThenShouldWork() {
      var factory = _factory;

      var a = 0;
      bool seen1 = false;
      string taskResult = null;
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
        return "test";
      }).Then(_ => {
        taskResult = _.Result;
        return factory.StartNew(() => {
          if (a == 1)
            seen1 = true;
          a = 2;
        });
      });
      Assert.IsTrue(longTask.Wait(TimeSpan.FromSeconds(3)));

      Assert.AreEqual("test", taskResult);
      Assert.IsTrue(seen1);
      Assert.AreEqual(2, a);
    }

    [TestMethod]
    public void TaskWithResultThenWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      bool seen1 = false;
      string taskResult = "";
      var longTask = factory.StartNew(() => {
        Thread.Sleep(_taskSleep);
        a = 1;
        return "test";
      }).Then(_ => {
        taskResult = _.Result;
        return factory.StartNew(() => {
          if (a == 1)
            seen1 = true;
          a = 2;
          return "test2";
        });
      });
      Assert.IsTrue(longTask.Wait(TimeSpan.FromSeconds(3)));

      Assert.AreEqual("test", taskResult);
      Assert.AreEqual("test2", longTask.Result);
      Assert.IsTrue(seen1);
      Assert.AreEqual(2, a);
    }

    [TestMethod]
    public void CollectionWithContinuationShouldWork() {
      var factory = _factory;

      var a = 0;
      var seen10 = 0;
      var taskCollection = factory.CreateCollection(Enumerable.Range(1, 10).Select(x => {
        return factory.StartNew(() => {
          Thread.Sleep(_taskSleep);
          Interlocked.Increment(ref a);
        });
      }));
      var longTask = taskCollection.ContinueWith(_ => {
        seen10 = a;
        a = 21;
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      Assert.AreEqual(10, seen10);
      Assert.AreEqual(21, a);
    }

    [TestMethod]
    public void CollectionWithContinuationWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      var seen10 = 0;
      var taskCollection = factory.CreateCollection(Enumerable.Range(1, 10).Select(x => {
        return factory.StartNew(() => {
          Thread.Sleep(_taskSleep);
          Interlocked.Increment(ref a);
        });
      }));
      var longTask = taskCollection.ContinueWith(_ => {
        seen10 = a;
        a = 21;
        return "test";
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      Assert.AreEqual(10, seen10);
      Assert.AreEqual(21, a);
      Assert.AreEqual("test", longTask.Result);
    }

    [TestMethod]
    public void CollectionThenShouldWork() {
      var factory = _factory;

      var a = 0;
      var seen10 = 0;
      var taskCollection = factory.CreateCollection(Enumerable.Range(1, 10).Select(x => {
        return factory.StartNew(() => {
          Thread.Sleep(_taskSleep);
          Interlocked.Increment(ref a);
        });
      }));
      var longTask = taskCollection.Then(_ => {
        return factory.StartNew(() => {
          seen10 = a;
          a = 21;
        });
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      Assert.AreEqual(10, seen10);
      Assert.AreEqual(21, a);
    }

    [TestMethod]
    public void CollectionThenWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      var seen10 = 0;
      var taskCollection = factory.CreateCollection(Enumerable.Range(1, 10).Select(x => {
        return factory.StartNew(() => {
          Thread.Sleep(_taskSleep);
          Interlocked.Increment(ref a);
        });
      }));
      var longTask = taskCollection.Then(_ => {
        return factory.StartNew(() => {
          seen10 = a;
          a = 21;
          return "test";
        });
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      Assert.AreEqual(10, seen10);
      Assert.AreEqual(21, a);
      Assert.AreEqual("test", longTask.Result);
    }

    [TestMethod]
    public void CollectionWithResultWithContinuationShouldWork() {
      var factory = _factory;

      var a = 0;
      var seen10 = 0;
      string[] results = null;
      var taskCollection = factory.CreateCollection(Enumerable.Range(1, 10).Select(x => {
        return factory.StartNew(() => {
          Thread.Sleep(_taskSleep);
          Interlocked.Increment(ref a);
          return "test";
        });
      }));
      var longTask = taskCollection.ContinueWith(_ => {
        results = _.Select(x => x.Result).ToArray();
        seen10 = a;
        a = 21;
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      CollectionAssert.AllItemsAreInstancesOfType(results, typeof(string));
      Assert.AreEqual(10, seen10);
      Assert.AreEqual(21, a);
    }

    [TestMethod]
    public void CollectionWithResultWithContinuationWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      var seen10 = 0;
      var taskCollection = factory.CreateCollection(Enumerable.Range(1, 10).Select(x => {
        return factory.StartNew(() => {
          Thread.Sleep(_taskSleep);
          return Interlocked.Increment(ref a);
        });
      }));
      var longTask = taskCollection.ContinueWith(_ => {
        seen10 = a;
        a = 21;
        return _.Select(x => x.Result).Sum();
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      Assert.AreEqual(10, seen10);
      Assert.AreEqual(21, a);
      // 55 = sum of integers from 1 to 10
      Assert.AreEqual(55, longTask.Result);
    }

    [TestMethod]
    public void CollectionWithResultThenShouldWork() {
      var factory = _factory;

      var a = 0;
      var seen10 = 0;
      string[] results = null;
      var taskCollection = factory.CreateCollection(Enumerable.Range(1, 10).Select(x => {
        return factory.StartNew(() => {
          Thread.Sleep(_taskSleep);
          Interlocked.Increment(ref a);
          return "test";
        });
      }));
      var longTask = taskCollection.Then(_ => {
        return factory.StartNew(() => {
          results = _.Select(x => x.Result).ToArray();
          seen10 = a;
          a = 21;
        });
      });
      Assert.IsTrue(longTask.Wait(_timeOut));
      CollectionAssert.AllItemsAreInstancesOfType(results, typeof(string));
      Assert.AreEqual(10, seen10);
      Assert.AreEqual(21, a);
    }

    [TestMethod]
    public void CollectionWithResultThenWithResultShouldWork() {
      var factory = _factory;

      var a = 0;
      var seen10 = 0;
      var taskCollection = factory.CreateCollection(Enumerable.Range(1, 10).Select(x => {
        return factory.StartNew(() => {
          Thread.Sleep(_taskSleep);
          return Interlocked.Increment(ref a);
        });
      }));
      var longTask = taskCollection.Then(_ => {
        return factory.StartNew(() => {
          seen10 = a;
          a = 21;
          return _.Select(x => x.Result).Sum();
        });
      });
      Assert.IsTrue(longTask.Wait(_timeOut));

      // 55 = sum of integers from 1 to 10
      Assert.AreEqual(55, longTask.Result);
      Assert.AreEqual(10, seen10);
      Assert.AreEqual(21, a);
    }
  }
}
