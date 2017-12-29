using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SteamLibraryExplorer.UserInterface;
using SteamLibraryExplorer.SteamUtil;

namespace SteamLibraryExplorer {
  public class View {
    private readonly MainForm _mainForm;
    private readonly Model _model;
    private readonly IDictionary<string, ItemToRefresh> _itemsToRefresh = new Dictionary<string, ItemToRefresh>(StringComparer.OrdinalIgnoreCase);
    private readonly IDictionary<string, GroupToRefresh> _groupsToRefresh = new Dictionary<string, GroupToRefresh>(StringComparer.OrdinalIgnoreCase);
    private readonly object _itemsToRefreshLock = new object();

    public View(Model model, MainForm mainForm) {
      _model = model;
      _mainForm = mainForm;
    }

    public event EventHandler FirstShow;
    public event EventHandler RefreshView;

    public void Run() {
      _mainForm.Load += (sender, args) => OnFirstShow();
      _mainForm.refreshToolStripMenuItem.Click += (sender, args) => OnRefreshView();
      _model.SteamConfiguration.Location.ValueChanged += (sender, arg) => ShowSteamLocation(arg.NewValue);
      _model.SteamConfiguration.SteamLibraries.CollectionChanged += SteamLibraries_CollectionChanged;
      _mainForm.refreshListViewTimer.Tick += RefreshListViewTimerOnTick;
      _mainForm.refreshListViewTimer.Enabled = true;
      Application.Run(_mainForm);
    }

    private void RefreshListViewTimerOnTick(object o, EventArgs eventArgs) {
      List<ItemToRefresh> itemsToRefresh;
      List<GroupToRefresh> groupsToRefresh;
      lock (_itemsToRefreshLock) {
        if (_itemsToRefresh.Count == 0 && _groupsToRefresh.Count == 0) {
          return;
        }

        itemsToRefresh = _itemsToRefresh.Values.ToList();
        _itemsToRefresh.Clear();

        groupsToRefresh = _groupsToRefresh.Values.ToList();
        _groupsToRefresh.Clear();
      }

      _mainForm.gameLibrariesListView.BeginUpdate();
      try {
        foreach (var x in itemsToRefresh) {
          x.Item.Text = x.Text;
        }
        foreach (var x in groupsToRefresh) {
          x.ListViewGroup.Header = x.Text;
        }
        if (groupsToRefresh.Count > 0) {
          // Note: We have to re-set the state of all groups, because the ListVie class overwrites
          // our custom LVGS_COLLAPSIBLE state everytime the header changes
          foreach (ListViewGroup group in _mainForm.gameLibrariesListView.Groups) {
            _mainForm.gameLibrariesListView.SetGroupState(group, FlickerFixListView.GroupState.LVGS_COLLAPSIBLE);
          }
        }
      }
      finally {
        _mainForm.gameLibrariesListView.EndUpdate();
      }
    }

    private void SteamLibraries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
          AddGameLibrary((SteamLibrary)e.NewItems[0]);
          break;
        case NotifyCollectionChangedAction.Reset:
          ClearGameLibraries();
          break;
        default:
          throw new ArgumentOutOfRangeException("e", "Unsupported collection change");
      }

    }

    private void AddGameLibrary(SteamLibrary library) {
      _mainForm.gameLibrariesListView.BeginUpdate();
      try {
        var group = new ListViewGroup();
        group.Tag = library;
        group.Header = GetGroupHeaderText(library);
        library.FreeDiskSize.ValueChanged += (sender, arg) => {
          lock (_itemsToRefreshLock) {
            _groupsToRefresh[library.Location.FullName] = new GroupToRefresh(group, GetGroupHeaderText(library));
          }
        };
        _mainForm.gameLibrariesListView.Groups.Add(group);

        foreach (var game in library.Games.OrderBy(x => x.DisplayName)) {
          var item = new ListViewItem();
          item.UseItemStyleForSubItems = false;
          item.Tag = game;
          item.Group = group;

          // Name
          item.Text = game.DisplayName;

          // ACF file
          var acfFileItem = item.SubItems.Add(game.AcfFile == null
            ? "<Missing>"
            : game.AcfFile.FileInfo.GetRelativePathTo(library.Location));
          if (game.AcfFile == null) {
            acfFileItem.ForeColor = Color.Red;
          }

          // Location
          var locationItem = item.SubItems.Add(game.Location == null
            ? "<Not found>"
            : game.Location.GetRelativePathTo(library.Location));
          if (game.Location == null) {
            locationItem.ForeColor = Color.Red;
          }

          // Size
          var sizeItem = item.SubItems.Add(HumanReadableDiskSize(game.SizeOnDisk.Value));
          game.SizeOnDisk.ValueChanged += (sender, arg) => {
            lock (_itemsToRefreshLock) {
              _itemsToRefresh[game.Location.FullName + "-size"] = new ItemToRefresh(sizeItem, HumanReadableDiskSize(arg.NewValue));
            }
          };
          if (game.Location == null) {
            sizeItem.ForeColor = Color.Red;
          }

          // Size
          var fileCountItem = item.SubItems.Add(HumanReadableFileCount(game.FileCount.Value));
          game.FileCount.ValueChanged += (sender, arg) => {
            lock (_itemsToRefreshLock) {
              _itemsToRefresh[game.Location.FullName + "-count"] = new ItemToRefresh(fileCountItem, HumanReadableFileCount(arg.NewValue));
            }
          };
          if (game.Location == null) {
            fileCountItem.ForeColor = Color.Red;
          }

          // Add item
          _mainForm.gameLibrariesListView.Items.Add(item);
        }

        _mainForm.gameLibrariesListView.SetGroupState(group, FlickerFixListView.GroupState.LVGS_COLLAPSIBLE);
      }
      finally {
        _mainForm.gameLibrariesListView.EndUpdate();
      }
    }

    private static string GetGroupHeaderText(SteamLibrary library) {
      if (library.TotalDiskSize.Value > 0 && library.FreeDiskSize.Value > 0) {
        return string.Format("{0} - {1} of {2} available",
          library.DisplayName,
          HumanReadableDiskSize(library.FreeDiskSize.Value),
          HumanReadableDiskSize(library.TotalDiskSize.Value));
      }
      return library.DisplayName;
    }

    private static string HumanReadableFileCount(long value) {
      return string.Format("{0:n0}", value);
    }

    private static string HumanReadableDiskSize(long value) {
      if (value <= 0) {
        return "0 KB";
      }
      if (value <= 1024) {
        return string.Format("1 KB");
      }
      if (value < 1024 * 1024) {
        return string.Format("{0:n0} KB", value / 1024);
      }
      if (value < 1024 * 1024 * 1024) {
        return string.Format("{0:n2} MB", (double)value / 1024 / 1024);
      }
      return string.Format("{0:n2} GB", (double)value / 1024 / 1024 / 1024);

      //if (value < 1024 * 1024) {
      //  return string.Format("{0:n0} KB ({1:n0} Bytes)", value / 1024, value);
      //}
      //if (value < 1024 * 1024 * 1024) {
      //  return string.Format("{0:n0} MB ({1:n0} Bytes)", value / 1024 / 1024, value);
      //}
      //return string.Format("{0:n2} GB ({1:n0} Bytes)", (double)value / 1024 / 1024 / 1024, value);
    }

    private void ClearGameLibraries() {
      _mainForm.gameLibrariesListView.Items.Clear();
    }

    public void ShowSteamLocation(DirectoryInfo directoryInfo) {
      //_mainForm.steamLocationTextBox.Text = directoryInfo.FullName;
      if (directoryInfo == null) {
        _mainForm.toolbarLabel.Text = "Steam location: <Not found>";
        return;
      }

      _mainForm.toolbarLabel.Text = string.Format("Steam location: {0}", directoryInfo.FullName);
    }

    protected virtual void OnFirstShow() {
      FirstShow?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnRefreshView() {
      RefreshView?.Invoke(this, EventArgs.Empty);
    }

    private class ItemToRefresh {
      public ItemToRefresh(ListViewItem.ListViewSubItem item, string text) {
        Item = item;
        Text = text;
      }

      public ListViewItem.ListViewSubItem Item { get; }
      public string Text { get; }
    }

    private class GroupToRefresh {
      public GroupToRefresh(ListViewGroup listViewGroup, string text) {
        ListViewGroup = listViewGroup;
        Text = text;
      }

      public ListViewGroup ListViewGroup { get; }
      public string Text { get; }
    }

    public void ShowError(string text) {
      MessageBox.Show(_mainForm, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }
  }
}
