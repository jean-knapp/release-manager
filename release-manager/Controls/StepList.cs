using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ModernWinForms;
using ReleaseManager.Services;

namespace ReleaseManager.Controls
{
    public enum StepStatus
    {
        /// <summary>Not run yet.</summary>
        Idle,
        Running,
        Done,
        Failed,
        /// <summary>Turned off: the release goes ahead without it.</summary>
        Skipped,
    }

    /// <summary>One row of the <see cref="StepList"/>.</summary>
    public sealed class StepItem
    {
        internal StepItem(StepList owner, int number, string title)
        {
            Owner = owner;
            Number = number;
            Title = title;
        }

        internal StepList Owner { get; }
        internal CommandButton Button { get; set; }

        public int Number { get; }
        public string Title { get; }
        public StepStatus State { get; set; }

        /// <summary>The single line under the title.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>When it last finished, "22:16:41"; empty for none.</summary>
        public string Time { get; set; } = string.Empty;

        public string ActionText { get; set; } = "Run";
        public bool ActionEnabled { get; set; } = true;

        /// <summary>The accent style, kept for Retry on a failed step.</summary>
        public bool ActionIsAccent { get; set; }

        /// <summary>Raised when the row's button is clicked.</summary>
        public event EventHandler Action;

        internal void RaiseAction() => Action?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// The release steps as one card: a row per step with a 32 px status slot (a check for done,
    /// a progress ring while running, "!" for failed, "–" for skipped), the step number and
    /// title over a single status line, the time it finished and its button. Rows are at least
    /// 64 px tall, padded 16 at the sides, with a --sep divider between them.
    /// </summary>
    [ToolboxItem(false)]
    public sealed class StepList : SurfacePanel
    {
        private const int RowHeight = 64;
        private const int PadX = 16;
        private const int Gap = 16;
        private const int Slot = 32;
        private const int ButtonMinWidth = 96;

        private readonly List<StepItem> _items = new List<StepItem>();
        private readonly ModernProgressRing _ring = new ModernProgressRing();

        public StepList()
        {
            Surface = SurfaceKind.Card;
            CornerRadius = 6;
            _ring.Size = new Size(24, 24);
            _ring.IsIndeterminate = true;
            _ring.Thickness = 2.5f;
            _ring.Visible = false;
            Controls.Add(_ring);
        }

        public IReadOnlyList<StepItem> Items => _items;

        /// <summary>The height every row together needs.</summary>
        public int ContentHeight => _items.Count * RowHeight;

        public StepItem Add(string title)
        {
            var item = new StepItem(this, _items.Count + 1, title);
            var button = new CommandButton
            {
                Appearance = ButtonAppearance.Standard,
                Size = new Size(ButtonMinWidth, 32),
            };
            button.Click += (s, e) => item.RaiseAction();
            item.Button = button;
            Controls.Add(button);
            _items.Add(item);
            Apply(item);
            return item;
        }

        /// <summary>Applies a row's changed properties to its button and repaints it.</summary>
        public void Apply(StepItem item)
        {
            var button = item.Button;
            button.Text = item.ActionText;
            button.Appearance = item.ActionIsAccent ? ButtonAppearance.Accent : ButtonAppearance.Standard;
            button.Enabled = item.ActionEnabled;
            button.Width = Math.Max(ButtonMinWidth, button.PreferredWidth);
            button.Invalidate();
            PerformLayout();
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            StepItem running = null;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                int top = i * RowHeight;
                item.Button.Location = new Point(Width - PadX - item.Button.Width, top + (RowHeight - item.Button.Height) / 2);
                if (item.State == StepStatus.Running) running = item;
            }
            if (running != null)
            {
                int top = (running.Number - 1) * RowHeight;
                _ring.Location = new Point(PadX + (Slot - _ring.Width) / 2, top + (RowHeight - _ring.Height) / 2);
            }
            _ring.Visible = running != null;
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
            var surface = SurfaceColor;
            var divider = p.DividerOn(surface);

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                int top = i * RowHeight;
                if (i > 0) Draw.HLine(g, 1, top, Width - 2, divider);
                PaintSlot(g, p, surface, item, new Rectangle(PadX, top + (RowHeight - Slot) / 2, Slot, Slot));

                int textLeft = PadX + Slot + Gap;
                int right = item.Button.Left - Gap;
                var timeFont = Fonts.Ui(12f);
                if (!string.IsNullOrEmpty(item.Time))
                {
                    int width = Draw.MeasureWidth(item.Time, timeFont);
                    Draw.Text(g, item.Time, timeFont, new Rectangle(right - width, top, width + 2, RowHeight), p.Foreground3, Draw.LeftMiddle);
                    right -= width + Gap;
                }
                int textWidth = Math.Max(0, right - textLeft);

                // "01  Title" over the status line, centred as a block on the row.
                int blockTop = top + (RowHeight - (20 + 2 + 16)) / 2;
                var number = item.Number.ToString("00");
                var numberFont = Fonts.Code(11f);
                int numberWidth = Draw.MeasureWidth(number, numberFont);
                Draw.Text(g, number, numberFont, new Rectangle(textLeft, blockTop + 1, numberWidth + 2, 20), p.Foreground3, Draw.LeftMiddle);
                int titleLeft = textLeft + numberWidth + 8;
                Draw.Text(g, item.Title, Fonts.Ui(14f, true), new Rectangle(titleLeft, blockTop, Math.Max(0, textLeft + textWidth - titleLeft), 20), p.Foreground, Draw.LeftMiddle);
                Draw.Text(g, item.Status, Fonts.Ui(12f), new Rectangle(textLeft, blockTop + 22, textWidth, 16), p.Foreground2, Draw.LeftMiddle);
            }
        }

        private static void PaintSlot(Graphics g, ThemePalette p, Color surface, StepItem item, Rectangle slot)
        {
            if (item.State == StepStatus.Running) return; // the progress ring sits here
            var circle = new Rectangle(slot.X + 2, slot.Y + 2, 28, 28);
            var saved = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            switch (item.State)
            {
                case StepStatus.Done:
                    using (var brush = new SolidBrush(p.OkBack)) g.FillEllipse(brush, circle);
                    g.SmoothingMode = saved;
                    IconCache.DrawCentered(g, Icons.Check, 16, p.OkIcon, circle.X + 14, circle.Y + 14);
                    return;
                case StepStatus.Failed:
                    using (var brush = new SolidBrush(p.ErrBack)) g.FillEllipse(brush, circle);
                    g.SmoothingMode = saved;
                    Draw.Text(g, "!", Fonts.Ui(14f, true), circle, p.ErrIcon, Draw.CenterMiddle);
                    return;
                case StepStatus.Skipped:
                    using (var pen = new Pen(p.CardBorder)) g.DrawEllipse(pen, circle.X + 0.5f, circle.Y + 0.5f, circle.Width - 1f, circle.Height - 1f);
                    g.SmoothingMode = saved;
                    Draw.Text(g, "–", Fonts.Ui(14f), circle, p.Foreground3, Draw.CenterMiddle);
                    return;
                default:
                    // Not run yet: the empty outline, so the column still reads as a checklist.
                    using (var pen = new Pen(p.CardBorder)) g.DrawEllipse(pen, circle.X + 0.5f, circle.Y + 0.5f, circle.Width - 1f, circle.Height - 1f);
                    g.SmoothingMode = saved;
                    return;
            }
        }
    }
}
