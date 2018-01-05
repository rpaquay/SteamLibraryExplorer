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
using System.Threading;

namespace mtsuite.CoreFileSystem.ObjectPool {
  /// <summary>
  /// A thread safe implementation of <see cref="IPool{T}"/> using a fixed
  /// number of slots to hold and recycle object instances. By default, the
  /// number of slots is proportional to the number of processors/cores.
  /// </summary>
  public class ConcurrentFixedSizeArrayPool<T> : IPool<T> where T : class {
    [DebuggerDisplay("{Value}")]
    private struct Entry {
      public T Value;
    }

    /// <summary>
    /// Instance creation function, used when pool is empty.
    /// </summary>
    private readonly Func<T> _creator;

    /// <summary>
    /// Instance recycle function: used everytime an object is put back into the
    /// pool.
    /// </summary>
    private readonly Action<T> _recycler;

    /// <summary>
    /// Slots used to store the available instances. We use the <see
    /// cref="Entry"/> class to wrap instances so that we can use <see
    /// cref="Interlocked.CompareExchange{T}(ref T,T,T)"/> to obtain/release
    /// instances in a thread safe way.
    /// </summary>
    private readonly Entry[] _entries;

    public ConcurrentFixedSizeArrayPool(Func<T> creator, Action<T> recycler)
      : this(creator, recycler, Environment.ProcessorCount * 2) {
    }

    public ConcurrentFixedSizeArrayPool(Func<T> creator, Action<T> recycler, int size) {
      if (creator == null)
        throw new ArgumentNullException("creator");
      if (recycler == null)
        throw new ArgumentNullException("recycler");
      if (size < 1)
        throw new ArgumentException("Size must be >= 1", "size");
      _creator = creator;
      _recycler = recycler;
      _entries = new Entry[size];
    }

    public T Allocate() {
      for (var i = 0; i < _entries.Length; i++) {
        var current = _entries[i].Value;
        if (current != null) {
          // Item looks available: Take owership of it in a thread-safe manner,
          // keep going if another thread was faster than us.
          var item = Interlocked.CompareExchange(ref _entries[i].Value, null, current);
          if (item == current) {
            // We won: leave now!
            return item;
          }
        }
      }
      return _creator();
    }

    public void Recycle(T item) {
      _recycler(item);

      for (var i = 0; i < _entries.Length; i++) {
        var current = _entries[i].Value;
        if (current == null) {
          // Free slot looks available: Take owership of it in a thread-safe
          // manner, keep going if another thread was faster than us.
          if (Interlocked.CompareExchange(ref _entries[i].Value, item, null) == null) {
            // We won: leave now!
            return;
          }
        }
      }
    }
  }
}