using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReleaseManager.Services;

namespace ReleaseManager.Controls
{
    /// <summary>
    /// The design system's SettingsCard: a Windows Settings row with a header and a 12 px --fg2
    /// description on the left, an optional 16 px glyph before them, and its child controls (a
    /// switch, a button, a segmented control) at the trailing edge. A --card surface with a 1 px
    /// --pn-border outline and a 6 px radius; at least 68 px tall, padded 12 by 16.
    /// </summary>
    [ToolboxItem(true)]
    public class SettingsCard : SurfacePanel
    {
        private const int PadX = 16;
        private const int Gap = 16;
        private const int HeaderHeight = 20;

        private int _padY = 12;
        private int _padRight = 16;
        private int _minHeight = 68;

        private string _header = string.Empty;
        private string _description = string.Empty;
        private string _iconSvg;

        public SettingsCard()
        {
            Surface = SurfaceKind.Card;
            CornerRadius = 6;
            Size = new Size(360, _minHeight);
        }

        /// <summary>The card's least height: 68 for a settings row, 56 for the compact account card.</summary>
        [Category("Layout"), DefaultValue(68)]
        public int MinHeight { get => _minHeight; set { _minHeight = value; PerformLayout(); Invalidate(); } }

        [Category("Layout"), DefaultValue(12)]
        public int PaddingY { get => _padY; set { _padY = value; PerformLayout(); Invalidate(); } }

        /// <summary>Space right of the trailing controls; the account card brings its button to 8.</summary>
        [Category("Layout"), DefaultValue(16)]
        public int PaddingRight { get => _padRight; set { _padRight = value; PerformLayout(); Invalidate(); } }

        [Category("Appearance"), DefaultValue("")]
        public string Header { get => _header; set { _header = value ?? string.Empty; PerformLayout(); Invalidate(); } }

        [Category("Appearance"), DefaultValue("")]
        public string Description { get => _description; set { _description = value ?? string.Empty; PerformLayout(); Invalidate(); } }

        /// <summary>A 16 px glyph before the text, in --fg2.</summary>
        [Category("Appearance"), DefaultValue(null)]
        public string IconSvg { get => _iconSvg; set { _iconSvg = value; PerformLayout(); Invalidate(); } }

        private static Font HeaderFont => Fonts.Ui(14f);
        private static Font DescriptionFont => Fonts.Ui(12f);

        /// <summary>The text keeps the full 16 px from the edge when there are no trailing controls.</summary>
        private int TextRightPad => Controls.Cast<Control>().Any(c => c.Visible) ? _padRight : PadX;

        private int TextLeft => PadX + (string.IsNullOrEmpty(_iconSvg) ? 0 : 16 + Gap);

        /// <summary>Width the trailing controls take, with the gap before them; 0 when there are none.</summary>
        private int TrailingWidth()
        {
            var controls = Controls.Cast<Control>().Where(c => c.Visible).ToList();
            if (controls.Count == 0) return 0;
            return controls.Sum(c => c.Width) + (controls.Count - 1) * 8 + Gap;
        }

        private int DescriptionHeight(int textWidth)
        {
            if (string.IsNullOrEmpty(_description) || textWidth <= 0) return 0;
            var size = TextRenderer.MeasureText(_description, DescriptionFont, new Size(textWidth, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            return size.Height;
        }

        /// <summary>The height this card needs at <paramref name="width"/>, for the parent's layout.</summary>
        public int MeasureHeight(int width)
        {
            int textWidth = width - TextLeft - TextRightPad - TrailingWidth();
            int description = DescriptionHeight(textWidth);
            int content = HeaderHeight + (description > 0 ? 2 + description : 0);
            return Math.Max(_minHeight, content + _padY * 2);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            // Trailing controls, right to left, centred on the card's height.
            int right = Width - _padRight;
            foreach (var control in Controls.Cast<Control>().Where(c => c.Visible).Reverse())
            {
                control.Location = new Point(right - control.Width, (Height - control.Height) / 2);
                right = control.Left - 8;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PerformLayout();
            Invalidate();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.SizeChanged += (s, a) => PerformLayout();
            e.Control.VisibleChanged += (s, a) => { PerformLayout(); Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var p = Theme.Palette;
            int textLeft = TextLeft;
            int textWidth = Math.Max(0, Width - textLeft - TextRightPad - TrailingWidth());
            int description = DescriptionHeight(textWidth);
            int content = HeaderHeight + (description > 0 ? 2 + description : 0);
            int top = (Height - content) / 2;

            var fore = Enabled ? p.Foreground : p.DisabledForeground;
            if (!string.IsNullOrEmpty(_iconSvg))
            {
                IconCache.DrawCentered(g, _iconSvg, 16, Enabled ? p.Foreground2 : p.DisabledForeground, PadX + 8, Height / 2);
            }
            Draw.Text(g, _header, HeaderFont, new Rectangle(textLeft, top, textWidth, HeaderHeight), fore, Draw.LeftMiddle);
            if (description > 0)
            {
                TextRenderer.DrawText(g, _description, DescriptionFont, new Rectangle(textLeft, top + HeaderHeight + 2, textWidth, description),
                    Enabled ? p.Foreground2 : p.DisabledForeground, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }
    }
}
