using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ModernWinForms;
using ReleaseManager.Controls;
using ReleaseManager.Services;

namespace ReleaseManager.Views
{
    public sealed class RecentEventArgs : EventArgs
    {
        public RecentEventArgs(string path) { Path = path; }
        public string Path { get; }
    }

    /// <summary>The no-project screen: a heading, an "Open a project" button and recent projects.</summary>
    public sealed class WelcomeView : ModernUserControl
    {
        private readonly TextLabel _heading = new TextLabel();
        private readonly TextLabel _subhead = new TextLabel();
        private readonly CommandButton _openButton = new CommandButton();
        private readonly TextLabel _recentLabel = new TextLabel();
        private readonly List<CommandButton> _recentButtons = new List<CommandButton>();
        private readonly SurfacePanel _card = new SurfacePanel();

        public event EventHandler OpenRequested;
        public event EventHandler<RecentEventArgs> RecentActivated;

        public WelcomeView()
        {
            _heading.Text = "Release a program";
            _heading.Semibold = true;
            _heading.SizePx = 30f;

            _subhead.Text = "Build, obfuscate, package and publish a C# program to GitHub Releases — with updates built in.";
            _subhead.Role = TextRole.Secondary;
            _subhead.SizePx = 15f;
            _subhead.MultiLine = true;

            _openButton.Text = "Open a C# project…";
            _openButton.Appearance = ButtonAppearance.Accent;
            _openButton.CornerRadius = 6;
            _openButton.PaddingX = 22;
            _openButton.IconSvg = Icons.Plus;
            _openButton.TextSizePx = 14f;
            _openButton.Click += (s, e) => OpenRequested?.Invoke(this, EventArgs.Empty);

            _recentLabel.Text = "Recent";
            _recentLabel.Role = TextRole.Tertiary;
            _recentLabel.Semibold = true;

            _card.Surface = SurfaceKind.Card;
            _card.CornerRadius = 12;

            Controls.Add(_card);
            _card.Controls.Add(_heading);
            _card.Controls.Add(_subhead);
            _card.Controls.Add(_openButton);
            _card.Controls.Add(_recentLabel);

            Theme.Changed += (s, e) => { if (!IsDisposed) Invalidate(true); };
        }

        public void SetRecent(IEnumerable<string> paths)
        {
            foreach (var button in _recentButtons) { _card.Controls.Remove(button); button.Dispose(); }
            _recentButtons.Clear();

            foreach (var path in paths.Take(8))
            {
                var captured = path;
                var button = new CommandButton
                {
                    Appearance = ButtonAppearance.Subtle,
                    CenterContent = false,
                    CornerRadius = 6,
                    PaddingX = 12,
                    IconSvg = Icons.File,
                    Text = Path.GetFileNameWithoutExtension(path) + "   " + Shorten(path),
                    TextSizePx = 13f,
                };
                button.Enabled = File.Exists(path);
                button.Click += (s, e) => RecentActivated?.Invoke(this, new RecentEventArgs(captured));
                _recentButtons.Add(button);
                _card.Controls.Add(button);
            }
            _recentLabel.Visible = _recentButtons.Count > 0;
            LayoutContent();
        }

        private static string Shorten(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path) ?? path;
                var parts = dir.Split(Path.DirectorySeparatorChar);
                return parts.Length <= 3 ? dir : "…" + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar.ToString(), parts.Skip(parts.Length - 2));
            }
            catch { return path; }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutContent();
        }

        private void LayoutContent()
        {
            int cardWidth = Math.Min(680, Math.Max(360, Width - 120));
            int cardLeft = (Width - cardWidth) / 2;

            int y = 40;
            _heading.SetBounds(40, y, cardWidth - 80, 42); y += 50;
            var subSize = TextRenderer.MeasureText(_subhead.Text, Fonts.Ui(15f), new Size(cardWidth - 80, int.MaxValue), TextFormatFlags.WordBreak);
            _subhead.SetBounds(40, y, cardWidth - 80, Math.Max(24, subSize.Height + 4)); y += _subhead.Height + 22;
            _openButton.Width = Math.Max(200, _openButton.PreferredWidth);
            _openButton.SetBounds(40, y, _openButton.Width, 40); y += 56;

            if (_recentButtons.Count > 0)
            {
                _recentLabel.SetBounds(40, y, cardWidth - 80, 18); y += 26;
                foreach (var button in _recentButtons)
                {
                    button.SetBounds(32, y, cardWidth - 64, 34);
                    y += 38;
                }
                y += 8;
            }

            int cardHeight = y + 16;
            int cardTop = Math.Max(24, (Height - cardHeight) / 2);
            _card.SetBounds(cardLeft, cardTop, cardWidth, cardHeight);
        }
    }
}
