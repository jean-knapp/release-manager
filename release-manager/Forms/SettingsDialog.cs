using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModernWinForms;
using ReleaseManager.Controls;
using ReleaseManager.Release;
using ReleaseManager.Services;

namespace ReleaseManager.Forms
{
    /// <summary>
    /// Settings: the theme, the GitHub sign-in and saved token, the tools a release needs as found
    /// on this PC (with vpk's install or update), and the recent-projects list.
    /// </summary>
    public sealed class SettingsDialog : ModernForm
    {
        private readonly ModernSkin _skin = new ModernSkin();
        private readonly ModernScrollableControl _scroll = new ModernScrollableControl();
        private readonly FooterBand _footer = new FooterBand();
        private readonly CommandButton _closeButton = new CommandButton();

        private readonly TextLabel _heading = new TextLabel();
        private readonly TextLabel _appearanceHeading = new TextLabel();
        private readonly SettingsCard _themeCard = new SettingsCard();
        private readonly ModernSegmentedControl _themeSwitch = new ModernSegmentedControl();
        private readonly TextLabel _githubHeading = new TextLabel();
        private readonly SettingsCard _accountCard = new SettingsCard();
        private readonly CommandButton _tokenButton = new CommandButton();
        private readonly SettingsCard _savedTokenCard = new SettingsCard();
        private readonly CommandButton _removeTokenButton = new CommandButton();
        private readonly TextLabel _toolsHeading = new TextLabel();
        private readonly TextLabel _toolsCaption = new TextLabel();
        private readonly ToolList _tools = new ToolList();
        private readonly TextLabel _projectsHeading = new TextLabel();
        private readonly SettingsCard _recentCard = new SettingsCard();
        private readonly CommandButton _clearRecentButton = new CommandButton();

        private bool _vpkBusy;

        public SettingsDialog()
        {
            Theme.Apply(_skin);
            Skin = _skin;
            Text = "Settings";
            AppIcon.Apply(this);
            Sizeable = true;
            TitleBar.ShowMaximizeBox = false;
            TitleBar.ShowMinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(640, 720);
            MinimumSize = new Size(520, 480);

            _scroll.Dock = DockStyle.Fill;
            Heading(_heading, "Settings", 20f);
            Heading(_appearanceHeading, "Appearance", 14f);
            Heading(_githubHeading, "GitHub", 14f);
            Heading(_toolsHeading, "Tools", 14f);
            Heading(_projectsHeading, "Projects", 14f);
            _toolsCaption.Text = "Found on this PC";
            _toolsCaption.Role = TextRole.Secondary;
            _toolsCaption.SizePx = 12f;

            _themeCard.Header = "Theme";
            _themeCard.Description = "Changes apply to every open window.";
            _themeSwitch.Items.Add("Light");
            _themeSwitch.Items.Add("Dark");
            _themeSwitch.SegmentPadding = 14;
            _themeSwitch.Font = Fonts.Ui(14f);
            _themeSwitch.AutoSize = true;
            _themeSwitch.Height = 32;
            _themeSwitch.SelectedIndex = Theme.Mode == ThemeMode.Light ? 0 : 1;
            _themeSwitch.SelectedIndexChanged += ThemeSwitch_SelectedIndexChanged;
            _themeCard.Controls.Add(_themeSwitch);

            _accountCard.IconSvg = Icons.Github;
            _accountCard.Header = "Looking for a GitHub sign-in…";
            _tokenButton.Appearance = ButtonAppearance.Standard;
            _tokenButton.Text = "Use a different token…";
            _tokenButton.Click += async (s, e) => await ChangeTokenAsync();
            _accountCard.Controls.Add(_tokenButton);

            _savedTokenCard.Header = "Saved token";
            _removeTokenButton.Appearance = ButtonAppearance.Standard;
            _removeTokenButton.Text = "Remove";
            _removeTokenButton.Click += async (s, e) => await RemoveTokenAsync();
            _savedTokenCard.Controls.Add(_removeTokenButton);

            _tools.ActionClick += async (s, e) => await InstallVpkAsync();

            _recentCard.Header = "Recent projects";
            _clearRecentButton.Appearance = ButtonAppearance.Standard;
            _clearRecentButton.Text = "Clear list";
            _clearRecentButton.Click += (s, e) => ClearRecent();
            _recentCard.Controls.Add(_clearRecentButton);

            _scroll.Content.Controls.AddRange(new Control[]
            {
                _heading, _appearanceHeading, _themeCard, _githubHeading, _accountCard, _savedTokenCard,
                _toolsHeading, _toolsCaption, _tools, _projectsHeading, _recentCard,
            });

            _closeButton.Appearance = ButtonAppearance.Standard;
            _closeButton.Text = "Close";
            _closeButton.Click += (s, e) => Close();
            _footer.Controls.Add(_closeButton);

            Controls.Add(_scroll);
            Controls.Add(_footer);

            RefreshSavedToken();
            RefreshRecent();
            Theme.Changed += OnThemeChanged;
        }

        /// <summary>Raised after the recent-projects list is cleared, so the start page and jump list follow.</summary>
        public event EventHandler RecentCleared;

        private static void Heading(TextLabel label, string text, float size)
        {
            label.Text = text;
            label.Semibold = true;
            label.SizePx = size;
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LayoutDialog();
            var account = RefreshAccountAsync();
            await _tools.DetectAsync();
            await account;
            LayoutDialog();
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            Theme.Apply(_skin);
            Invalidate(true);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Theme.Changed -= OnThemeChanged;
            base.OnFormClosed(e);
        }

        // ------------------------------------------------------------------ appearance

        private void ThemeSwitch_SelectedIndexChanged(object sender, EventArgs e)
        {
            Theme.Mode = _themeSwitch.SelectedIndex == 0 ? ThemeMode.Light : ThemeMode.Dark;
            AppSettings.Current.Theme = Theme.Mode;
            AppSettings.Current.Save();
        }

        // ------------------------------------------------------------------ GitHub

        private async Task RefreshAccountAsync()
        {
            var account = await GitHubSignIn.FindAsync();
            if (IsDisposed) return;
            if (account.Login != null)
            {
                _accountCard.Header = "Signed in as " + account.Login;
                _accountCard.Description = GitHubSignIn.UsingText(account) + ". Used to list repositories and publish releases.";
                _tokenButton.Text = "Use a different token…";
            }
            else
            {
                _accountCard.Header = "Not signed in";
                _accountCard.Description = account.Problem + " Sign in with git, or add a personal access token, to list repositories and publish releases.";
                _tokenButton.Text = "Sign in…";
            }
            LayoutDialog();
        }

        private void RefreshSavedToken()
        {
            bool saved = !string.IsNullOrEmpty(AppSettings.Current.ProtectedGitHubToken);
            _savedTokenCard.Description = saved
                ? "Saved, encrypted for your Windows account. Publishing uses it before git's sign-in."
                : "None saved. A pasted token is stored encrypted for your Windows account.";
            _removeTokenButton.Enabled = saved;
            _removeTokenButton.Invalidate();
        }

        private async Task ChangeTokenAsync()
        {
            if (await GitHubSignIn.AddTokenAsync(this) == null) return;
            RefreshSavedToken();
            await RefreshAccountAsync();
        }

        private async Task RemoveTokenAsync()
        {
            if (!Dialogs.Confirm(this, "Saved token", "Remove the saved GitHub token? Release Manager then uses git's sign-in for github.com.", "Remove")) return;
            AppSettings.Current.GitHubToken = null;
            AppSettings.Current.Save();
            RefreshSavedToken();
            await RefreshAccountAsync();
        }

        // ------------------------------------------------------------------ tools

        private async Task InstallVpkAsync()
        {
            if (_vpkBusy) return;
            _vpkBusy = true;
            _tools.SetVpkWorking(true);
            try
            {
                var result = await Toolchain.InstallVpkAsync((line, error) => { }, CancellationToken.None);
                if (!result.Succeeded) Dialogs.Error(this, "vpk", "vpk could not be installed:\n\n" + result.Explanation);
            }
            catch (Exception ex)
            {
                Dialogs.Error(this, "vpk", ex.Message);
            }
            finally
            {
                _vpkBusy = false;
                _tools.SetVpkWorking(false);
                await _tools.DetectAsync();
                LayoutDialog();
            }
        }

        // ------------------------------------------------------------------ projects

        private void RefreshRecent()
        {
            int count = AppSettings.Current.RecentProjects.Count;
            _recentCard.Description = (count == 0 ? "No projects" : count == 1 ? "1 project" : count + " projects")
                + ". Shown on the start page and in the taskbar jump list.";
            _clearRecentButton.Enabled = count > 0;
            _clearRecentButton.Invalidate();
        }

        private void ClearRecent()
        {
            AppSettings.Current.RecentProjects.Clear();
            AppSettings.Current.Save();
            RefreshRecent();
            RecentCleared?.Invoke(this, EventArgs.Empty);
        }

        // ------------------------------------------------------------------ layout

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_scroll != null) LayoutDialog();
        }

        private void LayoutDialog()
        {
            _footer.Height = 80;
            _closeButton.SetBounds(_footer.ClientSize.Width - 24 - 120, 24, 120, 32);
            foreach (var button in new[] { _tokenButton, _removeTokenButton, _clearRecentButton }) button.Width = button.PreferredWidth;

            int height = LayoutAt(_scroll.Width - 48);
            if (height > _scroll.Height) height = LayoutAt(_scroll.Width - 48 - 12);
            _scroll.ContentWidth = _scroll.Width - (height > _scroll.Height ? 12 : 0);
            _scroll.ContentHeight = height;
        }

        /// <summary>Padding 20 24 24, 24 px between groups, 8 under each heading, cards 2 px apart.</summary>
        private int LayoutAt(int width)
        {
            int x = 24, y = 20;
            _heading.SetBounds(x, y, width, 28);
            y += 28 + 24;

            y = Group(_appearanceHeading, x, y, width);
            y = Card(_themeCard, x, y, width) + 24;

            y = Group(_githubHeading, x, y, width);
            y = Card(_accountCard, x, y, width);
            y = Card(_savedTokenCard, x, y + 2, width) + 24;

            _toolsHeading.Width = _toolsHeading.PreferredWidth + 2;
            _toolsHeading.SetBounds(x, y, _toolsHeading.Width, 20);
            _toolsCaption.SetBounds(_toolsHeading.Right + 6, y + 2, width, 18);
            y += 28;
            _tools.SetBounds(x, y, width, _tools.ContentHeight);
            y += _tools.Height + 24;

            y = Group(_projectsHeading, x, y, width);
            y = Card(_recentCard, x, y, width);
            return y + 24;
        }

        private static int Group(TextLabel heading, int x, int y, int width)
        {
            heading.SetBounds(x, y, width, 20);
            return y + 28;
        }

        private static int Card(SettingsCard card, int x, int y, int width)
        {
            card.SetBounds(x, y, width, card.MeasureHeight(width));
            return y + card.Height;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ------------------------------------------------------------------ the tools card

        /// <summary>
        /// One card with a row per tool: an 8 px status dot, the name over what was found (or what is
        /// missing and what that blocks), and vpk's Install or Update button.
        /// </summary>
        private sealed class ToolList : SurfacePanel
        {
            private const int RowHeight = 56;

            private sealed class Row
            {
                public string Name;
                public string Detail = "Looking…";
                public bool? Found;
            }

            private readonly Row _msbuild = new Row { Name = "MSBuild" };
            private readonly Row _dotnet = new Row { Name = ".NET SDK" };
            private readonly Row _git = new Row { Name = "Git" };
            private readonly Row _vpk = new Row { Name = "Velopack CLI (vpk)" };
            private readonly Row _obfuscator = new Row { Name = "Obfuscator" };
            private readonly CommandButton _vpkButton = new CommandButton();

            public event EventHandler ActionClick;

            public ToolList()
            {
                Surface = SurfaceKind.Card;
                CornerRadius = 6;
                _vpkButton.Appearance = ButtonAppearance.Standard;
                _vpkButton.Text = "Update";
                _vpkButton.Visible = false;
                _vpkButton.Click += (s, e) => ActionClick?.Invoke(this, EventArgs.Empty);
                Controls.Add(_vpkButton);
            }

            private IReadOnlyList<Row> Rows => new[] { _msbuild, _dotnet, _git, _vpk, _obfuscator };

            public int ContentHeight => Rows.Count * RowHeight;

            public async Task DetectAsync()
            {
                var msbuild = Toolchain.FindMSBuildAsync();
                var vsName = Toolchain.VisualStudioNameAsync();
                var dotnetPath = Toolchain.FindDotnet();
                var dotnet = Toolchain.VersionOfAsync(dotnetPath, "--version");
                var gitPath = Toolchain.FindGit();
                var git = Toolchain.VersionOfAsync(gitPath, "--version");
                var vpk = Toolchain.VpkVersionAsync();

                var msbuildPath = await msbuild;
                var name = await vsName;
                Set(_msbuild, msbuildPath != null, msbuildPath != null
                    ? (name ?? "Visual Studio") + " · found with vswhere"
                    : "Not found. Builds need Visual Studio or its Build Tools with the .NET desktop workload.");

                var dotnetVersion = await dotnet;
                Set(_dotnet, dotnetVersion != null, dotnetVersion != null
                    ? "dotnet " + dotnetVersion
                    : "Not found. vpk installs with the .NET SDK, so packaging and publishing need it.");

                var gitVersion = await git;
                Set(_git, gitVersion != null, gitVersion != null
                    ? "git " + gitVersion.Replace("git version", string.Empty).Trim() + " · reads the GitHub remote and saved credentials"
                    : "Not found. The repository can't be read from the project, and publishing needs a saved token.");

                var vpkVersion = await vpk;
                Set(_vpk, vpkVersion != null, vpkVersion != null
                    ? "vpk " + vpkVersion + " · packages and uploads releases"
                    : "Not installed. Packaging and publishing need it.");
                _vpkButton.Text = vpkVersion != null ? "Update" : "Install";
                _vpkButton.Width = Math.Max(80, _vpkButton.PreferredWidth);
                _vpkButton.Visible = true;

                Set(_obfuscator, Toolchain.HasConfuser, Toolchain.HasConfuser
                    ? "Included with Release Manager"
                    : "Missing from Release Manager's Tools folder. Obfuscation is unavailable.");
                PerformLayout();
                Invalidate();
            }

            public void SetVpkWorking(bool working)
            {
                _vpkButton.Enabled = !working;
                if (working) _vpk.Detail = "Installing or updating vpk…";
                Invalidate();
            }

            private void Set(Row row, bool found, string detail)
            {
                row.Found = found;
                row.Detail = detail;
                Invalidate();
            }

            protected override void OnLayout(LayoutEventArgs e)
            {
                base.OnLayout(e);
                int top = 3 * RowHeight;
                _vpkButton.SetBounds(Width - 16 - _vpkButton.Width, top + (RowHeight - 32) / 2, _vpkButton.Width, 32);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                PerformLayout();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                var p = Theme.Palette;
                var rows = Rows;
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    int top = i * RowHeight;
                    if (i > 0) Draw.HLine(g, 1, top, Width - 2, p.DividerOn(SurfaceColor));
                    if (row.Found.HasValue)
                    {
                        var saved = g.SmoothingMode;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var brush = new SolidBrush(row.Found.Value ? p.OkIcon : p.ErrIcon))
                            g.FillEllipse(brush, 16, top + RowHeight / 2 - 4, 8, 8);
                        g.SmoothingMode = saved;
                    }
                    int textLeft = 16 + 8 + 12;
                    int right = row == _vpk && _vpkButton.Visible ? _vpkButton.Left - 12 : Width - 16;
                    int width = Math.Max(0, right - textLeft);
                    int blockTop = top + (RowHeight - 38) / 2;
                    Draw.Text(g, row.Name, Fonts.Ui(14f), new Rectangle(textLeft, blockTop, width, 20), p.Foreground, Draw.LeftMiddle);
                    Draw.Text(g, row.Detail, Fonts.Ui(12f), new Rectangle(textLeft, blockTop + 22, width, 16), p.Foreground2, Draw.LeftMiddle);
                }
            }
        }
    }
}
