using System;
using System.Drawing;
using System.IO;
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
    /// Edits the README's description of the program - what it is and what it does - which is
    /// published to the README under the download button with each release. "Write with Claude"
    /// has the Claude Code CLI read the project's source and draft it.
    /// </summary>
    public sealed class ReadmeAboutDialog : ModernForm
    {
        private readonly ModernSkin _skin = new ModernSkin();
        private readonly SurfacePanel _content = new SurfacePanel();
        private readonly TextLabel _heading = new TextLabel();
        private readonly TextLabel _subhead = new TextLabel();
        private readonly ModernTextBox _editor = new ModernTextBox();
        private readonly ModernProgressRing _ring = new ModernProgressRing();
        private readonly TextLabel _status = new TextLabel();
        private readonly FooterBand _footer = new FooterBand();
        private readonly CommandButton _claudeButton = new CommandButton();
        private readonly CommandButton _saveButton = new CommandButton();
        private readonly CommandButton _cancelButton = new CommandButton();

        private readonly CsProject _project;
        private readonly string _initial;
        private CancellationTokenSource _writing;

        public ReadmeAboutDialog(CsProject project, string about)
        {
            _project = project;
            _initial = about ?? string.Empty;

            Theme.Apply(_skin);
            Skin = _skin;
            Text = "README description";
            AppIcon.Apply(this);
            Sizeable = true;
            TitleBar.ShowMaximizeBox = false;
            TitleBar.ShowMinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(640, 560);
            MinimumSize = new Size(480, 420);

            _content.Surface = SurfaceKind.Base;
            _content.CornerRadius = 0;
            _content.Dock = DockStyle.Fill;

            _heading.Text = "Describe " + project.AssemblyName;
            _heading.Semibold = true;
            _heading.SizePx = 20f;
            _subhead.Text = "What it is and what it does, in Markdown. Published to the README, under the download button, with the next release.";
            _subhead.Role = TextRole.Secondary;
            _subhead.SizePx = 12f;
            _subhead.MultiLine = true;

            _editor.Multiline = true;
            _editor.WordWrap = true;
            _editor.ScrollBars = ScrollBars.Vertical;
            _editor.PlaceholderText = "A short paragraph, then a list of features.";
            _editor.TextChanged += (s, e) => UpdateButtons();

            _ring.Size = new Size(16, 16);
            _ring.IsIndeterminate = true;
            _ring.Thickness = 2f;
            _ring.Visible = false;
            _status.Role = TextRole.Secondary;
            _status.SizePx = 12f;

            _claudeButton.Text = "Write with Claude";
            _claudeButton.IconSvg = Icons.Rename;
            _claudeButton.Click += async (s, e) => await WriteWithClaudeAsync();
            _saveButton.Text = "Save";
            _saveButton.Appearance = ButtonAppearance.Accent;
            _saveButton.Click += (s, e) => Save();
            _cancelButton.Text = "Cancel";
            _cancelButton.Appearance = ButtonAppearance.Standard;
            _cancelButton.Click += (s, e) => CancelOrStop();

            _content.Controls.AddRange(new Control[] { _heading, _subhead, _editor, _ring, _status });
            _footer.Controls.AddRange(new Control[] { _claudeButton, _saveButton, _cancelButton });
            Controls.Add(_content);
            Controls.Add(_footer);
            Theme.Changed += OnThemeChanged;
        }

        /// <summary>The description, once the dialog returns OK.</summary>
        public string About { get; private set; }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // The inner text box only exists once the form is up.
            _editor.Text = _initial.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            _editor.Font = Fonts.Ui(14f);
            UpdateButtons();
            LayoutDialog();
            _editor.Focus();
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (!IsDisposed) Theme.Apply(_skin);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Theme.Changed -= OnThemeChanged;
            _writing?.Cancel();
            base.OnFormClosed(e);
        }

        // ------------------------------------------------------------------ Claude

        private async Task WriteWithClaudeAsync()
        {
            if (_writing != null) return;
            var executable = ClaudeCode.FindExecutable();
            if (executable == null)
            {
                Dialogs.Error(this, "Write with Claude",
                    "The Claude Code CLI was not found on this PC.\n\nInstall it from https://claude.com/claude-code, sign in, then try again.");
                return;
            }
            if (_editor.Text.Trim().Length > 0 && !Dialogs.Confirm(this, "Write with Claude",
                "Replace the description with one Claude writes from the project's source?", "Replace")) return;

            _writing = new CancellationTokenSource();
            SetWorking(true, "Claude is reading the project's source. This can take a minute…");
            try
            {
                var text = await ClaudeCode.DescribeProgramAsync(executable, _project, WorkingDirectory(), _writing.Token);
                _editor.Text = text.Replace("\n", Environment.NewLine);
                SetWorking(false, "Written by Claude. Check it, then save.");
            }
            catch (OperationCanceledException)
            {
                SetWorking(false, "Stopped.");
            }
            catch (ClaudeSignInRequiredException ex)
            {
                SetWorking(false, string.Empty);
                ClaudeCode.OfferSignIn(this, executable, ex.Message);
            }
            catch (Exception ex)
            {
                SetWorking(false, "Claude could not write it: " + ex.Message);
            }
            finally
            {
                _writing?.Dispose();
                _writing = null;
                UpdateButtons();
            }
        }

        /// <summary>The repository's root, so Claude sees the whole program; the project's folder when it is not in one.</summary>
        private string WorkingDirectory()
        {
            for (var d = new DirectoryInfo(_project.Directory); d != null; d = d.Parent)
            {
                if (Directory.Exists(Path.Combine(d.FullName, ".git"))) return d.FullName;
            }
            return _project.Directory;
        }

        private void SetWorking(bool working, string status)
        {
            _ring.Visible = working;
            _status.Text = status;
            _editor.Enabled = !working;
            _cancelButton.Text = working ? "Stop" : "Cancel";
            UpdateButtons();
            LayoutDialog();
        }

        private void UpdateButtons()
        {
            bool working = _writing != null;
            _claudeButton.Enabled = !working;
            _saveButton.Enabled = !working;
            _claudeButton.Invalidate();
            _saveButton.Invalidate();
        }

        // ------------------------------------------------------------------ closing

        private void Save()
        {
            if (_writing != null) return;
            About = _editor.Text.Replace("\r\n", "\n").Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelOrStop()
        {
            if (_writing != null)
            {
                _writing.Cancel();
                return;
            }
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                CancelOrStop();
                return true;
            }
            if (keyData == (Keys.Control | Keys.Enter))
            {
                Save();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ------------------------------------------------------------------ layout

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_content != null) LayoutDialog();
        }

        private void LayoutDialog()
        {
            // Content padding 20 24 16, 16 px between blocks; the footer is padded 24.
            int width = _content.ClientSize.Width - 48;
            _heading.SetBounds(24, 20, width, 28);
            var subSize = TextRenderer.MeasureText(_subhead.Text, Fonts.Ui(12f), new Size(width, int.MaxValue), TextFormatFlags.WordBreak);
            _subhead.SetBounds(24, 52, width, subSize.Height + 2);
            int statusTop = _content.ClientSize.Height - 16 - 20;
            int editorTop = _subhead.Bottom + 16;
            _editor.SetBounds(24, editorTop, width, Math.Max(80, statusTop - 12 - editorTop));
            _ring.Location = new Point(24, statusTop + 2);
            int statusLeft = _ring.Visible ? 24 + 16 + 8 : 24;
            _status.SetBounds(statusLeft, statusTop, Math.Max(0, 24 + width - statusLeft), 20);

            _footer.Height = 80;
            _claudeButton.Appearance = ButtonAppearance.Subtle;
            _claudeButton.Width = _claudeButton.PreferredWidth;
            _claudeButton.SetBounds(24, 24, _claudeButton.Width, 32);
            _cancelButton.SetBounds(_footer.ClientSize.Width - 24 - 120, 24, 120, 32);
            _saveButton.SetBounds(_cancelButton.Left - 8 - 120, 24, 120, 32);
        }
    }
}
