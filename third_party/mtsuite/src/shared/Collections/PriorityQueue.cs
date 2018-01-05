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

namespace mtsuite.shared.Collections {
  public class PriorityQueue<T> {
    private const int DefaultCapacity = 6;

    private readonly IComparer<T> _comparer;
    private int _count;
    private T[] _items;

    public PriorityQueue()
      : this(DefaultCapacity, null) {
    }

    public PriorityQueue(int capacity)
      : this(capacity, null) {
    }

    public PriorityQueue(int capacity, IComparer<T> comparer) {
      _items = new T[capacity];
      _count = 0;
      _comparer = (comparer ?? Comparer<T>.Default);
    }

    public PriorityQueue(IComparer<T> comparer)
      : this(DefaultCapacity, comparer) {
    }

    public T Max {
      get { return Root; }
    }

    public int Count {
      get { return _count; }
    }

    public T Root {
      get {
        if (_count == 0)
          throw new InvalidOperationException("Heap is empty.");

        return _items[0];
      }
    }

    public T Remove() {
      if (_count == 0)
        throw new InvalidOperationException("Heap is empty.");

      var result = Root;
      var last = _count - 1;
      Swap(0, last);
      _items[_count - 1] = default(T);
      _count--;
      SiftDown(0);
      return result;
    }

    public void Add(T value) {
      ExpandArray();
      var leaf = _count;
      _items[leaf] = value;
      _count++;
      SiftUp(leaf);
    }

    public void Clear() {
      Array.Clear(_items, 0, _items.Length);
      _count = 0;
    }

    private void SiftDown(int root) {
      var child = LeftChild(root);
      while (child < _count) {
        var rightChild = RightChildFromLeftChild(child);
        if (rightChild < _count && Compare(child, rightChild) < 0)
          child = rightChild;
        if (Compare(root, child) < 0)
          Swap(root, child);
        else
          return;

        root = child;
        child = LeftChild(root);
      }
    }

    private void SiftUp(int child) {
      while (child > 0) {
        var parent = Parent(child);
        if (Compare(parent, child) >= 0)
          break;
        Swap(child, parent);
        child = parent;
      }
    }

    private void Swap(int child, int parent) {
      var temp = _items[child];
      _items[child] = _items[parent];
      _items[parent] = temp;
    }

    private int Compare(int x, int y) {
      return -_comparer.Compare(_items[x], _items[y]);
    }

    private void ExpandArray() {
      if (_count == _items.Length) {
        var array = new T[_count * 2];
        Array.Copy(_items, array, _count);
        _items = array;
      }
    }

    private static int LeftChild(int i) {
      return i * 2 + 1;
    }

    private static int Parent(int i) {
      return (i - 1) / 2;
    }

    private static int RightChildFromLeftChild(int i) {
      return i + 1;
    }
  }
}