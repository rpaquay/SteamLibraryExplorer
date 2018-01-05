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

namespace mtsuite.CoreFileSystem.ObjectPool {
  /// <summary>
  /// Disposable wrapper around an object allocated from a pool. The <see
  /// cref="Dispose"/> method releases the object back into the owning pool.
  /// </summary>
  public struct FromPool<T> : IDisposable where T : class {
    private readonly IPool<T> _pool;
    private readonly T _item;

    public FromPool(IPool<T> pool, T item) {
      _pool = pool;
      _item = item;
    }

    /// <summary>
    /// The item that has been allocated from the pool. The item value is
    /// invalid after the <see cref="Dispose"/> method has been called.
    /// </summary>
    public T Item {
      get { return _item; }
    }

    public void Dispose() {
      _pool.Recycle(_item);
    }
  }
}