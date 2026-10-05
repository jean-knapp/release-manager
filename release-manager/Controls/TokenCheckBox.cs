using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ReleaseManager.Services;
using ModernWinForms;

namespace ReleaseManager.Controls
{
    /// <summary>
    /// The design system's check box: a 20 px box with a 4 px radius, outlined in --sw-off-border
    /// over --radio-off-bg, filled with the accent and a --sw-on-thumb check mark when set. The
    /// label sits 8 px to the right in 14 px --fg.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty("Checked")]
    [DefaultEvent("CheckedChanged")]
    public sealed class TokenCheckBox : ModernControl
    {
        private const int BoxSize = 20;
        private const int LabelGap = 8;

        /// <summary>The check mark on the design system's 18-unit grid.</summary>
        private const string Mark = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 18 18\"><path d=\"M4.5 9.2l3 3 6-6.4\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.4\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>";

        private bool _checked;
        private float _textSizePx = 14f;
        private bool _hot;

        public event EventHandler CheckedChanged;

        public TokenCheckBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
            TabStop = true;
            BackColor = Color.Transparent;
            Size = new Size(200, 24);
            Theme.Changed += (s, e) => Invalidate();
        }

        [Category("Behavior"), DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Sets the state without raising <see cref="CheckedChanged"/>.</summary>
        public void SetCheckedQuiet(bool value)
        {
            if (_checked == value) return;
            _checked = value;
            Invalidate();
        }

        [Category("Appearance"), DefaultValue(14f)]
        public float TextSizePx
        {
            get => _textSizePx;
            set { _textSizePx = Math.Max(6f, value); Invalidate(); }
        }

        [Category("Appearance")]
        public override string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }

        private Color ParentSurface()
        {
            for (var c = Parent; c != null; c = c.Parent)
            {
                var surface = c as SurfacePanel;
                if (surface != null) return surface.SurfaceColor;
            }
            return Theme.Palette.Layer;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var p = Theme.Palette;
            var surface = ParentSurface();
            var box = new Rectangle(0, (Height - BoxSize) / 2, BoxSize, BoxSize);
            bool enabled = Enabled;

            if (_checked)
            {
                var fill = enabled ? p.AccentFill : ThemePalette.Flatten(p.Mode == ThemeMode.Dark ? Color.FromArgb(41, 255, 255, 255) : Color.FromArgb(55, 0, 0, 0), surface);
                Draw.FillRounded(g, box, 4f, fill);
                IconCache.DrawCentered(g, Mark, 18, p.AccentForeground, box.X + box.Width / 2, box.Y + box.Height / 2);
            }
            else
            {
                var fill = ThemePalette.Flatten(p.CheckOffFill, surface);
                if (_hot && enabled) fill = ThemePalette.Flatten(p.SubtleHover, fill);
                Draw.FillRounded(g, box, 4f, fill);
                var border = enabled ? p.SwitchOffBorder : ThemePalette.Flatten(Color.FromArgb(153, p.SwitchOffBorder), surface);
                Draw.DrawRounded(g, new Rectangle(box.X, box.Y, box.Width - 1, box.Height - 1), 4f, border);
            }

            var textRect = new Rectangle(BoxSize + LabelGap, 0, Math.Max(0, Width - BoxSize - LabelGap), Height);
            var fore = enabled ? p.Foreground : p.DisabledForeground;
            Draw.Text(g, Text, Fonts.Ui(_textSizePx), textRect, fore, Draw.LeftMiddle);

            if (Focused && ShowFocusCues) Draw.DrawRounded(g, new Rectangle(0, 0, Width - 1, Height - 1), 4f, p.Foreground, 1f);
        }

        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hot = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hot = false; Invalidate(); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !Enabled) return;
            Focus();
            Checked = !Checked;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Space) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space && Enabled)
            {
                e.Handled = true;
                Checked = !Checked;
            }
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    }
}
