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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace mtsuite.CoreFileSystem.Win32 {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public static class NativeMethods {

    [DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern SafeFindHandle FindFirstFileEx(
      char[] pszPattern,
      FINDEX_INFO_LEVELS fInfoLevelId,
      out WIN32_FIND_DATA lpFindFileData,
      FINDEX_SEARCH_OPS fSearchOp,
      IntPtr lpSearchFilter,
      FINDEX_ADDITIONAL_FLAGS dwAdditionalFlags);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool FindNextFile(SafeFindHandle hndFindFile, out WIN32_FIND_DATA lpFindFileData);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool FindClose(IntPtr handle);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool DeleteFile(char[] path);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool RemoveDirectory(char[] lpPathName);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetFileAttributes(char[] lpPathName, FILE_ATTRIBUTE dwFileAttributes);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern FILE_ATTRIBUTE GetFileAttributes(char[] lpFileName);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetFileAttributesEx(
      char[] lpFileName,
      GET_FILEEX_INFO_LEVELS fInfoLevelId,
      out WIN32_FILE_ATTRIBUTE_DATA fileData);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CreateDirectory(char[] lpPathName, IntPtr lpSecurityAttributes);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CopyFileEx(
      char[] lpExistingFileName,
      char[] lpNewFileName,
      IntPtr/*CopyProgressRoutine*/ lpProgressRoutine,
      IntPtr lpData,
      ref Int32 pbCancel,
      CopyFileFlags dwCopyFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [SuppressUnmanagedCodeSecurity]
    public static extern SafeFileHandle CreateFile(
      char[] lpFileName,
      EFileAccess dwDesiredAccess,
      EFileShare dwShareMode,
      IntPtr lpSecurityAttributes,
      ECreationDisposition creationDisposition,
      EFileAttributes flagsAndAttributes,
      IntPtr hTemplateFile);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public unsafe static extern bool GetFileTime(
      SafeFileHandle hFile,
      FILETIME* lpCreationTime,
      FILETIME* lpLastAccessTime,
      FILETIME* lpLastWriteTime);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public unsafe static extern bool SetFileTime(
      SafeFileHandle hFile,
      FILETIME* lpCreationTime,
      FILETIME* lpLastAccessTime,
      FILETIME* lpLastWriteTime);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int CreateSymbolicLink(
      char[] lpSymlinkFileName,
      string lpTargetFileName,
      SYMBOLIC_LINK_FLAG dwFlags);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeviceIoControl(
      SafeFileHandle hDevice,
      EIOControlCode dwIoControlCode,
      IntPtr lpInBuffer,
      int nInBufferSize,
      IntPtr lpOutBuffer,
      int nOutBufferSize,
      out int lpBytesReturned,
      //[In] ref System.Threading.NativeOverlapped Overlapped);
      IntPtr lpOverlapped);

    public enum FINDEX_INFO_LEVELS {
      FindExInfoStandard = 0,
      FindExInfoBasic = 1
    }

    public enum FINDEX_SEARCH_OPS {
      FindExSearchNameMatch = 0,
      FindExSearchLimitToDirectories = 1,
      FindExSearchLimitToDevices = 2
    }

    [Flags]
    public enum FINDEX_ADDITIONAL_FLAGS {
      FindFirstExCaseSensitive = 1,
      FindFirstExLargeFetch = 2,
    }

    public enum SYMBOLIC_LINK_FLAG {
      File = 0,
      Directory = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FILETIME {
      public uint dwLowDateTime;
      public uint dwHighDateTime;

      public static FILETIME FromDateTime(DateTime dateTime) {
        FILETIME result = default(FILETIME);
        LongToHighLow(dateTime.ToFileTimeUtc(), out result.dwHighDateTime, out result.dwLowDateTime);
        return result;
      }

      public DateTime ToDateTimeUtc() {
        return DateTime.FromFileTimeUtc(HighLowToLong(dwHighDateTime, dwLowDateTime));
      }

      public override string ToString() {
        return ToDateTimeUtc().ToString();
      }
    }

    public static long HighLowToLong(int high, int low) {
      return HighLowToLong((uint)high, (uint)low);
    }

    public static long HighLowToLong(uint high, uint low) {
      return low + ((long)high << 32);
    }

    public static void LongToHighLow(long value, out uint high, out uint low) {
      high = (uint)((value >> 32) & uint.MaxValue);
      low = (uint)(value & 0xffffffff);
    }

    public delegate CopyProgressResult CopyProgressRoutine(
     long TotalFileSize,
     long TotalBytesTransferred,
     long StreamSize,
     long StreamBytesTransferred,
     uint dwStreamNumber,
     CopyProgressCallbackReason dwCallbackReason,
     IntPtr hSourceFile,
     IntPtr hDestinationFile,
     IntPtr lpData);

    public enum CopyProgressCallbackReason : uint {
      CALLBACK_CHUNK_FINISHED = 0x00000000,
      CALLBACK_STREAM_SWITCH = 0x00000001
    }

    public enum CopyProgressResult : uint {
      PROGRESS_CONTINUE = 0,
      PROGRESS_CANCEL = 1,
      PROGRESS_STOP = 2,
      PROGRESS_QUIET = 3
    }

    [Flags]
    public enum CopyFileFlags : uint {
      COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
      COPY_FILE_RESTARTABLE = 0x00000002,
      COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
      COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,
      COPY_FILE_COPY_SYMLINK = 0x00000800, //NT 6.0+
      COPY_FILE_NO_BUFFERING = 0x00001000 //NT 6.0+
    }

    [Flags]
    public enum EFileAccess : uint {
      //
      // Standart Section
      //

      AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
      MaximumAllowed = 0x2000000,     // MaximumAllowed access type

      Delete = 0x10000,
      ReadControl = 0x20000,
      WriteDAC = 0x40000,
      WriteOwner = 0x80000,
      Synchronize = 0x100000,

      StandardRightsRequired = 0xF0000,
      StandardRightsRead = ReadControl,
      StandardRightsWrite = ReadControl,
      StandardRightsExecute = ReadControl,
      StandardRightsAll = 0x1F0000,
      SpecificRightsAll = 0xFFFF,

      FILE_READ_DATA = 0x0001,        // file & pipe
      FILE_LIST_DIRECTORY = 0x0001,       // directory
      FILE_WRITE_DATA = 0x0002,       // file & pipe
      FILE_ADD_FILE = 0x0002,         // directory
      FILE_APPEND_DATA = 0x0004,      // file
      FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
      FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
      FILE_READ_EA = 0x0008,          // file & directory
      FILE_WRITE_EA = 0x0010,         // file & directory
      FILE_EXECUTE = 0x0020,          // file
      FILE_TRAVERSE = 0x0020,         // directory
      FILE_DELETE_CHILD = 0x0040,     // directory
      FILE_READ_ATTRIBUTES = 0x0080,      // all
      FILE_WRITE_ATTRIBUTES = 0x0100,     // all

      //
      // Generic Section
      //

      GenericRead = 0x80000000,
      GenericWrite = 0x40000000,
      GenericExecute = 0x20000000,
      GenericAll = 0x10000000,

      SPECIFIC_RIGHTS_ALL = 0x00FFFF,
      FILE_ALL_ACCESS =
      StandardRightsRequired |
      Synchronize |
      0x1FF,

      FILE_GENERIC_READ =
      StandardRightsRead |
      FILE_READ_DATA |
      FILE_READ_ATTRIBUTES |
      FILE_READ_EA |
      Synchronize,

      FILE_GENERIC_WRITE =
      StandardRightsWrite |
      FILE_WRITE_DATA |
      FILE_WRITE_ATTRIBUTES |
      FILE_WRITE_EA |
      FILE_APPEND_DATA |
      Synchronize,

      FILE_GENERIC_EXECUTE =
      StandardRightsExecute |
        FILE_READ_ATTRIBUTES |
        FILE_EXECUTE |
        Synchronize
    }
    [Flags]
    public enum EFileAttributes : uint {
      Readonly = 0x00000001,
      Hidden = 0x00000002,
      System = 0x00000004,
      Directory = 0x00000010,
      Archive = 0x00000020,
      Device = 0x00000040,
      Normal = 0x00000080,
      Temporary = 0x00000100,
      SparseFile = 0x00000200,
      ReparsePoint = 0x00000400,
      Compressed = 0x00000800,
      Offline = 0x00001000,
      NotContentIndexed = 0x00002000,
      Encrypted = 0x00004000,
      Write_Through = 0x80000000,
      Overlapped = 0x40000000,
      NoBuffering = 0x20000000,
      RandomAccess = 0x10000000,
      SequentialScan = 0x08000000,
      DeleteOnClose = 0x04000000,
      BackupSemantics = 0x02000000,
      PosixSemantics = 0x01000000,
      OpenReparsePoint = 0x00200000,
      OpenNoRecall = 0x00100000,
      FirstPipeInstance = 0x00080000
    }

    [Flags]
    public enum EFileShare : uint {
      /// <summary>
      /// 
      /// </summary>
      None = 0x00000000,
      /// <summary>
      /// Enables subsequent open operations on an object to request read access. 
      /// Otherwise, other processes cannot open the object if they request read access. 
      /// If this flag is not specified, but the object has been opened for read access, the function fails.
      /// </summary>
      Read = 0x00000001,
      /// <summary>
      /// Enables subsequent open operations on an object to request write access. 
      /// Otherwise, other processes cannot open the object if they request write access. 
      /// If this flag is not specified, but the object has been opened for write access, the function fails.
      /// </summary>
      Write = 0x00000002,
      /// <summary>
      /// Enables subsequent open operations on an object to request delete access. 
      /// Otherwise, other processes cannot open the object if they request delete access.
      /// If this flag is not specified, but the object has been opened for delete access, the function fails.
      /// </summary>
      Delete = 0x00000004
    }

    public enum ECreationDisposition : uint {
      /// <summary>
      /// Creates a new file. The function fails if a specified file exists.
      /// </summary>
      New = 1,
      /// <summary>
      /// Creates a new file, always. 
      /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
      /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
      /// </summary>
      CreateAlways = 2,
      /// <summary>
      /// Opens a file. The function fails if the file does not exist. 
      /// </summary>
      OpenExisting = 3,
      /// <summary>
      /// Opens a file, always. 
      /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
      /// </summary>
      OpenAlways = 4,
      /// <summary>
      /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
      /// The calling process must open the file with the GENERIC_WRITE access right. 
      /// </summary>
      TruncateExisting = 5
    }

    /// <summary>
    /// IO Control Codes
    /// Useful links:
    ///     http://www.ioctls.net/
    ///     http://msdn.microsoft.com/en-us/library/windows/hardware/ff543023(v=vs.85).aspx
    /// </summary>
    [Flags]
    public enum EIOControlCode : uint {
      // STORAGE
      StorageCheckVerify = (EFileDevice.MassStorage << 16) | (0x0200 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageCheckVerify2 = (EFileDevice.MassStorage << 16) | (0x0200 << 2) | EMethod.Buffered | (0 << 14), // FileAccess.Any
      StorageMediaRemoval = (EFileDevice.MassStorage << 16) | (0x0201 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageEjectMedia = (EFileDevice.MassStorage << 16) | (0x0202 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageLoadMedia = (EFileDevice.MassStorage << 16) | (0x0203 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageLoadMedia2 = (EFileDevice.MassStorage << 16) | (0x0203 << 2) | EMethod.Buffered | (0 << 14),
      StorageReserve = (EFileDevice.MassStorage << 16) | (0x0204 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageRelease = (EFileDevice.MassStorage << 16) | (0x0205 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageFindNewDevices = (EFileDevice.MassStorage << 16) | (0x0206 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageEjectionControl = (EFileDevice.MassStorage << 16) | (0x0250 << 2) | EMethod.Buffered | (0 << 14),
      StorageMcnControl = (EFileDevice.MassStorage << 16) | (0x0251 << 2) | EMethod.Buffered | (0 << 14),
      StorageGetMediaTypes = (EFileDevice.MassStorage << 16) | (0x0300 << 2) | EMethod.Buffered | (0 << 14),
      StorageGetMediaTypesEx = (EFileDevice.MassStorage << 16) | (0x0301 << 2) | EMethod.Buffered | (0 << 14),
      StorageResetBus = (EFileDevice.MassStorage << 16) | (0x0400 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageResetDevice = (EFileDevice.MassStorage << 16) | (0x0401 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      StorageGetDeviceNumber = (EFileDevice.MassStorage << 16) | (0x0420 << 2) | EMethod.Buffered | (0 << 14),
      StoragePredictFailure = (EFileDevice.MassStorage << 16) | (0x0440 << 2) | EMethod.Buffered | (0 << 14),
      StorageObsoleteResetBus = (EFileDevice.MassStorage << 16) | (0x0400 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      StorageObsoleteResetDevice = (EFileDevice.MassStorage << 16) | (0x0401 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      // DISK
      DiskGetDriveGeometry = (EFileDevice.Disk << 16) | (0x0000 << 2) | EMethod.Buffered | (0 << 14),
      DiskGetDriveGeometryEx = (EFileDevice.Disk << 16) | (0x0028 << 2) | EMethod.Buffered | (0 << 14),
      DiskGetPartitionInfo = (EFileDevice.Disk << 16) | (0x0001 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskGetPartitionInfoEx = (EFileDevice.Disk << 16) | (0x0012 << 2) | EMethod.Buffered | (0 << 14),
      DiskSetPartitionInfo = (EFileDevice.Disk << 16) | (0x0002 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskGetDriveLayout = (EFileDevice.Disk << 16) | (0x0003 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskSetDriveLayout = (EFileDevice.Disk << 16) | (0x0004 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskVerify = (EFileDevice.Disk << 16) | (0x0005 << 2) | EMethod.Buffered | (0 << 14),
      DiskFormatTracks = (EFileDevice.Disk << 16) | (0x0006 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskReassignBlocks = (EFileDevice.Disk << 16) | (0x0007 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskPerformance = (EFileDevice.Disk << 16) | (0x0008 << 2) | EMethod.Buffered | (0 << 14),
      DiskIsWritable = (EFileDevice.Disk << 16) | (0x0009 << 2) | EMethod.Buffered | (0 << 14),
      DiskLogging = (EFileDevice.Disk << 16) | (0x000a << 2) | EMethod.Buffered | (0 << 14),
      DiskFormatTracksEx = (EFileDevice.Disk << 16) | (0x000b << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskHistogramStructure = (EFileDevice.Disk << 16) | (0x000c << 2) | EMethod.Buffered | (0 << 14),
      DiskHistogramData = (EFileDevice.Disk << 16) | (0x000d << 2) | EMethod.Buffered | (0 << 14),
      DiskHistogramReset = (EFileDevice.Disk << 16) | (0x000e << 2) | EMethod.Buffered | (0 << 14),
      DiskRequestStructure = (EFileDevice.Disk << 16) | (0x000f << 2) | EMethod.Buffered | (0 << 14),
      DiskRequestData = (EFileDevice.Disk << 16) | (0x0010 << 2) | EMethod.Buffered | (0 << 14),
      DiskControllerNumber = (EFileDevice.Disk << 16) | (0x0011 << 2) | EMethod.Buffered | (0 << 14),
      DiskSmartGetVersion = (EFileDevice.Disk << 16) | (0x0020 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskSmartSendDriveCommand = (EFileDevice.Disk << 16) | (0x0021 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskSmartRcvDriveData = (EFileDevice.Disk << 16) | (0x0022 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskUpdateDriveSize = (EFileDevice.Disk << 16) | (0x0032 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskGrowPartition = (EFileDevice.Disk << 16) | (0x0034 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskGetCacheInformation = (EFileDevice.Disk << 16) | (0x0035 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskSetCacheInformation = (EFileDevice.Disk << 16) | (0x0036 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskDeleteDriveLayout = (EFileDevice.Disk << 16) | (0x0040 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskFormatDrive = (EFileDevice.Disk << 16) | (0x00f3 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskSenseDevice = (EFileDevice.Disk << 16) | (0x00f8 << 2) | EMethod.Buffered | (0 << 14),
      DiskCheckVerify = (EFileDevice.Disk << 16) | (0x0200 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskMediaRemoval = (EFileDevice.Disk << 16) | (0x0201 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskEjectMedia = (EFileDevice.Disk << 16) | (0x0202 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskLoadMedia = (EFileDevice.Disk << 16) | (0x0203 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskReserve = (EFileDevice.Disk << 16) | (0x0204 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskRelease = (EFileDevice.Disk << 16) | (0x0205 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskFindNewDevices = (EFileDevice.Disk << 16) | (0x0206 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      DiskGetMediaTypes = (EFileDevice.Disk << 16) | (0x0300 << 2) | EMethod.Buffered | (0 << 14),
      DiskSetPartitionInfoEx = (EFileDevice.Disk << 16) | (0x0013 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskGetDriveLayoutEx = (EFileDevice.Disk << 16) | (0x0014 << 2) | EMethod.Buffered | (0 << 14),
      DiskSetDriveLayoutEx = (EFileDevice.Disk << 16) | (0x0015 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskCreateDisk = (EFileDevice.Disk << 16) | (0x0016 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      DiskGetLengthInfo = (EFileDevice.Disk << 16) | (0x0017 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      // CHANGER
      ChangerGetParameters = (EFileDevice.Changer << 16) | (0x0000 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      ChangerGetStatus = (EFileDevice.Changer << 16) | (0x0001 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      ChangerGetProductData = (EFileDevice.Changer << 16) | (0x0002 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      ChangerSetAccess = (EFileDevice.Changer << 16) | (0x0004 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      ChangerGetElementStatus = (EFileDevice.Changer << 16) | (0x0005 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      ChangerInitializeElementStatus = (EFileDevice.Changer << 16) | (0x0006 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      ChangerSetPosition = (EFileDevice.Changer << 16) | (0x0007 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      ChangerExchangeMedium = (EFileDevice.Changer << 16) | (0x0008 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      ChangerMoveMedium = (EFileDevice.Changer << 16) | (0x0009 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      ChangerReinitializeTarget = (EFileDevice.Changer << 16) | (0x000A << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      ChangerQueryVolumeTags = (EFileDevice.Changer << 16) | (0x000B << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      // FILESYSTEM
      FsctlRequestOplockLevel1 = (EFileDevice.FileSystem << 16) | (0 << 2) | EMethod.Buffered | (0 << 14),
      FsctlRequestOplockLevel2 = (EFileDevice.FileSystem << 16) | (1 << 2) | EMethod.Buffered | (0 << 14),
      FsctlRequestBatchOplock = (EFileDevice.FileSystem << 16) | (2 << 2) | EMethod.Buffered | (0 << 14),
      FsctlOplockBreakAcknowledge = (EFileDevice.FileSystem << 16) | (3 << 2) | EMethod.Buffered | (0 << 14),
      FsctlOpBatchAckClosePending = (EFileDevice.FileSystem << 16) | (4 << 2) | EMethod.Buffered | (0 << 14),
      FsctlOplockBreakNotify = (EFileDevice.FileSystem << 16) | (5 << 2) | EMethod.Buffered | (0 << 14),
      FsctlLockVolume = (EFileDevice.FileSystem << 16) | (6 << 2) | EMethod.Buffered | (0 << 14),
      FsctlUnlockVolume = (EFileDevice.FileSystem << 16) | (7 << 2) | EMethod.Buffered | (0 << 14),
      FsctlDismountVolume = (EFileDevice.FileSystem << 16) | (8 << 2) | EMethod.Buffered | (0 << 14),
      FsctlIsVolumeMounted = (EFileDevice.FileSystem << 16) | (10 << 2) | EMethod.Buffered | (0 << 14),
      FsctlIsPathnameValid = (EFileDevice.FileSystem << 16) | (11 << 2) | EMethod.Buffered | (0 << 14),
      FsctlMarkVolumeDirty = (EFileDevice.FileSystem << 16) | (12 << 2) | EMethod.Buffered | (0 << 14),
      FsctlQueryRetrievalPointers = (EFileDevice.FileSystem << 16) | (14 << 2) | EMethod.Neither | (0 << 14),
      FsctlGetCompression = (EFileDevice.FileSystem << 16) | (15 << 2) | EMethod.Buffered | (0 << 14),
      FsctlSetCompression = (EFileDevice.FileSystem << 16) | (16 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      FsctlMarkAsSystemHive = (EFileDevice.FileSystem << 16) | (19 << 2) | EMethod.Neither | (0 << 14),
      FsctlOplockBreakAckNo2 = (EFileDevice.FileSystem << 16) | (20 << 2) | EMethod.Buffered | (0 << 14),
      FsctlInvalidateVolumes = (EFileDevice.FileSystem << 16) | (21 << 2) | EMethod.Buffered | (0 << 14),
      FsctlQueryFatBpb = (EFileDevice.FileSystem << 16) | (22 << 2) | EMethod.Buffered | (0 << 14),
      FsctlRequestFilterOplock = (EFileDevice.FileSystem << 16) | (23 << 2) | EMethod.Buffered | (0 << 14),
      FsctlFileSystemGetStatistics = (EFileDevice.FileSystem << 16) | (24 << 2) | EMethod.Buffered | (0 << 14),
      FsctlGetNtfsVolumeData = (EFileDevice.FileSystem << 16) | (25 << 2) | EMethod.Buffered | (0 << 14),
      FsctlGetNtfsFileRecord = (EFileDevice.FileSystem << 16) | (26 << 2) | EMethod.Buffered | (0 << 14),
      FsctlGetVolumeBitmap = (EFileDevice.FileSystem << 16) | (27 << 2) | EMethod.Neither | (0 << 14),
      FsctlGetRetrievalPointers = (EFileDevice.FileSystem << 16) | (28 << 2) | EMethod.Neither | (0 << 14),
      FsctlMoveFile = (EFileDevice.FileSystem << 16) | (29 << 2) | EMethod.Buffered | (0 << 14),
      FsctlIsVolumeDirty = (EFileDevice.FileSystem << 16) | (30 << 2) | EMethod.Buffered | (0 << 14),
      FsctlGetHfsInformation = (EFileDevice.FileSystem << 16) | (31 << 2) | EMethod.Buffered | (0 << 14),
      FsctlAllowExtendedDasdIo = (EFileDevice.FileSystem << 16) | (32 << 2) | EMethod.Neither | (0 << 14),
      FsctlReadPropertyData = (EFileDevice.FileSystem << 16) | (33 << 2) | EMethod.Neither | (0 << 14),
      FsctlWritePropertyData = (EFileDevice.FileSystem << 16) | (34 << 2) | EMethod.Neither | (0 << 14),
      FsctlFindFilesBySid = (EFileDevice.FileSystem << 16) | (35 << 2) | EMethod.Neither | (0 << 14),
      FsctlDumpPropertyData = (EFileDevice.FileSystem << 16) | (37 << 2) | EMethod.Neither | (0 << 14),
      FsctlSetObjectId = (EFileDevice.FileSystem << 16) | (38 << 2) | EMethod.Buffered | (0 << 14),
      FsctlGetObjectId = (EFileDevice.FileSystem << 16) | (39 << 2) | EMethod.Buffered | (0 << 14),
      FsctlDeleteObjectId = (EFileDevice.FileSystem << 16) | (40 << 2) | EMethod.Buffered | (0 << 14),
      FsctlSetReparsePoint = (EFileDevice.FileSystem << 16) | (41 << 2) | EMethod.Buffered | (0 << 14),
      FsctlGetReparsePoint = (EFileDevice.FileSystem << 16) | (42 << 2) | EMethod.Buffered | (0 << 14),
      FsctlDeleteReparsePoint = (EFileDevice.FileSystem << 16) | (43 << 2) | EMethod.Buffered | (0 << 14),
      FsctlEnumUsnData = (EFileDevice.FileSystem << 16) | (44 << 2) | EMethod.Neither | (0 << 14),
      FsctlSecurityIdCheck = (EFileDevice.FileSystem << 16) | (45 << 2) | EMethod.Neither | (FileAccess.Read << 14),
      FsctlReadUsnJournal = (EFileDevice.FileSystem << 16) | (46 << 2) | EMethod.Neither | (0 << 14),
      FsctlSetObjectIdExtended = (EFileDevice.FileSystem << 16) | (47 << 2) | EMethod.Buffered | (0 << 14),
      FsctlCreateOrGetObjectId = (EFileDevice.FileSystem << 16) | (48 << 2) | EMethod.Buffered | (0 << 14),
      FsctlSetSparse = (EFileDevice.FileSystem << 16) | (49 << 2) | EMethod.Buffered | (0 << 14),
      FsctlSetZeroData = (EFileDevice.FileSystem << 16) | (50 << 2) | EMethod.Buffered | (FileAccess.Write << 14),
      FsctlQueryAllocatedRanges = (EFileDevice.FileSystem << 16) | (51 << 2) | EMethod.Neither | (FileAccess.Read << 14),
      FsctlEnableUpgrade = (EFileDevice.FileSystem << 16) | (52 << 2) | EMethod.Buffered | (FileAccess.Write << 14),
      FsctlSetEncryption = (EFileDevice.FileSystem << 16) | (53 << 2) | EMethod.Neither | (0 << 14),
      FsctlEncryptionFsctlIo = (EFileDevice.FileSystem << 16) | (54 << 2) | EMethod.Neither | (0 << 14),
      FsctlWriteRawEncrypted = (EFileDevice.FileSystem << 16) | (55 << 2) | EMethod.Neither | (0 << 14),
      FsctlReadRawEncrypted = (EFileDevice.FileSystem << 16) | (56 << 2) | EMethod.Neither | (0 << 14),
      FsctlCreateUsnJournal = (EFileDevice.FileSystem << 16) | (57 << 2) | EMethod.Neither | (0 << 14),
      FsctlReadFileUsnData = (EFileDevice.FileSystem << 16) | (58 << 2) | EMethod.Neither | (0 << 14),
      FsctlWriteUsnCloseRecord = (EFileDevice.FileSystem << 16) | (59 << 2) | EMethod.Neither | (0 << 14),
      FsctlExtendVolume = (EFileDevice.FileSystem << 16) | (60 << 2) | EMethod.Buffered | (0 << 14),
      FsctlQueryUsnJournal = (EFileDevice.FileSystem << 16) | (61 << 2) | EMethod.Buffered | (0 << 14),
      FsctlDeleteUsnJournal = (EFileDevice.FileSystem << 16) | (62 << 2) | EMethod.Buffered | (0 << 14),
      FsctlMarkHandle = (EFileDevice.FileSystem << 16) | (63 << 2) | EMethod.Buffered | (0 << 14),
      FsctlSisCopyFile = (EFileDevice.FileSystem << 16) | (64 << 2) | EMethod.Buffered | (0 << 14),
      FsctlSisLinkFiles = (EFileDevice.FileSystem << 16) | (65 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      FsctlHsmMsg = (EFileDevice.FileSystem << 16) | (66 << 2) | EMethod.Buffered | (FileAccess.ReadWrite << 14),
      FsctlNssControl = (EFileDevice.FileSystem << 16) | (67 << 2) | EMethod.Buffered | (FileAccess.Write << 14),
      FsctlHsmData = (EFileDevice.FileSystem << 16) | (68 << 2) | EMethod.Neither | (FileAccess.ReadWrite << 14),
      FsctlRecallFile = (EFileDevice.FileSystem << 16) | (69 << 2) | EMethod.Neither | (0 << 14),
      FsctlNssRcontrol = (EFileDevice.FileSystem << 16) | (70 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
      // VIDEO
      VideoQuerySupportedBrightness = (EFileDevice.Video << 16) | (0x0125 << 2) | EMethod.Buffered | (0 << 14),
      VideoQueryDisplayBrightness = (EFileDevice.Video << 16) | (0x0126 << 2) | EMethod.Buffered | (0 << 14),
      VideoSetDisplayBrightness = (EFileDevice.Video << 16) | (0x0127 << 2) | EMethod.Buffered | (0 << 14)
    }

    [Flags]
    public enum EFileDevice : uint {
      Beep = 0x00000001,
      CDRom = 0x00000002,
      CDRomFileSytem = 0x00000003,
      Controller = 0x00000004,
      Datalink = 0x00000005,
      Dfs = 0x00000006,
      Disk = 0x00000007,
      DiskFileSystem = 0x00000008,
      FileSystem = 0x00000009,
      InPortPort = 0x0000000a,
      Keyboard = 0x0000000b,
      Mailslot = 0x0000000c,
      MidiIn = 0x0000000d,
      MidiOut = 0x0000000e,
      Mouse = 0x0000000f,
      MultiUncProvider = 0x00000010,
      NamedPipe = 0x00000011,
      Network = 0x00000012,
      NetworkBrowser = 0x00000013,
      NetworkFileSystem = 0x00000014,
      Null = 0x00000015,
      ParallelPort = 0x00000016,
      PhysicalNetcard = 0x00000017,
      Printer = 0x00000018,
      Scanner = 0x00000019,
      SerialMousePort = 0x0000001a,
      SerialPort = 0x0000001b,
      Screen = 0x0000001c,
      Sound = 0x0000001d,
      Streams = 0x0000001e,
      Tape = 0x0000001f,
      TapeFileSystem = 0x00000020,
      Transport = 0x00000021,
      Unknown = 0x00000022,
      Video = 0x00000023,
      VirtualDisk = 0x00000024,
      WaveIn = 0x00000025,
      WaveOut = 0x00000026,
      Port8042 = 0x00000027,
      NetworkRedirector = 0x00000028,
      Battery = 0x00000029,
      BusExtender = 0x0000002a,
      Modem = 0x0000002b,
      Vdm = 0x0000002c,
      MassStorage = 0x0000002d,
      Smb = 0x0000002e,
      Ks = 0x0000002f,
      Changer = 0x00000030,
      Smartcard = 0x00000031,
      Acpi = 0x00000032,
      Dvd = 0x00000033,
      FullscreenVideo = 0x00000034,
      DfsFileSystem = 0x00000035,
      DfsVolume = 0x00000036,
      Serenum = 0x00000037,
      Termsrv = 0x00000038,
      Ksec = 0x00000039,
      // From Windows Driver Kit 7
      Fips = 0x0000003A,
      Infiniband = 0x0000003B,
      Vmbus = 0x0000003E,
      CryptProvider = 0x0000003F,
      Wpd = 0x00000040,
      Bluetooth = 0x00000041,
      MtComposite = 0x00000042,
      MtTransport = 0x00000043,
      Biometric = 0x00000044,
      Pmi = 0x00000045
    }

    [Flags]
    public enum EMethod : uint {
      Buffered = 0,
      InDirect = 1,
      OutDirect = 2,
      Neither = 3
    }

    /// <remarks>
    /// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct REPARSE_DATA_BUFFER_HEADER {
      public ReparseTagType ReparseTag;
      public ushort ReparseDataLength;
      public ushort Reserved;
    }

    /// <remarks>
    /// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SymbolicLinkReparseData {
      private const int MaxUnicodePathLength = 0x3FF0;

      public REPARSE_DATA_BUFFER_HEADER Header;

      /// <summary>
      /// Offset, in bytes, of the substitute name string in the PathBuffer array. Note that this offset must be divided by sizeof(WCHAR) to get the array index.
      /// </summary>
      public ushort SubstituteNameOffset;
      /// <summary>
      /// Length, in bytes, of the substitute name string. If this string is NULL-terminated, SubstituteNameLength does not include space for the UNICODE_NULL character.
      /// </summary>
      public ushort SubstituteNameLength;
      /// <summary>
      /// Offset, in bytes, of the print name string in the PathBuffer array. Note that this offset must be divided by sizeof(WCHAR) to get the array index.
      /// </summary>
      public ushort PrintNameOffset;
      /// <summary>
      /// Length, in bytes, of the print name string. If this string is NULL-terminated, PrintNameLength does not include space for the UNICODE_NULL character.
      /// </summary>
      public ushort PrintNameLength;
      public SymbolicLinkFlags Flags;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxUnicodePathLength)]
      public char[] PathBuffer;
    }

    /// <remarks>
    /// See https://msdn.microsoft.com/en-us/library/windows/hardware/ff552012(v=vs.85).aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MountPointReparseBuffer {
      public const int MaxUnicodePathLength = 0x3FF0;

      public REPARSE_DATA_BUFFER_HEADER Header;

      /// <summary>
      /// Offset, in bytes, of the substitute name string in the PathBuffer array. Note that this offset must be divided by sizeof(WCHAR) to get the array index.
      /// </summary>
      public ushort SubstituteNameOffset;
      /// <summary>
      /// Length, in bytes, of the substitute name string. If this string is NULL-terminated, SubstituteNameLength does not include space for the UNICODE_NULL character.
      /// </summary>
      public ushort SubstituteNameLength;
      /// <summary>
      /// Offset, in bytes, of the print name string in the PathBuffer array. Note that this offset must be divided by sizeof(WCHAR) to get the array index.
      /// </summary>
      public ushort PrintNameOffset;
      /// <summary>
      /// Length, in bytes, of the print name string. If this string is NULL-terminated, PrintNameLength does not include space for the UNICODE_NULL character.
      /// </summary>
      public ushort PrintNameLength;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxUnicodePathLength)]
      public char[] PathBuffer;
    }

    /// <remarks>
    /// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct GenericReparseBuffer {
      private const int MaxByteCount = 0x7FF0;

      public REPARSE_DATA_BUFFER_HEADER Header;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxByteCount)]
      public byte[] DataBuffer;
    }

    [Flags]
    public enum SymbolicLinkFlags : uint {
      SYMLINK_FLAG_RELATIVE = 0x01
    }

    public enum ReparseTagType : uint {
      IO_REPARSE_TAG_MOUNT_POINT = (0xA0000003),
      IO_REPARSE_TAG_HSM = (0xC0000004),
      IO_REPARSE_TAG_SIS = (0x80000007),
      IO_REPARSE_TAG_DFS = (0x8000000A),
      IO_REPARSE_TAG_SYMLINK = (0xA000000C),
      IO_REPARSE_TAG_DFSR = (0x80000012),
    }

    public enum GET_FILEEX_INFO_LEVELS {
      GetFileExInfoStandard,
      GetFileExMaxInfoLevel
    }

    public static T PtrToStructure<T>(IntPtr ptr) {
      return (T)Marshal.PtrToStructure(ptr, typeof(T));
    }
  }
}