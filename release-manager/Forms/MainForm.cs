using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ModernWinForms;
using ReleaseManager.Controls;
using ReleaseManager.Release;
using ReleaseManager.Services;
using ReleaseManager.Views;

namespace ReleaseManager.Forms
{
    /// <summary>
    /// The shell: a tab per open program, each a <see cref="ProjectView"/>, or the start page when
    /// none is open. "+" opens a project, the sliders button opens Settings, and the status bar
    /// follows the active project.
    /// </summary>
    public partial class MainForm : ModernForm
    {
        private readonly List<ProjectView> _views = new List<ProjectView>();
        private readonly Dictionary<string, Image> _projectIcons = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private ProjectView _activeView;
        private int _tabMenuIndex = -1;
        private bool _restoringTabs;

        public MainForm()
        {
            InitializeComponent();
            Theme.Mode = AppSettings.Current.Theme;
            Theme.Apply(skin);
            Theme.Changed += OnThemeChanged;

            settingsButton.IconSvg = Icons.Sliders;
            settingsButton.ToolTipText = "Settings";
            statusLabel.Text = string.Empty;
            AppIcon.Apply(this);
            ApplyTabStripTheme();

            welcomeView.OpenRequested += (s, e) => OpenProjectDialog();
            welcomeView.RecentActivated += (s, e) => OpenProject(e.Path, true);
        }

        // ------------------------------------------------------------------ theme and icons

        private void ApplyTabStripTheme()
        {
            var p = Theme.Palette;
            bool dark = p.Mode == ThemeMode.Dark;
            sessionTabs.Font = Fonts.Ui(14f);
            var c = sessionTabs.Colors;
            c.BackColor = p.Background;
            c.TabForeColor = p.Foreground;
            c.SelectedForeColor = p.Foreground;
            c.SubtitleForeColor = p.Foreground3;
            // --ts-sel and --ts-sel-border; hover is the neutral --sub-hover tint.
            c.SelectedBackColor = dark ? Color.FromArgb(45, 45, 45) : Color.FromArgb(230, 230, 230);
            c.SelectedBorderColor = dark ? Color.FromArgb(69, 69, 69) : Color.FromArgb(207, 207, 207);
            c.HoverBackColor = ThemePalette.Flatten(p.SubtleHover, p.Background);
            c.ButtonHoverBackColor = ThemePalette.Flatten(p.SubtleHover, p.Background);
            c.GlyphColor = p.Foreground3;
            c.AccentColor = p.Accent;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            Theme.Apply(skin);
            ApplyTabStripTheme();
            // The placeholder tiles are drawn in the theme's colours.
            foreach (var image in _projectIcons.Values) image?.Dispose();
            _projectIcons.Clear();
            RefreshTabs();
            Invalidate(true);
        }

        /// <summary>
        /// The project's ApplicationIcon at 16 px; without one, a 16 px tile carrying the name's
        /// first letter (--tile-bg, a --tile-border outline, radius 3, 9 px semibold --fg2).
        /// </summary>
        private Image ProjectIcon(ProjectView view)
        {
            var key = view.ProjectPath ?? view.TabTitle;
            if (_projectIcons.TryGetValue(key, out var cached)) return cached;
            Image image = null;
            try
            {
                if (view.IconPath != null && File.Exists(view.IconPath))
                {
                    using (var icon = new Icon(view.IconPath, 16, 16)) image = new Bitmap(icon.ToBitmap(), 16, 16);
                }
            }
            catch
            {
                image = null;
            }
            if (image == null) image = PlaceholderTile(view.TabTitle);
            _projectIcons[key] = image;
            return image;
        }

        private static Image PlaceholderTile(string name)
        {
            var p = Theme.Palette;
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var tile = new Rectangle(0, 0, 16, 16);
                Draw.FillRounded(g, tile, 3f, ThemePalette.Flatten(p.TileFill, p.Background));
                Draw.DrawRounded(g, new Rectangle(0, 0, 15, 15), 3f, ThemePalette.Flatten(p.TileBorder, p.Background));
                var letter = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
                Draw.Text(g, letter, Fonts.Ui(9f, true), tile, p.Foreground2, Draw.CenterMiddle);
            }
            return bitmap;
        }

        // ------------------------------------------------------------------ lifetime

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RestoreWindowPosition();

            var settings = AppSettings.Current;
            _restoringTabs = true;
            try
            {
                foreach (var path in settings.OpenProjects.ToList())
                {
                    if (File.Exists(path) && _views.All(v => !PathEquals(v.ProjectPath, path))) AddProject(path);
                }
            }
            finally
            {
                _restoringTabs = false;
            }

            RefreshWelcome();
            int active = settings.ActiveTab;
            Activate(_views.Count == 0 ? null : active >= 0 && active < _views.Count ? _views[active] : _views[0]);
            JumpListUpdater.Update(AppSettings.Current.RecentProjects);
        }

        private void RestoreWindowPosition()
        {
            var settings = AppSettings.Current;
            if (settings.WindowWidth > 400 && settings.WindowHeight > 300) Size = new Size(settings.WindowWidth, settings.WindowHeight);
            if (settings.WindowX >= 0 && settings.WindowY >= 0)
            {
                var bounds = new Rectangle(settings.WindowX, settings.WindowY, Width, Height);
                if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(bounds)))
                {
                    StartPosition = FormStartPosition.Manual;
                    Location = new Point(settings.WindowX, settings.WindowY);
                }
            }
            if (settings.WindowMaximized) WindowState = FormWindowState.Maximized;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var busy = _views.Where(v => v.IsBusy).ToList();
                if (busy.Count > 0 && !Dialogs.Confirm(this, "Quit",
                    "A release is still running for " + string.Join(", ", busy.Select(v => v.TabTitle)) + ".\n\nQuitting stops it. Quit anyway?", "Quit"))
                {
                    e.Cancel = true;
                    return;
                }
                foreach (var view in busy) view.CancelRunning();
            }
            var settings = AppSettings.Current;
            settings.WindowMaximized = WindowState == FormWindowState.Maximized;
            if (WindowState == FormWindowState.Normal)
            {
                settings.WindowX = Location.X;
                settings.WindowY = Location.Y;
                settings.WindowWidth = Width;
                settings.WindowHeight = Height;
            }
            StoreOpenTabs(settings);
            settings.Theme = Theme.Mode;
            settings.Save();
            base.OnFormClosing(e);
        }

        private void StoreOpenTabs(AppSettings settings)
        {
            settings.OpenProjects = _views.Select(v => v.ProjectPath).ToList();
            settings.ActiveTab = _activeView == null ? -1 : _views.IndexOf(_activeView);
        }

        private void SaveOpenTabs()
        {
            if (_restoringTabs || IsDisposed) return;
            StoreOpenTabs(AppSettings.Current);
            AppSettings.Current.Save();
        }

        // ------------------------------------------------------------------ opening projects

        private void OpenProjectDialog()
        {
            using (var dialog = new OpenFileDialog { Filter = "C# project (*.csproj)|*.csproj", Title = "Open a C# project" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) OpenProject(dialog.FileName, true);
            }
        }

        /// <summary>Opens a project, or switches to its tab when it is open already. Used by the jump list too.</summary>
        public void OpenProject(string path, bool activate)
        {
            path = Path.GetFullPath(path);
            var existing = _views.FirstOrDefault(v => PathEquals(v.ProjectPath, path));
            if (existing != null)
            {
                Activate(existing);
                return;
            }
            if (!File.Exists(path))
            {
                Dialogs.Warning(this, "Open project", "That project file no longer exists:\n\n" + path);
                AppSettings.Current.RecentProjects.RemoveAll(p => PathEquals(p, path));
                AppSettings.Current.Save();
                RefreshWelcome();
                JumpListUpdater.Update(AppSettings.Current.RecentProjects);
                return;
            }

            ProjectView view;
            try
            {
                view = AddProject(path);
            }
            catch (Exception ex)
            {
                Dialogs.Error(this, "Open project", "Could not read the project:\n\n" + ex.Message);
                return;
            }
            AppSettings.Current.AddRecent(path);
            AppSettings.Current.Save();
            JumpListUpdater.Update(AppSettings.Current.RecentProjects);
            if (activate) Activate(view);
            else RefreshTabs();
        }

        private ProjectView AddProject(string path)
        {
            var project = CsProject.Load(path);
            var settings = AppSettings.Current.ProjectFor(path);
            var view = new ProjectView { Dock = DockStyle.Fill, Visible = false };
            hostPanel.Controls.Add(view);
            view.Attach(project, settings);
            view.TitleChanged += View_TitleChanged;
            view.StateChanged += View_StateChanged;
            _views.Add(view);
            RefreshTabs();
            SaveOpenTabs();
            return view;
        }

        private void View_TitleChanged(object sender, EventArgs e)
        {
            RefreshTabs();
            UpdateTitle();
            UpdateStatus();
        }

        private void View_StateChanged(object sender, EventArgs e)
        {
            RefreshTabs();
            UpdateStatus();
        }

        // ------------------------------------------------------------------ tabs

        private void RefreshTabs()
        {
            var tabs = _views.Select(v => new ModernTabStripItem
            {
                Text = v.TabTitle,
                Subtitle = "release",
                Icon = ProjectIcon(v),
                Tag = v,
            }).ToList();
            int selected = _activeView != null ? _views.IndexOf(_activeView) : -1;
            sessionTabs.SetItems(tabs, selected);
        }

        private void Activate(ProjectView view)
        {
            _activeView = view;
            foreach (var v in _views) v.Visible = ReferenceEquals(v, view);
            welcomeView.Visible = view == null;
            if (view != null)
            {
                view.BringToFront();
            }
            else
            {
                welcomeView.BringToFront();
                RefreshWelcome();
            }
            RefreshTabs();
            UpdateTitle();
            UpdateStatus();
            SaveOpenTabs();
        }

        /// <summary>"Release Manager", with the active project as the title bar's second part.</summary>
        private void UpdateTitle()
        {
            Text = "Release Manager";
            TitleBar.SecondaryTitle = _activeView?.TabTitle ?? string.Empty;
        }

        private void UpdateStatus()
        {
            statusLabel.Text = _activeView?.StatusText ?? string.Empty;
        }

        private void RefreshWelcome()
        {
            welcomeView.SetRecent(AppSettings.Current.RecentProjects.Where(File.Exists));
        }

        private void sessionTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = sessionTabs.SelectedIndex;
            if (index >= 0 && index < _views.Count) Activate(_views[index]);
        }

        private void sessionTabs_TabCloseRequested(object sender, ModernTabStripEventArgs e) => CloseTab(e.Index);

        private void sessionTabs_TabMoved(object sender, ModernTabMovedEventArgs e)
        {
            if (e.FromIndex >= 0 && e.FromIndex < _views.Count && e.ToIndex >= 0 && e.ToIndex < _views.Count)
            {
                var view = _views[e.FromIndex];
                _views.RemoveAt(e.FromIndex);
                _views.Insert(e.ToIndex, view);
                SaveOpenTabs();
            }
            RefreshTabs();
        }

        private void CloseTab(int index)
        {
            if (index < 0 || index >= _views.Count) return;
            var view = _views[index];
            if (view.IsBusy && !Dialogs.Confirm(this, "Close " + view.TabTitle,
                "A release is still running. Closing the tab stops it. Close anyway?", "Close tab")) return;
            view.CancelRunning();

            _views.RemoveAt(index);
            view.TitleChanged -= View_TitleChanged;
            view.StateChanged -= View_StateChanged;
            if (_activeView == view) _activeView = null;
            hostPanel.Controls.Remove(view);
            view.Dispose();

            Activate(_views.Count == 0 ? null : _views[Math.Min(index, _views.Count - 1)]);
        }

        private void sessionTabs_TabContextMenuRequested(object sender, ModernTabStripEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _views.Count) return;
            _tabMenuIndex = e.Index;
            tabCloseOthersItem.Enabled = _views.Count > 1;
            tabMenu.Show(sessionTabs, Cursor.Position);
        }

        /// <summary>"+" goes straight to the file browser.</summary>
        private void sessionTabs_AddRequested(object sender, EventArgs e) => OpenProjectDialog();

        private void tabCloseItem_Click(object sender, EventArgs e) => CloseTab(_tabMenuIndex);

        private void tabCloseOthersItem_Click(object sender, EventArgs e)
        {
            if (_tabMenuIndex < 0 || _tabMenuIndex >= _views.Count) return;
            var keep = _views[_tabMenuIndex];
            for (int i = _views.Count - 1; i >= 0; i--)
            {
                if (!ReferenceEquals(_views[i], keep)) CloseTab(i);
            }
        }

        private void tabRevealItem_Click(object sender, EventArgs e)
        {
            if (_tabMenuIndex < 0 || _tabMenuIndex >= _views.Count) return;
            var path = _views[_tabMenuIndex].ProjectPath;
            if (File.Exists(path)) Reveal("/select,\"" + path + "\"");
        }

        private void tabOpenFolderItem_Click(object sender, EventArgs e)
        {
            if (_tabMenuIndex < 0 || _tabMenuIndex >= _views.Count) return;
            try
            {
                var project = CsProject.Load(_views[_tabMenuIndex].ProjectPath);
                var settings = AppSettings.Current.ProjectFor(project.Path);
                var folders = new ReleaseFolders(project, settings.OutputFolder);
                Directory.CreateDirectory(folders.Root);
                Reveal("\"" + folders.Root + "\"");
            }
            catch (Exception ex)
            {
                Dialogs.Error(this, "Open folder", ex.Message);
            }
        }

        private static void Reveal(string arguments)
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", arguments) { UseShellExecute = true }); }
            catch { }
        }

        // ------------------------------------------------------------------ settings

        private void settingsButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new SettingsDialog())
            {
                dialog.RecentCleared += (s, a) =>
                {
                    RefreshWelcome();
                    JumpListUpdater.Update(AppSettings.Current.RecentProjects);
                };
                dialog.ShowDialog(this);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.O:
                    OpenProjectDialog();
                    return true;
                case Keys.Control | Keys.W:
                    if (_activeView != null) CloseTab(_views.IndexOf(_activeView));
                    return true;
                case Keys.Control | Keys.Oemcomma:
                    settingsButton_Click(this, EventArgs.Empty);
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>A project path from a second launch (the jump list, say), opened here instead.</summary>
        public void OpenFromAnotherInstance(string path)
        {
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
            if (!string.IsNullOrWhiteSpace(path)) OpenProject(path, true);
        }

        // ------------------------------------------------------------------ helpers

        private static bool PathEquals(string a, string b) =>
            a != null && b != null && string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
    }
}
