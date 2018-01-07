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
using System.Text;

namespace mtsuite.CoreFileSystem.Utils {
  /// <summary>
  /// A class similar to <see cref="StringBuilder"/> with the ability to expose
  /// its internal array of characters, which is always terminated with a
  /// <cdoe>NUL</cdoe> character. This class allows calling efficiently (i.e.
  /// without extra allocations due to marshaling) native methods that take one
  /// or more <code>NUL</code> terminated strings as input parameter(s).
  /// </summary>
  public class StringBuffer {
    private char[] _items;
    private int _length;

    /// <summary>
    /// Create a buffer with a default initial capacity.
    /// </summary>
    public StringBuffer()
      : this(32) {
    }

    /// <summary>
    /// Create a buffer with an initial <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity"></param>
    public StringBuffer(int capacity) {
      _items = new char[capacity + 1];
      Clear();
    }

    /// <summary>
    /// Clear the buffer so that it contains the equivalent of an empty string.
    /// </summary>
    public void Clear() {
      _length = 0;
      _items[0] = '\0';
    }

    /// <summary>
    /// The number of characters, not counting the terminating <code>NUL</code>
    /// character.
    /// </summary>
    public int Length { get { return _length; } }

    /// <summary>
    /// Return a reference to the internal array of characters. The array
    /// contains always at least one character, and the last character is always
    /// the <code>NUL</code> terminator.
    /// </summary>
    public char[] Data { get { return _items; } }

    /// <summary>
    /// Return a string representation of the internal array of character, not
    /// including the <code>NUL</code> terminator. This call is guaranteed to
    /// perform exactly one string allocation.
    /// </summary>
    public string Text { get { return ToString(); } }

    public void Append(char value) {
      EnsureCapacity(_length + 2);
      _items[_length++] = value;
      _items[_length] = '\0';
    }

    public void Append(string value) {
      EnsureCapacity(_length + value.Length + 1);
      value.CopyTo(0, _items, _length, value.Length);
      _length += value.Length;
      _items[_length] = '\0';
    }

    public void InsertAt(int index, char value) {
      EnsureCapacity(_length + 2);
      Array.Copy(_items, index, _items, index + 1, _length - index);
      _items[index] = value;

      _length++;
      _items[_length] = '\0';
    }

    public void InsertAt(int index, string value) {
      EnsureCapacity(_length + value.Length + 1);
      Array.Copy(_items, index, _items, index + value.Length, _length - index);

      value.CopyTo(0, _items, index, value.Length);

      _length += value.Length;
      _items[_length] = '\0';
    }

    public void DeleteAt(int position, int count) {
      if (position < 0) throw new ArgumentException("position");
      if (position > _length) throw new ArgumentException("position");
      if (count < 0) throw new ArgumentException("count");
      if (count > _length - position) throw new ArgumentException("count");

      Array.Copy(_items, position + count, _items, position, _length - count);
      _length -= count;
      _items[_length] = '\0';
    }

    public override string ToString() {
      return new string(_items, 0, _length);
    }

    /// <summary>
    /// Double the size of the character array until there is enough room for
    /// <paramref name="capacity"/> characters (including the <code>NUL</code>
    /// terminated).
    /// </summary>
    private void EnsureCapacity(int capacity) {
      while (capacity > _items.Length) {
        Array.Resize(ref _items, _items.Length * 2);
      }
    }
  }
}