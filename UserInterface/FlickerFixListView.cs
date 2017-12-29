using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SteamLibraryExplorer.UserInterface {
  public partial class FlickerFixListView : ListView {
    private bool _explorerStyleTheme;

    public FlickerFixListView() {
      //Activate double buffering
      SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

      //Enable the OnNotifyMessage event so we get a chance to filter out 
      // Windows messages before they get to the form's WndProc
      SetStyle(ControlStyles.EnableNotifyMessage, true);

      HandleCreated += OnHandleCreated;
      InitializeComponent();
    }

    /// <summary>
    /// Set listview to look like Vista/Win7 style
    /// </summary>
    public bool ExplorerStyleTheme {
      get { return _explorerStyleTheme; }
      set {
        _explorerStyleTheme = value;
        if (IsHandleCreated) {
          SetExplorerWindowTheme(_explorerStyleTheme);
        }
      }
    }

    private void SetExplorerWindowTheme(bool explorerStyle) {
      string style = explorerStyle ? "Explorer" : null;
      int hr = SetWindowTheme(Handle, style, null);
      if (hr < 0) {
        Marshal.ThrowExceptionForHR(hr);
      }
    }

    private void OnHandleCreated(object sender, EventArgs eventArgs) {
      SetExplorerWindowTheme(_explorerStyleTheme);
    }

    protected override void OnNotifyMessage(Message m) {
      //Filter out the WM_ERASEBKGND message
      if (m.Msg != 0x14) {
        base.OnNotifyMessage(m);
      }
    }

    public GroupState GetGroupState(ListViewGroup group) {
      var groupId = GetGroupId(group);

      LVGROUP lvgroup = new LVGROUP();
      lvgroup.cbSize = Marshal.SizeOf(lvgroup);
      lvgroup.mask = (int)GroupMask.LVGF_STATE;
      lvgroup.stateMask = (int)GroupState.All;
      IntPtr ip = IntPtr.Zero;
      try {
        ip = Marshal.AllocHGlobal(lvgroup.cbSize);
        Marshal.StructureToPtr(lvgroup, ip, false);
        int result = SendMessage(Handle, (int)ListViewMessage.LVM_GETGROUPINFO, groupId, ip);
        if (result != groupId) {
          throw new InvalidOperationException("Error setting list view group state");
        }
        lvgroup = (LVGROUP)Marshal.PtrToStructure(ip, typeof(LVGROUP));
        return (GroupState)lvgroup.state;
      }
      finally {
        if (ip != IntPtr.Zero) Marshal.FreeHGlobal(ip);
      }
    }

    public void SetGroupState(ListViewGroup group, GroupState state) {
      var groupId = GetGroupId(group);

      LVGROUP lvgroup = new LVGROUP();
      lvgroup.cbSize = Marshal.SizeOf(lvgroup);
      lvgroup.mask = (uint)(GroupMask.LVGF_STATE | GroupMask.LVGF_ALIGN);
      lvgroup.uAlign = (uint) (GroupAlign.LVGA_FOOTER_LEFT);
      lvgroup.state = (uint)state;
      lvgroup.stateMask = (int)GroupState.All;
      lvgroup.iGroupId = groupId;
      IntPtr ip = IntPtr.Zero;
      try {
        ip = Marshal.AllocHGlobal(lvgroup.cbSize);
        Marshal.StructureToPtr(lvgroup, ip, false);
        int result = SendMessage(Handle, (int)ListViewMessage.LVM_SETGROUPINFO, groupId, ip);
        if (result != groupId) {
          throw new InvalidOperationException("Error setting list view group state");
        }
      }
      finally {
        if (ip != IntPtr.Zero) Marshal.FreeHGlobal(ip);
      }
    }

    private int GetGroupId(ListViewGroup group) {
      if (!this.IsHandleCreated) {
        throw new InvalidOperationException("ListView is not created yet");
      }

      int index = Groups.IndexOf(group);
      if (index < 0) {
        throw new InvalidOperationException("Group is not a member of the group collection");
      }
      var lvgroup = new LVGROUP();
      lvgroup.cbSize = Marshal.SizeOf(lvgroup);
      lvgroup.mask = (int)GroupMask.LVGF_GROUPID;
      var ip = IntPtr.Zero;
      try {
        ip = Marshal.AllocHGlobal(lvgroup.cbSize);
        Marshal.StructureToPtr(lvgroup, ip, false);
        var result = SendMessage(Handle, (int)ListViewMessage.LVM_GETGROUPINFOBYINDEX, index, ip);
        if (result == 0) {
          throw new InvalidOperationException("Error retrieving group ID by index");
        }
        lvgroup = (LVGROUP)Marshal.PtrToStructure(ip, typeof(LVGROUP));
        return lvgroup.iGroupId;
      }
      finally {
        if (ip != IntPtr.Zero) {
          Marshal.FreeHGlobal(ip);
        }
      }
    }

    [DllImport("user32.dll")]
    static extern int SendMessage(IntPtr window, int message, int wParam, IntPtr lParam);

    [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [Flags]
    private enum GroupMask {
      LVGF_NONE = 0x00000000,
      LVGF_HEADER = 0x00000001,
      LVGF_FOOTER = 0x00000002,
      LVGF_STATE = 0x00000004,
      LVGF_ALIGN = 0x00000008,
      LVGF_GROUPID = 0x00000010,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [Flags]
    public enum GroupState {
      LVGS_NORMAL = 0x0000,
      LVGS_COLLAPSED = 0x0001,
      LVGS_COLLAPSIBLE = 0x0008,
      All = LVGS_COLLAPSED | LVGS_COLLAPSIBLE,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [Flags]
    public enum GroupAlign {
      LVGA_HEADER_LEFT = 0x00000001,
      LVGA_HEADER_CENTER = 0x00000002,
      LVGA_HEADER_RIGHT = 0x00000004,
      LVGA_FOOTER_LEFT = 0x00000008,
      LVGA_FOOTER_CENTER = 0x00000010,
      LVGA_FOOTER_RIGHT = 0x00000020,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ListViewMessage {
      LVM_FIRST = 0x1000,
      LVM_SETGROUPINFO = LVM_FIRST + 147,
      LVM_GETGROUPINFO = LVM_FIRST + 149,
      LVM_GETGROUPINFOBYINDEX = LVM_FIRST + 153,
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class LVGROUP {
      public int cbSize = (int)Marshal.SizeOf(typeof(LVGROUP));
      public uint mask;
      public IntPtr pszHeader;
      public int cchHeader;
      public IntPtr pszFooter = IntPtr.Zero;
      public int cchFooter = 0;
      public int iGroupId;
      public uint stateMask = 0;
      public uint state = 0;
      public uint uAlign;

      public override string ToString() {
        return "LVGROUP: header = " + pszHeader.ToString() + ", iGroupId = " + iGroupId.ToString(CultureInfo.InvariantCulture);
      }
    }
  }
}
