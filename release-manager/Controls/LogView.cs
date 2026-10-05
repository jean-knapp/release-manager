using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReleaseManager.Release;
using ReleaseManager.Services;

namespace ReleaseManager.Controls
{
    /// <summary>One line in the log: its text, kind and the time it arrived.</summary>
    internal sealed class LogLine
    {
        public LogKind Kind;
        public string Text;
        public DateTime Time;
    }

    public sealed class LogLineEventArgs : EventArgs
    {
        public LogLineEventArgs(LogKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        public LogKind Kind { get; }
        public string Text { get; }
    }

    /// <summary>
    /// The build log: Consolas 12 px lines 20 px apart, padded 12 at the sides, with a 10 px
    /// marker column. Commands are accent and semibold behind a "›", results --ib-ok-icon and
    /// semibold, warnings and errors take their InfoBar colours on a tinted row, info is --fg and
    /// tool output --fg2. The newest line is at the bottom, and the view follows the tail unless
    /// the user scrolls up.
    /// </summary>
    public sealed class LogView : VirtualListControl, IReleaseLog
    {
        private const int LineHeight = 20;
        private const int PadX = 12;
        private const int MarkerWidth = 10;

        private readonly List<LogLine> _lines = new List<LogLine>();
        private bool _followTail = true;

        /// <summary>Raised on the UI thread for every line written.</summary>
        public event EventHandler<LogLineEventArgs> LineWritten;

        public LogView()
        {
            EmptyText = "The build log appears here.";
        }

        protected override int RowCount => _lines.Count;
        protected override int GetRowHeight(int index) => LineHeight;
        protected override bool IsSelectable(int index) => false;
        protected override bool SelectsOnRightClick => false;
        protected override Color RowSurface => Theme.Palette.ListBackground;

        public void Write(LogKind kind, string text)
        {
            if (InvokeRequired)
            {
                try { BeginInvoke((Action)(() => Write(kind, text))); } catch (InvalidOperationException) { }
                return;
            }
            foreach (var raw in (text ?? string.Empty).Replace("\r", string.Empty).Split('\n'))
            {
                _lines.Add(new LogLine { Kind = kind, Text = raw, Time = DateTime.Now });
                LineWritten?.Invoke(this, new LogLineEventArgs(kind, raw));
            }
            if (_lines.Count > 5000) _lines.RemoveRange(0, _lines.Count - 5000);
            ContentChanged();
            if (_followTail) ScrollOffset = int.MaxValue;
        }

        public void Clear()
        {
            _lines.Clear();
            _followTail = true;
            ContentChanged();
            Invalidate();
        }

        /// <summary>The whole log as text, for Copy.</summary>
        public string AllText() => string.Join(Environment.NewLine, _lines.Select(l => l.Text));

        public bool HasContent => _lines.Count > 0;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            // Scrolling up stops the view from jumping back to the tail; reaching the bottom resumes it.
            _followTail = ScrollOffset >= RowsHeight - ViewportHeight - LineHeight;
        }

        protected override void PaintRow(Graphics g, int index, Rectangle bounds, RowState state)
        {
            var line = _lines[index];
            var p = Theme.Palette;
            bool strong = line.Kind == LogKind.Command || line.Kind == LogKind.Success;
            var font = Fonts.Code(12f, strong);

            if (line.Kind == LogKind.Warning) Draw.Fill(g, bounds, p.WarnBack);
            else if (line.Kind == LogKind.Error) Draw.Fill(g, bounds, p.ErrBack);

            if (line.Kind == LogKind.Command)
            {
                Draw.Text(g, "›", font, new Rectangle(bounds.X + PadX, bounds.Y, MarkerWidth + 4, bounds.Height), p.Accent, Draw.LeftMiddle);
            }
            int textLeft = bounds.X + PadX + MarkerWidth + 8;
            var textRect = new Rectangle(textLeft, bounds.Y, Math.Max(0, bounds.Right - PadX - textLeft), bounds.Height);
            Draw.Text(g, line.Text, font, textRect, ColorFor(line.Kind, p), Draw.LeftMiddle);
        }

        private static Color ColorFor(LogKind kind, ThemePalette p)
        {
            switch (kind)
            {
                case LogKind.Command: return p.Accent;
                case LogKind.Warning: return p.WarnIcon;
                case LogKind.Error: return p.ErrIcon;
                case LogKind.Success: return p.OkIcon;
                case LogKind.Info: return p.Foreground;
                default: return p.Foreground2;
            }
        }
    }
}
