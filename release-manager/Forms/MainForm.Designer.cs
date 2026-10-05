namespace ReleaseManager.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.skin = new ModernWinForms.ModernSkin();
            this.tabMenu = new ModernWinForms.ModernContextMenu(this.components);
            this.tabRevealItem = new ModernWinForms.ModernContextMenuItem();
            this.tabOpenFolderItem = new ModernWinForms.ModernContextMenuItem();
            this.tabCloseItem = new ModernWinForms.ModernContextMenuItem();
            this.tabCloseOthersItem = new ModernWinForms.ModernContextMenuItem();
            this.tabRow = new ReleaseManager.Controls.SurfacePanel();
            this.sessionTabs = new ModernWinForms.ModernTabStrip();
            this.settingsButton = new ReleaseManager.Controls.CommandButton();
            this.hostPanel = new ReleaseManager.Controls.SurfacePanel();
            this.welcomeView = new ReleaseManager.Views.WelcomeView();
            this.statusBar = new ReleaseManager.Controls.SurfacePanel();
            this.statusLabel = new ReleaseManager.Controls.TextLabel();
            this.tabRow.SuspendLayout();
            this.hostPanel.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            //
            // tabMenu
            //
            this.tabMenu.Items.Add(this.tabRevealItem);
            this.tabMenu.Items.Add(this.tabOpenFolderItem);
            this.tabMenu.Items.Add(this.tabCloseItem);
            this.tabMenu.Items.Add(this.tabCloseOthersItem);
            //
            // tabRevealItem
            //
            this.tabRevealItem.Text = "Reveal the project in Explorer";
            this.tabRevealItem.Click += new System.EventHandler(this.tabRevealItem_Click);
            //
            // tabOpenFolderItem
            //
            this.tabOpenFolderItem.Text = "Open the releases folder";
            this.tabOpenFolderItem.Click += new System.EventHandler(this.tabOpenFolderItem_Click);
            //
            // tabCloseItem
            //
            this.tabCloseItem.BeginGroup = true;
            this.tabCloseItem.Shortcut = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W;
            this.tabCloseItem.Text = "Close";
            this.tabCloseItem.Click += new System.EventHandler(this.tabCloseItem_Click);
            //
            // tabCloseOthersItem
            //
            this.tabCloseOthersItem.Text = "Close others";
            this.tabCloseOthersItem.Click += new System.EventHandler(this.tabCloseOthersItem_Click);
            //
            // tabRow: 4 px above the tabs, 12 px right of the Settings button.
            //
            this.tabRow.Controls.Add(this.sessionTabs);
            this.tabRow.Controls.Add(this.settingsButton);
            this.tabRow.CornerRadius = 0;
            this.tabRow.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabRow.Name = "tabRow";
            this.tabRow.Padding = new System.Windows.Forms.Padding(0, 4, 12, 0);
            this.tabRow.Size = new System.Drawing.Size(1440, 42);
            this.tabRow.Surface = ReleaseManager.Controls.SurfaceKind.Base;
            this.tabRow.TabIndex = 0;
            //
            // sessionTabs
            //
            this.sessionTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sessionTabs.Name = "sessionTabs";
            this.sessionTabs.TabIndex = 0;
            this.sessionTabs.SelectedIndexChanged += new System.EventHandler(this.sessionTabs_SelectedIndexChanged);
            this.sessionTabs.TabCloseRequested += new System.EventHandler<ModernWinForms.ModernTabStripEventArgs>(this.sessionTabs_TabCloseRequested);
            this.sessionTabs.TabContextMenuRequested += new System.EventHandler<ModernWinForms.ModernTabStripEventArgs>(this.sessionTabs_TabContextMenuRequested);
            this.sessionTabs.AddRequested += new System.EventHandler(this.sessionTabs_AddRequested);
            this.sessionTabs.TabMoved += new System.EventHandler<ModernWinForms.ModernTabMovedEventArgs>(this.sessionTabs_TabMoved);
            //
            // settingsButton
            //
            this.settingsButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.settingsButton.IconSize = 18;
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.PaddingX = 9;
            this.settingsButton.Size = new System.Drawing.Size(36, 32);
            this.settingsButton.TabIndex = 1;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            //
            // hostPanel
            //
            this.hostPanel.Controls.Add(this.welcomeView);
            this.hostPanel.CornerRadius = 0;
            this.hostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hostPanel.Name = "hostPanel";
            this.hostPanel.Surface = ReleaseManager.Controls.SurfaceKind.Base;
            this.hostPanel.TabIndex = 1;
            //
            // welcomeView
            //
            this.welcomeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.welcomeView.Name = "welcomeView";
            this.welcomeView.TabIndex = 0;
            //
            // statusBar: the 28 px status line at the bottom of the window.
            //
            this.statusBar.Controls.Add(this.statusLabel);
            this.statusBar.CornerRadius = 0;
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.Name = "statusBar";
            this.statusBar.Padding = new System.Windows.Forms.Padding(12, 0, 24, 0);
            this.statusBar.Size = new System.Drawing.Size(1440, 28);
            this.statusBar.Surface = ReleaseManager.Controls.SurfaceKind.Base;
            this.statusBar.TabIndex = 2;
            //
            // statusLabel
            //
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.SizePx = 12F;
            //
            // MainForm
            //
            this.ClientSize = new System.Drawing.Size(1440, 868);
            this.Controls.Add(this.hostPanel);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.tabRow);
            this.MinimumSize = new System.Drawing.Size(1000, 640);
            this.Name = "MainForm";
            this.Skin = this.skin;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Release Manager";
            this.TitleBar.ShowIcon = true;
            this.tabRow.ResumeLayout(false);
            this.hostPanel.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private ModernWinForms.ModernSkin skin;
        private ModernWinForms.ModernContextMenu tabMenu;
        private ModernWinForms.ModernContextMenuItem tabRevealItem;
        private ModernWinForms.ModernContextMenuItem tabOpenFolderItem;
        private ModernWinForms.ModernContextMenuItem tabCloseItem;
        private ModernWinForms.ModernContextMenuItem tabCloseOthersItem;
        private ReleaseManager.Controls.SurfacePanel tabRow;
        private ModernWinForms.ModernTabStrip sessionTabs;
        private ReleaseManager.Controls.CommandButton settingsButton;
        private ReleaseManager.Controls.SurfacePanel hostPanel;
        private ReleaseManager.Views.WelcomeView welcomeView;
        private ReleaseManager.Controls.SurfacePanel statusBar;
        private ReleaseManager.Controls.TextLabel statusLabel;
    }
}
