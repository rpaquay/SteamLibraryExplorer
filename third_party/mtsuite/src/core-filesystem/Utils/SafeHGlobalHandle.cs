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
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace mtsuite.CoreFileSystem.Utils {
  public class SafeHGlobalHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public SafeHGlobalHandle()
      : base(ownsHandle: true) {
    }

    public IntPtr Pointer { get { return handle; } }

    public void Alloc(int size) {
      ReleaseHandle();
      handle = Marshal.AllocHGlobal(size);
    }

    public void Realloc(int size) {
      if (handle == IntPtr.Zero) {
        Alloc(size);
      } else {
        handle = Marshal.ReAllocHGlobal(handle, new IntPtr(size));
      }
    }

    public void Free() {
      ReleaseHandle();
    }

    protected override bool ReleaseHandle() {
      if (handle != IntPtr.Zero) {
        Marshal.FreeHGlobal(handle);
        handle = IntPtr.Zero;
      }
      return true;
    }
  }
}