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
using System.Collections.Generic;
using System.Linq;

namespace mtsuite.shared.Collections {
  /// <summary>
  /// Implementation of a set that allows retrieving elements stored in a given
  /// <see cref="IList{T}"/> from a key surrogate element. <see
  /// cref="SmallSet{T}"/> uses a simple reference to the source list for small
  /// collections, or creates a dictionary for larger collections.
  /// </summary>
  public class SmallSet<T> {
    public const int Threshold = 20;
    private readonly IEqualityComparer<T> _comparer;
    private List<T> _itemsList;
    private Dictionary<T, T> _itemsDic;
    private Func<T, bool> _contains;
    private Func<T, KeyValuePair<bool, T>> _tryGet;

    public SmallSet()
      : this(EqualityComparer<T>.Default) {
    }

    public SmallSet(IEqualityComparer<T> comparer) {
      _comparer = comparer;
    }

    public SmallSet(List<T> items) : this(items, EqualityComparer<T>.Default) {
    }

    public SmallSet(List<T> items, IEqualityComparer<T> comparer): this(comparer) {
      SetList(items);
    }

    public void SetList(List<T> items) {
      if ((items.Count > Threshold)) {
        if (_itemsDic == null)
          _itemsDic = new Dictionary<T, T>(_comparer);
        foreach(var x in items)
          _itemsDic.Add(x, x);
        _contains = x => _itemsDic.ContainsKey(x);
        _tryGet = DictionaryTryGet;
      } else {
        _itemsList = items;
        _contains = t => _itemsList.Contains(t, _comparer);
        _tryGet = ListTryGet;
      }
    }

    public void Clear() {
      if (_itemsDic != null)
        _itemsDic.Clear();
      _itemsList = null;
      _contains = null;
      _tryGet = null;
    }

    public bool Contains(T item) {
      return _contains(item);
    }

    public bool TryGet(T key, out T value) {
      var result = TryGet(key);
      value = result.Value;
      return result.Key;
    }

    public KeyValuePair<bool, T> TryGet(T key) {
      return _tryGet(key);
    }

    private KeyValuePair<bool, T> DictionaryTryGet(T key) {
      T value;
      var found = _itemsDic.TryGetValue(key, out value);
      return new KeyValuePair<bool, T>(found, value);
    }

    private KeyValuePair<bool, T> ListTryGet(T key) {
      for (var i = 0; i < _itemsList.Count; i++) {
        var item = _itemsList[i];
        if (_comparer.Equals(key, item)) {
          return new KeyValuePair<bool, T>(true, item);
        }
      }
      return new KeyValuePair<bool, T>(false, default(T));
    }
  }
}