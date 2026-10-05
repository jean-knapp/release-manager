using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ReleaseManager.Services;
using ModernWinForms;

namespace ReleaseManager.Controls
{
    /// <summary>
    /// The design system's toggle switch: a 40x20 track to the right of an "On"/"Off" state text
    /// (8 px gap). Off, the track is outlined in --sw-off-border with a 12 px --sw-off-thumb; on,
    /// it fills with the accent and the thumb takes --sw-on-thumb. The thumb grows to 14 px on
    /// hover. ModernToggleSwitch clears itself with the skin's panel colour, which stamps a --bg
    /// rectangle onto a card, so this draws against whatever surface actually sits behind it.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty("Checked")]
    [DefaultEvent("CheckedChanged")]
    public sealed class ToggleSwitchControl : ModernControl
    {
        private const int TrackWidth = 40;
        private const int TrackHeight = 20;
        private const int StateGap = 8;

        private bool _checked;
        private bool _hot;
        private bool _showState;
        private string _onText = "On";
        private string _offText = "Off";

        public event EventHandler CheckedChanged;

        public ToggleSwitchControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
            TabStop = true;
            BackColor = Color.Transparent;
            Size = new Size(TrackWidth, TrackHeight);
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
            _checked = value;
            Invalidate();
        }

        /// <summary>Shows "On"/"Off" to the left of the track, as Windows Settings does.</summary>
        [Category("Appearance"), DefaultValue(false)]
        public bool ShowState
        {
            get => _showState;
            set { _showState = value; Size = GetPreferredSize(Size.Empty); Invalidate(); }
        }

        [Category("Appearance"), DefaultValue("On")]
        public string OnText { get => _onText; set { _onText = value ?? string.Empty; Invalidate(); } }

        [Category("Appearance"), DefaultValue("Off")]
        public string OffText { get => _offText; set { _offText = value ?? string.Empty; Invalidate(); } }

        private int StateWidth => !_showState ? 0 :
            Math.Max(22, Math.Max(Draw.MeasureWidth(_onText, Fonts.Ui(14f)), Draw.MeasureWidth(_offText, Fonts.Ui(14f)))) + StateGap;

        public override Size GetPreferredSize(Size proposedSize) => new Size(StateWidth + TrackWidth, TrackHeight);

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
            Draw.Fill(g, ClientRectangle, surface);

            bool enabled = Enabled;
            // The design dims a disabled switch to 45 %.
            Func<Color, Color> dim = c => enabled ? c : ThemePalette.Flatten(Color.FromArgb(115, c), surface);

            if (_showState)
            {
                Draw.Text(g, _checked ? _onText : _offText, Fonts.Ui(14f), new Rectangle(0, 0, StateWidth, Height), dim(p.Foreground), Draw.LeftMiddle);
            }

            var track = new Rectangle(StateWidth, (Height - TrackHeight) / 2, TrackWidth, TrackHeight);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var path = Pill(new RectangleF(track.X + 0.5f, track.Y + 0.5f, track.Width - 1f, track.Height - 1f)))
            {
                if (_checked)
                {
                    using (var fill = new SolidBrush(dim(p.AccentFill))) g.FillPath(fill, path);
                }
                using (var pen = new Pen(dim(_checked ? p.AccentFill : p.SwitchOffBorder)))
                {
                    g.DrawPath(pen, path);
                }
            }

            // 12 px thumb, 14 px while hovered; it sits 3 px in from the track's ends.
            float knob = _hot && enabled ? 14f : 12f;
            float centreY = track.Y + TrackHeight / 2f;
            float centreX = _checked ? track.Right - 9f : track.X + 9f;
            using (var brush = new SolidBrush(dim(_checked ? p.AccentForeground : p.SwitchOffThumb)))
            {
                g.FillEllipse(brush, centreX - knob / 2f, centreY - knob / 2f, knob, knob);
            }

            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Default;

            if (Focused && ShowFocusCues)
            {
                var focus = new Rectangle(track.X - 2, track.Y - 2, track.Width + 3, track.Height + 3);
                Draw.DrawRounded(g, focus, focus.Height / 2f, p.Foreground, 1.5f);
            }
        }

        private static GraphicsPath Pill(RectangleF r)
        {
            var path = new GraphicsPath();
            float d = r.Height;
            path.AddArc(r.Left, r.Top, d, d, 90, 180);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 180);
            path.CloseFigure();
            return path;
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
