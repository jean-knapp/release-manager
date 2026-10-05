using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using ReleaseManager.Release;
using ReleaseManager.Services;

namespace ReleaseManager.Controls
{
    /// <summary>
    /// The repository picker's list: 52 px rows inset 4 px in a --tv-bg card, each with a folder
    /// glyph, the name over its description (or default branch), a "Private" tag and when it was
    /// last pushed to. Hover and selection are the neutral --tv-hover and --tv-sel tints with a
    /// 4 px radius; the selected row also carries a 3 x 14 accent pill at its left edge.
    /// </summary>
    [ToolboxItem(true)]
    public sealed class GitHubRepositoryListControl : VirtualListControl
    {
        private const int RowHeight = 52;
        private const int Inset = 4;
        private const int PadX = 12;
        private const int Gap = 12;

        private readonly List<GitHubRepository> _entries = new List<GitHubRepository>();

        public GitHubRepositoryListControl()
        {
            EmptyText = "No repositories.";
        }

        [Browsable(false)]
        public GitHubRepository SelectedEntry
        {
            get
            {
                int index = SelectedIndex;
                return index >= 0 && index < _entries.Count ? _entries[index] : null;
            }
        }

        /// <summary>How many rows are shown, after any filtering.</summary>
        [Browsable(false)]
        public int Count => _entries.Count;

        /// <summary>Shows these repositories, selecting <paramref name="select"/> when it is among them, else the first.</summary>
        public void SetEntries(IEnumerable<GitHubRepository> entries, GitHubRepository select = null)
        {
            _entries.Clear();
            if (entries != null) _entries.AddRange(entries);
            int index = select != null ? _entries.IndexOf(select) : -1;
            if (index < 0) index = _entries.Count > 0 ? 0 : -1;
            RestoreState(index, 0);
            ContentChanged();
            if (index > 0) EnsureVisible(index);
        }

        protected override int RowCount => _entries.Count;
        protected override int GetRowHeight(int index) => RowHeight;
        protected override int DefaultScrollStep => RowHeight;
        protected override Color RowSurface => P.ListBackground;

        protected override void PaintRow(Graphics g, int index, Rectangle bounds, RowState state)
        {
            var p = P;
            var surface = RowSurface;
            var entry = _entries[index];
            bool selected = (state & RowState.Selected) != 0;
            var row = new Rectangle(bounds.X + Inset, bounds.Y, Math.Max(0, bounds.Width - Inset * 2), bounds.Height);

            var face = surface;
            if (selected) face = ThemePalette.Flatten(p.ListSelection, surface);
            else if ((state & RowState.Hot) != 0) face = ThemePalette.Flatten(p.ListHover, surface);
            if (face != surface) Draw.FillRounded(g, row, 4f, face);
            if (selected) Draw.FillRounded(g, new Rectangle(row.X, row.Y + (row.Height - 14) / 2, 3, 14), 1.5f, p.Accent);

            int x = row.X + PadX;
            IconCache.DrawLeft(g, Icons.Folder, 16, p.Foreground2, new Rectangle(x, row.Y, 16, row.Height));
            x += 16 + Gap;

            // The right-hand column: 84 px of push time, right-aligned, and a Private tag before it.
            int right = row.Right - PadX;
            var when = entry.PushedUtc.HasValue ? Relative(entry.PushedUtc.Value) : string.Empty;
            Draw.Text(g, when, Fonts.Ui(12f), new Rectangle(right - 84, row.Y, 84, row.Height), p.Foreground3, Draw.RightMiddle);
            right -= 84 + Gap;
            if (entry.IsPrivate)
            {
                var tagFont = Fonts.Ui(11f);
                int width = Draw.MeasureWidth("Private", tagFont) + 16 + 2;
                var tag = new Rectangle(right - width, row.Y + (row.Height - 20) / 2, width, 20);
                Draw.DrawRounded(g, new Rectangle(tag.X, tag.Y, tag.Width - 1, tag.Height - 1), 4f, p.CardBorder);
                Draw.Text(g, "Private", tagFont, tag, p.Foreground2, Draw.CenterMiddle);
                right -= width + Gap;
            }

            int available = Math.Max(0, right - x);
            int blockTop = row.Y + (row.Height - 38) / 2;
            Draw.Text(g, entry.FullName, Fonts.Ui(14f), new Rectangle(x, blockTop, available, 20), p.Foreground, Draw.LeftMiddle);
            var second = string.IsNullOrWhiteSpace(entry.Description) ? entry.DefaultBranch : entry.Description.Trim();
            Draw.Text(g, second, Fonts.Ui(12f), new Rectangle(x, blockTop + 22, available, 16), p.Foreground2, Draw.LeftMiddle);
        }

        /// <summary>"3 days ago" for a UTC time.</summary>
        private static string Relative(DateTime utc)
        {
            var span = DateTime.UtcNow - utc;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return Plural((int)span.TotalMinutes, "minute") + " ago";
            if (span.TotalHours < 24) return Plural((int)span.TotalHours, "hour") + " ago";
            if (span.TotalDays < 2) return "yesterday";
            if (span.TotalDays < 7) return Plural((int)span.TotalDays, "day") + " ago";
            if (span.TotalDays < 35) return Plural((int)(span.TotalDays / 7), "week") + " ago";
            if (span.TotalDays < 365) return Plural((int)(span.TotalDays / 30), "month") + " ago";
            return Plural((int)(span.TotalDays / 365), "year") + " ago";
        }

        private static string Plural(int count, string unit) => count + " " + unit + (count == 1 ? string.Empty : "s");
    }
}
