using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace SteamLibraryExplorer.Utils {
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  public class FileUtils {
    public static void CopyFile(string sourceFile, string destinationFile,
      bool largeFile, Action<CopyFileProgress> progress, CancellationToken cancellationToken) {

      CopyProgressRoutine copyProgress = (totalFileSize, totalBytesTransferred, streamSize, streamBytesTransferred, streamNumber, callbackReason, hSourceFile,
        hDestinationFile, data) => {
        if (cancellationToken.IsCancellationRequested) {
          return CopyProgressResult.PROGRESS_CANCEL;
        }
        progress(new CopyFileProgress {
          TotalBytesTransferred = totalBytesTransferred,
          TotalFileSize = totalFileSize
        });

        return CopyProgressResult.PROGRESS_CONTINUE;
      };

      var cancel = 0;
      var flags = CopyFileFlags.None;
      if (largeFile) {
        // For large file, don't buffer to avoid exhausing memory
        // See http://blog.mischel.com/2008/10/14/copying-large-files-on-windows/
        flags |= CopyFileFlags.COPY_FILE_NO_BUFFERING;
      }

      var sourcePath = MakeLongPath(sourceFile);
      var destinationPath = MakeLongPath(destinationFile);

      var success = CopyFileEx(sourcePath, destinationPath, copyProgress, IntPtr.Zero, ref cancel, flags);
      if (!success) {
        var error = Marshal.GetLastWin32Error();
        if (error == ERROR_REQUEST_ABORTED) {
          throw new OperationCanceledException(cancellationToken);
        }
        throw new Win32Exception(error, string.Format("Error copying file \"{0}\" to \"{1}\"", sourceFile, destinationFile));
      }
    }

    private const string LongPathPrefix = @"\\?\";
    private const string LongPathPrefixUNC = @"\\?\UNC\";

    private static string MakeLongPath(string path) {
      if (path.StartsWith("\\")) {
        // Convert from "\\server\share" to "\\?\UNC\server\share"
        return LongPathPrefixUNC + path.Substring(2);
      }
      return LongPathPrefix + path;
    }

    private const int ERROR_REQUEST_ABORTED = 1235;

    private delegate CopyProgressResult CopyProgressRoutine(
      long TotalFileSize,
      long TotalBytesTransferred,
      long StreamSize,
      long StreamBytesTransferred,
      uint dwStreamNumber,
      CopyProgressCallbackReason dwCallbackReason,
      IntPtr hSourceFile,
      IntPtr hDestinationFile,
      IntPtr lpData);

    private enum CopyProgressResult : uint {
      /// <summary>
      /// Continue the copy operation.
      /// </summary>
      PROGRESS_CONTINUE = 0,
      /// <summary>
      /// Cancel the copy operation and delete the destination file.
      /// </summary>
      PROGRESS_CANCEL = 1,
      /// <summary>
      /// Stop the copy operation. It can be restarted at a later time.
      /// </summary>
      PROGRESS_STOP = 2,
      /// <summary>
      /// Continue the copy operation, but stop invoking CopyProgressRoutine to report progress.
      /// </summary>
      PROGRESS_QUIET = 3
    }

    private enum CopyProgressCallbackReason : uint {
      CALLBACK_CHUNK_FINISHED = 0x00000000,
      CALLBACK_STREAM_SWITCH = 0x00000001
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName,
      CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel,
      CopyFileFlags dwCopyFlags);

    [Flags]
    private enum CopyFileFlags : uint {
      None = 0x00,
      COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
      COPY_FILE_RESTARTABLE = 0x00000002,
      COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
      COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,
      COPY_FILE_COPY_SYMLINK = 0x00000800, //NT 6.0+
      COPY_FILE_NO_BUFFERING = 0x00001000 //NT 6.0+
    }
  }

  public class CopyFileProgress {
    public long TotalBytesTransferred { get; set; }
    public long TotalFileSize { get; set; }
  }
}
