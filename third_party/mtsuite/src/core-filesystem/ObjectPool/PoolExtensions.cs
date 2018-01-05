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
  public static class PoolExtensions {
    /// <summary>
    /// Allocate an item from the pool, returning it inside a <see
    /// cref="FromPool{T}"/> wrapper.
    /// </summary>
    public static FromPool<T> AllocateFrom<T>(this IPool<T> pool) where T : class {
      return new FromPool<T>(pool, pool.Allocate());
    }
  }
}