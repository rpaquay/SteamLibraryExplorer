using SteamLibraryExplorer.UserInterface;

namespace SteamLibraryExplorer {
  partial class MainForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolbarLabel = new System.Windows.Forms.ToolStripLabel();
      this.mainViewPanel = new System.Windows.Forms.Panel();
      this.gameLibrariesListView = new SteamLibraryExplorer.UserInterface.FlickerFixListView();
      this.gameNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.acfFileHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.locationHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.sizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.fileCountHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.refreshListViewTimer = new System.Windows.Forms.Timer(this.components);
      this.statusStrip1.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.toolStrip1.SuspendLayout();
      this.mainViewPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
      this.statusStrip1.Location = new System.Drawing.Point(0, 430);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
      this.statusStrip1.Size = new System.Drawing.Size(813, 22);
      this.statusStrip1.TabIndex = 0;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // statusLabel
      // 
      this.statusLabel.Name = "statusLabel";
      this.statusLabel.Size = new System.Drawing.Size(0, 17);
      // 
      // menuStrip1
      // 
      this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem1});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
      this.menuStrip1.Size = new System.Drawing.Size(813, 24);
      this.menuStrip1.TabIndex = 1;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem,
            this.toolStripMenuItem2,
            this.helpToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // refreshToolStripMenuItem
      // 
      this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
      this.refreshToolStripMenuItem.ShortcutKeyDisplayString = "";
      this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
      this.refreshToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.refreshToolStripMenuItem.Text = "Refresh";
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(110, 6);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
      this.helpToolStripMenuItem.Text = "Exit";
      // 
      // helpToolStripMenuItem1
      // 
      this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
      this.helpToolStripMenuItem1.Size = new System.Drawing.Size(44, 22);
      this.helpToolStripMenuItem1.Text = "Help";
      // 
      // toolStrip1
      // 
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolbarLabel});
      this.toolStrip1.Location = new System.Drawing.Point(0, 24);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(813, 25);
      this.toolStrip1.TabIndex = 2;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // toolbarLabel
      // 
      this.toolbarLabel.Name = "toolbarLabel";
      this.toolbarLabel.Size = new System.Drawing.Size(0, 22);
      // 
      // mainViewPanel
      // 
      this.mainViewPanel.Controls.Add(this.gameLibrariesListView);
      this.mainViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mainViewPanel.Location = new System.Drawing.Point(0, 49);
      this.mainViewPanel.Margin = new System.Windows.Forms.Padding(2);
      this.mainViewPanel.Name = "mainViewPanel";
      this.mainViewPanel.Size = new System.Drawing.Size(813, 381);
      this.mainViewPanel.TabIndex = 3;
      // 
      // gameLibrariesListView
      // 
      this.gameLibrariesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.gameNameHeader,
            this.acfFileHeader,
            this.locationHeader,
            this.sizeColumnHeader,
            this.fileCountHeader});
      this.gameLibrariesListView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.gameLibrariesListView.ExplorerStyleTheme = false;
      this.gameLibrariesListView.FullRowSelect = true;
      this.gameLibrariesListView.GridLines = true;
      this.gameLibrariesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.gameLibrariesListView.Location = new System.Drawing.Point(0, 0);
      this.gameLibrariesListView.Margin = new System.Windows.Forms.Padding(1);
      this.gameLibrariesListView.Name = "gameLibrariesListView";
      this.gameLibrariesListView.Size = new System.Drawing.Size(813, 381);
      this.gameLibrariesListView.TabIndex = 6;
      this.gameLibrariesListView.UseCompatibleStateImageBehavior = false;
      this.gameLibrariesListView.View = System.Windows.Forms.View.Details;
      // 
      // gameNameHeader
      // 
      this.gameNameHeader.Text = "Game";
      this.gameNameHeader.Width = 200;
      // 
      // acfFileHeader
      // 
      this.acfFileHeader.Text = "ACF File";
      this.acfFileHeader.Width = 200;
      // 
      // locationHeader
      // 
      this.locationHeader.Text = "Location";
      this.locationHeader.Width = 200;
      // 
      // sizeColumnHeader
      // 
      this.sizeColumnHeader.Text = "Size";
      this.sizeColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.sizeColumnHeader.Width = 80;
      // 
      // fileCountHeader
      // 
      this.fileCountHeader.Text = "# of Files";
      this.fileCountHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.fileCountHeader.Width = 80;
      // 
      // refreshListViewTimer
      // 
      this.refreshListViewTimer.Interval = 300;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(813, 452);
      this.Controls.Add(this.mainViewPanel);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.menuStrip1);
      this.DoubleBuffered = true;
      this.MainMenuStrip = this.menuStrip1;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "MainForm";
      this.Text = "Steam Library Explorer";
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.mainViewPanel.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.Panel mainViewPanel;
    private System.Windows.Forms.ColumnHeader gameNameHeader;
    private System.Windows.Forms.ColumnHeader locationHeader;
    private System.Windows.Forms.ColumnHeader sizeColumnHeader;
    private System.Windows.Forms.ColumnHeader acfFileHeader;
    public FlickerFixListView gameLibrariesListView;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    public System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
    public System.Windows.Forms.Timer refreshListViewTimer;
    private System.Windows.Forms.ColumnHeader fileCountHeader;
    public System.Windows.Forms.ToolStripStatusLabel statusLabel;
    public System.Windows.Forms.ToolStripLabel toolbarLabel;
  }
}

