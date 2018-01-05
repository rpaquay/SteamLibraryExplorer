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
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace mtsuite.CoreFileSystem.Utils {
  /// <summary>
  /// Type safe access over a sequences of bytes from a <see cref="ByteBuffer"/>
  /// </summary>
  public struct TypedBuffer<TStruct> {
    private readonly ByteBuffer _buffer;

    public TypedBuffer(ByteBuffer buffer) {
      _buffer = buffer;
    }

    public int SizeOf {
      get { return Marshal.SizeOf(typeof(TStruct)); }
    }

    public int GetFieldOffset<TField>(Expression<Func<TStruct, TField>> field) {
      var name = ReflectionUtils.GetFieldName(default(TStruct), field);
      return Marshal.OffsetOf(typeof(TStruct), name).ToInt32();
    }

    public int GetFieldSize<TField>(Expression<Func<TStruct, TField>> field) {
      return Marshal.SizeOf(typeof(TField));
    }

    public sbyte Read(Expression<Func<TStruct, sbyte>> field) {
      return unchecked((sbyte)ReadField(field));
    }

    public byte Read(Expression<Func<TStruct, byte>> field) {
      return unchecked((byte)ReadField(field));
    }

    public short Read(Expression<Func<TStruct, short>> field) {
      var offset = GetFieldOffset(field);
      var value = _buffer.ReadFromMemory(offset, Marshal.SizeOf(typeof(short)));
      return unchecked((short)value);
    }

    public ushort Read(Expression<Func<TStruct, ushort>> field) {
      return unchecked((ushort)ReadField(field));
    }

    public int Read(Expression<Func<TStruct, int>> field) {
      return unchecked((int)ReadField(field));
    }

    public uint Read(Expression<Func<TStruct, uint>> field) {
      return unchecked((uint)ReadField(field));
    }

    public long Read(Expression<Func<TStruct, long>> field) {
      return unchecked((long)ReadField(field));
    }

    public ulong Read(Expression<Func<TStruct, ulong>> field) {
      return ReadField(field);
    }

    public TField Read<TField>(Expression<Func<TStruct, TField>> field) {
      var offset = GetFieldOffset(field);
      var fieldType = typeof(TField);
      if (fieldType.IsEnum) {
        fieldType = fieldType.GetEnumUnderlyingType();
      }
      var size = Marshal.SizeOf(fieldType);
      var value = _buffer.ReadFromMemory(offset, size);

      if (fieldType == typeof(byte))
        return (TField)(object)(byte)value;

      if (fieldType == typeof(sbyte))
        return (TField)(object)(sbyte)value;

      if (fieldType == typeof(ushort))
        return (TField)(object)(ushort)value;

      if (fieldType == typeof(short))
        return (TField)(object)(short)value;

      if (fieldType == typeof(int))
        return (TField)(object)(int)value;

      if (fieldType == typeof(uint))
        return (TField)(object)(uint)value;

      if (fieldType == typeof(long))
        return (TField)(object)value;

      if (fieldType == typeof(ulong))
        return (TField)(object)(ulong)value;

      throw new InvalidOperationException("Invalid integer type");
    }

    public string ReadString(int offset, int length) {
      return _buffer.ReadString(offset, length);
    }

    public void WriteString(int offset, StringBuffer stringBuffer) {
      _buffer.WriteString(offset, stringBuffer);
    }

    public void Write<TField>(Expression<Func<TStruct, TField>> field, Int64 value) {
      Write(field, ByteBuffer.AsUInt64(value));
    }

    public void Write<TField>(Expression<Func<TStruct, TField>> field, UInt64 value) {
      var offset = GetFieldOffset(field);
      var fieldType = typeof(TField);
      if (fieldType.IsEnum) {
        fieldType = fieldType.GetEnumUnderlyingType();
      }

      if (fieldType == typeof(byte)) {
        _buffer.WriteUInt8(offset, unchecked((byte)value));

      } else if (fieldType == typeof(sbyte)) {
        _buffer.WriteUInt8(offset, unchecked((byte)value));

      } else if (fieldType == typeof(short)) {
        _buffer.WriteUInt16(offset, unchecked((ushort)value));

      } else if (fieldType == typeof(ushort)) {
        _buffer.WriteUInt16(offset, unchecked((ushort)value));

      } else if (fieldType == typeof(int)) {
        _buffer.WriteUInt32(offset, unchecked((uint)value));

      } else if (fieldType == typeof(uint)) {
        _buffer.WriteUInt32(offset, unchecked((uint)value));

      } else if (fieldType == typeof(long)) {
        _buffer.WriteUInt64(offset, unchecked((ulong)value));

      } else if (fieldType == typeof(ulong)) {
        _buffer.WriteUInt64(offset, unchecked((ulong)value));

      } else {
        throw new InvalidOperationException("Invalid integer type");
      }
    }

    private UInt64 ReadField<TField>(Expression<Func<TStruct, TField>> field) {
      var offset = GetFieldOffset(field);
      return _buffer.ReadFromMemory(offset, Marshal.SizeOf(typeof(TField)));
    }
  }
}