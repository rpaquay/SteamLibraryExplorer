// Copyright 2016 Renaud Paquay All Rights Reserved.
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
  /// An abstraction over a sequence of bytes allocated from the Native HEAP.
  /// </summary>
  public class ByteBuffer : IDisposable {
    private readonly SafeHGlobalHandle _memoryHandle = new SafeHGlobalHandle();
    private int _capacity;

    public ByteBuffer(int capacity) {
      Allocate(capacity);
    }

    private void Allocate(int capacity) {
      if (capacity <= 0) {
        throw new ArgumentException("Capacity must be positive", "capacity");
      }
      _memoryHandle.Realloc(capacity);
      _capacity = capacity;
    }

    public int Capacity {
      get { return _capacity; }
      set { Allocate(value); }
    }

    public IntPtr Pointer {
      get {
        return _memoryHandle.Pointer;
      }
    }

    public void Dispose() {
      _memoryHandle.Dispose();
    }

    public override string ToString() {
      var sb = new StringBuilder();
      sb.AppendFormat("Capacity={0}, [", Capacity);
      for (var i = 0; i < Capacity; i++) {
        sb.AppendFormat("0x{0:X2}, ", (byte)ReadFromMemory(i, 1));
      }
      sb.AppendFormat("]");
      return sb.ToString();
    }

    public string ReadString(int offset, int count) {
      CheckRange(offset, count);
      var sb = new StringBuffer();
      ReadString(offset, count, sb);
      return sb.Text;
    }

    public unsafe void ReadString(int offset, int count, StringBuffer stringBuffer) {
      CheckRange(offset, count);
      var bufferStart = (char*)(Pointer + offset).ToPointer();
      for (var i = 0; i < count; i++) {
        stringBuffer.Append(bufferStart[i]);
      }
    }

    public void WriteString(int offset, string value) {
      WriteString(offset, value, value.Length + 1);
    }

    public void WriteString(int offset, StringBuffer stringBuffer) {
      WriteString(offset, stringBuffer, stringBuffer.Length + 1);
    }

    public unsafe void WriteString(int offset, string value, int count) {
      count = Math.Min(count, value.Length + 1);
      fixed (char* valuePtr = value) {
        WriteCharacters(offset, count, valuePtr);
      }
    }

    public unsafe void WriteString(int offset, StringBuffer stringBuffer, int count) {
      count = Math.Min(count, stringBuffer.Length + 1);
      fixed (char* valuePtr = stringBuffer.Data) { 
        WriteCharacters(offset, count, valuePtr);
      }
    }

    public unsafe void WriteCharacters(int offset, int count, char* source) {
      EnsureCapacity(offset, count * sizeof(char));
      char* bufferStart = (char*)(Pointer + offset).ToPointer();
      while (count > 0) {
        *bufferStart = *source;
        bufferStart++;
        source++;
        count--;
      }
    }

    public void WriteInt8(int offset, sbyte value) {
      WriteToMemory(offset, sizeof(sbyte), AsUInt64(value));
    }

    public void WriteUInt8(int offset, byte value) {
      WriteToMemory(offset, sizeof(byte), AsUInt64(value));
    }

    public void WriteInt16(int offset, short value) {
      WriteToMemory(offset, sizeof(short), AsUInt64(value));
    }

    public void WriteUInt16(int offset, ushort value) {
      WriteToMemory(offset, sizeof(ushort), AsUInt64(value));
    }

    public void WriteInt32(int offset, int value) {
      WriteToMemory(offset, sizeof(int), AsUInt64(value));
    }

    public void WriteUInt32(int offset, uint value) {
      WriteToMemory(offset, sizeof(uint), AsUInt64(value));
    }

    public void WriteInt64(int offset, long value) {
      WriteToMemory(offset, sizeof(long), AsUInt64(value));
    }

    public void WriteUInt64(int offset, ulong value) {
      WriteToMemory(offset, sizeof(ulong), AsUInt64(value));
    }

    public unsafe UInt64 ReadFromMemory(int offset, int size) {
      CheckRange(offset, size);

      void* bufferStart = (Pointer + offset).ToPointer();
      switch (size) {
        case 1:
          return *(byte*)bufferStart;;
        case 2:
          return *(ushort*)bufferStart;
        case 4:
          return *(uint*)bufferStart;
        case 8:
          return *(ulong*)bufferStart;
      }
      throw new InvalidOperationException("Invalid field size (must be 1, 2, 4 or 8)");
    }

    public unsafe void WriteToMemory(int offset, int size, UInt64 value) {
      EnsureCapacity(offset, size);

      void* bufferStart = (Pointer + offset).ToPointer();
      switch (size) {
        case 1:
          *(byte*)bufferStart = AsUInt8(value);
          break;
        case 2:
          *(ushort*)bufferStart = AsUInt16(value);
          break;
        case 4:
          *(uint*)bufferStart = AsUInt32(value);
          break;
        case 8:
          *(ulong*)bufferStart = AsUInt64(value);
          break;
        default:
          throw new ArgumentException("Invalid size (must be 1, 2, 4 or 8)", "size");
      }
    }

    public static byte AsUInt8(ulong value) {
      return unchecked((byte)value);
    }

    public static ushort AsUInt16(ulong value) {
      return unchecked((ushort)value);
    }

    public static uint AsUInt32(ulong value) {
      return unchecked((uint)value);
    }

    public static ulong AsUInt64(byte value) {
      return unchecked((ulong)value);
    }

    public static ulong AsUInt64(short value) {
      return unchecked((ulong)value);
    }

    public static ulong AsUInt64(int value) {
      return unchecked((ulong)value);
    }

    public static ulong AsUInt64(long value) {
      return unchecked((ulong)value);
    }

    public static ulong AsUInt64(ulong value) {
      return unchecked((ulong)value);
    }

    private void CheckRange(int offset, int size) {
      if (offset < 0)
        ThrowInvalidRange(offset, size);
      if (size < 0)
        ThrowInvalidRange(offset, size);
      if (checked(offset + size) > Capacity)
        ThrowInvalidRange(offset, size);
    }

    private void EnsureCapacity(int offset, int size) {
      if (offset < 0)
        ThrowInvalidRange(offset, size);
      if (size < 0)
        ThrowInvalidRange(offset, size);

      checked {
        if (_capacity >= offset + size)
          return;

        var newCapacity = _capacity;
        while (newCapacity < offset + size) {
          newCapacity *= 2;
        }
        Allocate(newCapacity);
      }
    }

    private void ThrowInvalidRange(int offset, int size) {
      throw new InvalidOperationException(string.Format("Trying to read past end of buffer (Offset={0}, Size={1}, Capacity={2}", offset, size, Capacity));
    }
  }
}