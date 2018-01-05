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
namespace mtsuite.CoreFileSystem.ObjectPool {
  /// <summary>
  /// Abstraction over a pool of objects. The <see cref="Allocate"/> method
  /// obtains an object from the pool. The <see cref="Recycle"/> method puts an
  /// object back into the pool. Implementations must guarantee that forgetting
  /// to recycle an object back into the pool is harmless, except maybe for some
  /// amount of performance overhead. In other words, implementations should not
  /// hold a strong reference to objects allocated from the pool so that they
  /// can be garbage collected when the consumer does not reference them
  /// anymore.
  /// </summary>
  public interface IPool<T> where T : class {
    T Allocate();
    void Recycle(T item);
  }
}