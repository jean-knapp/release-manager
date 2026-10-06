using System;
using System.Drawing;
using ModernWinForms;
using ModernWinForms.Enums;

namespace ReleaseManager.Services
{
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public enum ThemeMode
    {
        Dark,
        Light,
    }

    /// <summary>
    /// The design tokens of the ModernWinForms design system (its Windows 11 alignment target),
    /// one instance per theme. Every custom-drawn control reads its colours from
    /// <see cref="Theme.Palette"/>, and <see cref="Theme"/> copies them into the ModernSkin the
    /// library controls draw with, so a theme switch repaints the whole application consistently.
    /// The comment after each token names its CSS variable in the design files.
    /// </summary>
    public sealed class ThemePalette
    {
        public ThemeMode Mode { get; private set; }

        // Surfaces
        public Color Background { get; private set; }      // --bg
        public Color Layer { get; private set; }           // --card, flattened onto --bg
        public Color Fill { get; private set; }
        public Color Fill2 { get; private set; }
        public Color Stroke { get; private set; }
        public Color CardStroke { get; private set; }
        public Color CardBorder { get; private set; }      // --pn-border: cards, lists, settings cards
        public Color Divider { get; private set; }         // --sep
        public Color Hover { get; private set; }           // --hover
        public Color Selection { get; private set; }       // --sel
        public Color ListBackground { get; private set; }  // --tv-bg: lists and the log
        public Color ListHover { get; private set; }       // --tv-hover
        public Color ListSelection { get; private set; }   // --tv-sel
        public Color FooterBand { get; private set; }      // --gb-head: dialog footers
        public Color TileFill { get; private set; }        // --tile-bg: placeholder icon tiles
        public Color TileBorder { get; private set; }      // --tile-border

        // Text
        public Color Foreground { get; private set; }      // --fg
        public Color Foreground2 { get; private set; }     // --fg2
        public Color Foreground3 { get; private set; }     // --fg3
        public Color DisabledForeground { get; private set; } // --btn-dis-fg

        // Accent
        public Color Accent { get; private set; }          // --accent
        public Color AccentFill { get; private set; }      // --acc-bg
        public Color AccentHover { get; private set; }     // --acc-hover
        public Color AccentPressed { get; private set; }   // --acc-press
        public Color AccentForeground { get; private set; }// --acc-fg

        // Buttons
        public Color ButtonFill { get; private set; }      // --btn-bg
        public Color ButtonHover { get; private set; }     // --btn-hover
        public Color ButtonPressed { get; private set; }   // --btn-press
        public Color ButtonBorder { get; private set; }    // --btn-border
        public Color ButtonBorderBottom { get; private set; } // --btn-border-b
        public Color SubtleHover { get; private set; }     // --sub-hover
        public Color SubtlePressed { get; private set; }   // --sub-press

        // Switches, check boxes and progress
        public Color SwitchOffBorder { get; private set; } // --sw-off-border
        public Color SwitchOffThumb { get; private set; }  // --sw-off-thumb
        public Color CheckOffFill { get; private set; }    // --radio-off-bg
        public Color ProgressTrack { get; private set; }   // --pb-track

        // Severity: InfoBars and step status only
        public Color OkBack { get; private set; }          // --ib-ok-bg
        public Color OkIcon { get; private set; }          // --ib-ok-icon
        public Color WarnBack { get; private set; }        // --ib-warn-bg
        public Color WarnIcon { get; private set; }        // --ib-warn-icon
        public Color ErrBack { get; private set; }         // --ib-err-bg
        public Color ErrIcon { get; private set; }         // --ib-err-icon

        // Older names the log and the welcome page use, mapped onto the severity icons.
        public Color Up { get; private set; }
        public Color Down { get; private set; }
        public Color Warning { get; private set; }
        public Color Error { get; private set; }
        public Color Folder { get; private set; }
        public Color Lane { get; private set; }
        public Color Lane2 { get; private set; }

        public static readonly ThemePalette Dark = new ThemePalette
        {
            Mode = ThemeMode.Dark,
            Background = Rgb(0x202020),
            // rgba(255,255,255,.03) over #202020.
            Layer = Rgb(0x272727),
            Fill = Argb(14, 255, 255, 255),
            Fill2 = Argb(23, 255, 255, 255),
            Stroke = Argb(23, 255, 255, 255),
            CardStroke = Argb(21, 255, 255, 255),
            CardBorder = Rgb(0x373737),
            Divider = Argb(21, 255, 255, 255),       // rgba(255,255,255,.082)
            Hover = Argb(15, 255, 255, 255),         // .059
            Selection = Argb(19, 255, 255, 255),     // .075
            ListBackground = Rgb(0x262626),
            ListHover = Argb(15, 255, 255, 255),     // .06
            ListSelection = Argb(20, 255, 255, 255), // .08
            FooterBand = Rgb(0x2b2b2b),
            TileFill = Argb(18, 255, 255, 255),      // .07
            TileBorder = Argb(21, 255, 255, 255),    // .082
            Foreground = Rgb(0xffffff),
            Foreground2 = Rgb(0xc8c8c8),
            Foreground3 = Rgb(0x9d9d9d),
            DisabledForeground = Rgb(0x717171),
            Accent = Rgb(0x4cc2ff),
            AccentFill = Rgb(0x4cc2ff),
            AccentHover = Rgb(0x47b1e8),
            AccentPressed = Rgb(0x42a1d2),
            AccentForeground = Rgb(0x000000),
            ButtonFill = Rgb(0x2d2d2d),
            ButtonHover = Rgb(0x323232),
            ButtonPressed = Rgb(0x272727),
            ButtonBorder = Argb(24, 255, 255, 255),  // .093
            ButtonBorderBottom = Argb(18, 255, 255, 255), // .07
            SubtleHover = Argb(15, 255, 255, 255),   // .059
            SubtlePressed = Argb(11, 255, 255, 255), // .043
            SwitchOffBorder = Rgb(0xa0a0a0),
            SwitchOffThumb = Rgb(0xcccccc),
            CheckOffFill = Argb(26, 0, 0, 0),        // rgba(0,0,0,.1)
            ProgressTrack = Rgb(0x373737),
            OkBack = Rgb(0x393d1b),
            OkIcon = Rgb(0x6ccb5f),
            WarnBack = Rgb(0x433519),
            WarnIcon = Rgb(0xfce100),
            ErrBack = Rgb(0x442726),
            ErrIcon = Rgb(0xff99a4),
            Up = Rgb(0x6ccb5f),
            Down = Rgb(0x4cc2ff),
            Warning = Rgb(0xfce100),
            Error = Rgb(0xff99a4),
            Folder = Rgb(0xffc45c),
            Lane = Rgb(0x4cc2ff),
            Lane2 = Rgb(0xb284ff),
        };

        public static readonly ThemePalette Light = new ThemePalette
        {
            Mode = ThemeMode.Light,
            Background = Rgb(0xf3f3f3),
            // rgba(255,255,255,.5) over #F3F3F3.
            Layer = Rgb(0xf9f9f9),
            Fill = Argb(7, 0, 0, 0),
            Fill2 = Argb(15, 0, 0, 0),
            Stroke = Argb(28, 0, 0, 0),
            CardStroke = Argb(20, 0, 0, 0),
            CardBorder = Rgb(0xe5e5e5),
            Divider = Argb(20, 0, 0, 0),             // rgba(0,0,0,.078)
            Hover = Argb(10, 0, 0, 0),               // .04
            Selection = Argb(14, 0, 0, 0),           // .055
            ListBackground = Rgb(0xffffff),
            ListHover = Argb(9, 0, 0, 0),            // .037
            ListSelection = Argb(15, 0, 0, 0),       // .06
            FooterBand = Rgb(0xfbfbfb),
            TileFill = Argb(12, 0, 0, 0),            // .047
            TileBorder = Argb(20, 0, 0, 0),          // .078
            Foreground = Rgb(0x1d1d1d),
            Foreground2 = Rgb(0x606060),
            Foreground3 = Rgb(0x8a8a8a),
            DisabledForeground = Rgb(0xa0a0a0),
            Accent = Rgb(0x005fb8),
            AccentFill = Rgb(0x005fb8),
            AccentHover = Rgb(0x196ebf),
            AccentPressed = Rgb(0x317cc5),
            AccentForeground = Rgb(0xffffff),
            ButtonFill = Rgb(0xfbfbfb),
            ButtonHover = Rgb(0xf5f5f5),
            ButtonPressed = Rgb(0xeeeeee),
            ButtonBorder = Argb(19, 0, 0, 0),        // .073
            ButtonBorderBottom = Argb(41, 0, 0, 0),  // .16
            SubtleHover = Argb(10, 0, 0, 0),         // .04
            SubtlePressed = Argb(6, 0, 0, 0),        // .024
            SwitchOffBorder = Rgb(0x8c8c8c),
            SwitchOffThumb = Rgb(0x5f5f5f),
            CheckOffFill = Argb(6, 0, 0, 0),         // rgba(0,0,0,.024)
            ProgressTrack = Rgb(0xe6e6e6),
            OkBack = Rgb(0xdff6dd),
            OkIcon = Rgb(0x0f7b0f),
            WarnBack = Rgb(0xfff4ce),
            WarnIcon = Rgb(0x9d5d00),
            ErrBack = Rgb(0xfde7e9),
            ErrIcon = Rgb(0xc42b1c),
            Up = Rgb(0x0f7b0f),
            Down = Rgb(0x005fb8),
            Warning = Rgb(0x9d5d00),
            Error = Rgb(0xc42b1c),
            Folder = Rgb(0xc98a00),
            Lane = Rgb(0x005fb8),
            Lane2 = Rgb(0x6b3fc9),
        };

        private static Color Rgb(int value) => Color.FromArgb(255, (value >> 16) & 0xff, (value >> 8) & 0xff, value & 0xff);
        private static Color Argb(int alpha, int r, int g, int b) => Color.FromArgb(alpha, r, g, b);

        /// <summary>Flattens a translucent token onto a background so it can be used where alpha is not supported.</summary>
        public static Color Flatten(Color over, Color under)
        {
            if (over.A == 255) return over;
            float a = over.A / 255f;
            return Color.FromArgb(255,
                (int)Math.Round(over.R * a + under.R * (1 - a)),
                (int)Math.Round(over.G * a + under.G * (1 - a)),
                (int)Math.Round(over.B * a + under.B * (1 - a)));
        }

        /// <summary>A semantic colour at a fraction of its strength over a surface.</summary>
        public static Color Tint(Color semantic, double strength, Color surface)
        {
            int alpha = (int)Math.Round(255 * Math.Max(0, Math.Min(1, strength)));
            return Flatten(Color.FromArgb(alpha, semantic.R, semantic.G, semantic.B), surface);
        }

        public Color FillOn(Color surface) => Flatten(Fill, surface);
        public Color Fill2On(Color surface) => Flatten(Fill2, surface);
        public Color StrokeOn(Color surface) => Flatten(Stroke, surface);
        public Color CardStrokeOn(Color surface) => Flatten(CardStroke, surface);
        public Color DividerOn(Color surface) => Flatten(Divider, surface);
        public Color HoverOn(Color surface) => Flatten(Hover, surface);
        public Color SelectionOn(Color surface) => Flatten(Selection, surface);
    }

    /// <summary>Application-wide theme. Controls read <see cref="Palette"/> and repaint on <see cref="Changed"/>.</summary>
    public static class Theme
    {
        private static ThemeMode _mode = ThemeMode.Dark;
        private static ModernSkin _skin;

        /// <summary>Raised after the theme changes, so custom-drawn controls can repaint.</summary>
        public static event EventHandler Changed;

        public static ThemeMode Mode
        {
            get => _mode;
            set
            {
                if (_mode == value) return;
                _mode = value;
                // The skin instance is kept and re-filled, so every control already bound to it
                // repaints through ModernSkin.Changed instead of needing a new reference.
                if (_skin != null) CopyInto(_skin, Palette);
                Changed?.Invoke(null, EventArgs.Empty);
            }
        }

        public static ThemePalette Palette => _mode == ThemeMode.Light ? ThemePalette.Light : ThemePalette.Dark;

        /// <summary>A ModernSkin built from the palette, so library controls match the custom-drawn ones.</summary>
        public static ModernSkin Skin => _skin ?? (_skin = BuildSkin(Palette));

        /// <summary>Applies the current theme to a skin instance that forms already reference.</summary>
        public static void Apply(ModernSkin target)
        {
            if (target == null) return;
            CopyInto(target, Palette);
        }

        private static ModernSkin BuildSkin(ThemePalette p)
        {
            var skin = new ModernSkin();
            CopyInto(skin, p);
            return skin;
        }

        private static void CopyInto(ModernSkin s, ThemePalette p)
        {
            bool dark = p.Mode == ThemeMode.Dark;
            var layer = p.Layer;
            var popup = dark ? Color.FromArgb(44, 44, 44) : Color.FromArgb(252, 252, 252);        // --dd-bg
            var popupStroke = dark ? Color.FromArgb(62, 62, 62) : Color.FromArgb(26, 0, 0, 0);    // --dd-border

            s.Control.BackColor = p.Background;
            s.Control.ForeColor = p.Foreground;

            s.Form.TitleBar.BackColor = p.Background;
            s.Form.TitleBar.ButtonColor = p.Foreground;
            s.Form.TitleBar.TitleColor = p.Foreground;

            s.Label.BackColor = Color.Transparent;
            s.Label.ForeColor = p.Foreground;

            s.Panel.BackColor = p.Background;
            s.Panel.BorderColor = Color.Transparent;
            s.Panel.CornerStyle = CornerStyle.Square;

            s.GroupBox.BackColor = layer;
            s.GroupBox.BorderColor = p.CardBorder;
            s.GroupBox.CornerStyle = CornerStyle.Round;
            s.GroupBox.Header.BackColor = p.FooterBand;
            s.GroupBox.Header.ForeColor = p.Foreground;

            // Text fields: a faint outline with a stronger bottom edge, a 2 px accent underline on focus.
            var fieldFill = dark ? Color.FromArgb(45, 45, 45) : Color.FromArgb(251, 251, 251);         // --tb-bg
            var fieldHover = dark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(246, 246, 246);        // --tb-hover-bg
            var fieldFocus = dark ? Color.FromArgb(31, 31, 31) : Color.White;                           // --tb-focus-bg
            var fieldStroke = dark ? Color.FromArgb(20, 255, 255, 255) : Color.FromArgb(19, 0, 0, 0); // --tb-border
            var fieldBottom = dark ? Color.FromArgb(138, 255, 255, 255) : Color.FromArgb(115, 0, 0, 0); // --tb-border-b
            var fieldDisabled = dark ? Color.FromArgb(39, 39, 39) : Color.FromArgb(245, 245, 245);
            var fieldDisabledStroke = dark ? Color.FromArgb(18, 255, 255, 255) : Color.FromArgb(15, 0, 0, 0);

            s.TextBox.CornerStyle = CornerStyle.Round;
            s.TextBox.Colors.BorderColor = fieldStroke;
            s.TextBox.Colors.UnderlineColor = fieldBottom;
            s.TextBox.Colors.Normal.BackColor = fieldFill;
            s.TextBox.Colors.Normal.ForeColor = p.Foreground;
            s.TextBox.Colors.Hover.BackColor = fieldHover;
            s.TextBox.Colors.Hover.BorderColor = fieldStroke;
            s.TextBox.Colors.Active.BackColor = fieldFocus;
            s.TextBox.Colors.Active.BorderColor = p.Accent;
            s.TextBox.Colors.Disabled.BackColor = fieldDisabled;
            s.TextBox.Colors.Disabled.ForeColor = p.DisabledForeground;
            s.TextBox.Colors.Disabled.BorderColor = fieldDisabledStroke;

            s.ComboBox.CornerStyle = CornerStyle.Round;
            s.ComboBox.Colors.BorderColor = p.ButtonBorder;
            s.ComboBox.Colors.UnderlineColor = p.ButtonBorderBottom;
            s.ComboBox.Colors.Normal.BackColor = fieldFill;
            s.ComboBox.Colors.Normal.ForeColor = p.Foreground;
            s.ComboBox.Colors.Hover.BackColor = fieldHover;
            s.ComboBox.Colors.Hover.BorderColor = p.ButtonBorder;
            s.ComboBox.Colors.Active.BackColor = fieldFocus;
            s.ComboBox.Colors.Active.BorderColor = p.Accent;
            s.ComboBox.Colors.Disabled.BackColor = fieldDisabled;
            s.ComboBox.Colors.Disabled.ForeColor = p.DisabledForeground;
            s.ComboBox.Colors.Disabled.BorderColor = fieldDisabledStroke;
            s.ComboBox.DropDown.BackColor = popup;
            s.ComboBox.DropDown.BorderColor = popupStroke;
            s.ComboBox.DropDown.HoverColor = p.ListHover;
            s.ComboBox.DropDown.SelectedColor = p.Accent;
            s.ComboBox.DropDown.CornerStyle = CornerStyle.Round;

            s.Button.CornerStyle = CornerStyle.Round;
            s.Button.Colors.BorderColor = p.ButtonBorder;
            s.Button.Colors.BorderBottomColor = p.ButtonBorderBottom;
            s.Button.Colors.Normal.BackColor = p.ButtonFill;
            s.Button.Colors.Normal.ForeColor = p.Foreground;
            s.Button.Colors.Hover.BackColor = p.ButtonHover;
            s.Button.Colors.Pressed.BackColor = p.ButtonPressed;
            s.Button.Colors.Disabled.BackColor = p.ButtonFill;
            s.Button.Colors.Disabled.ForeColor = p.DisabledForeground;
            s.Button.Colors.Checked.BackColor = p.AccentFill;
            s.Button.Colors.Checked.HoverBackColor = p.AccentHover;
            s.Button.Colors.Checked.ForeColor = p.AccentForeground;
            s.Button.Colors.Checked.BorderColor = Color.Transparent;
            s.Button.SubtleColors.Normal.ForeColor = p.Foreground;
            s.Button.SubtleColors.Hover.BackColor = p.SubtleHover;
            s.Button.SubtleColors.Pressed.BackColor = p.SubtlePressed;
            s.Button.SubtleColors.Disabled.ForeColor = p.DisabledForeground;
            s.Button.AccentColors.Normal.BackColor = p.AccentFill;
            s.Button.AccentColors.Normal.ForeColor = p.AccentForeground;
            s.Button.AccentColors.Hover.BackColor = p.AccentHover;
            s.Button.AccentColors.Pressed.BackColor = p.AccentPressed;
            s.Button.AccentColors.Disabled.BackColor = dark ? Color.FromArgb(67, 67, 67) : Color.FromArgb(56, 0, 0, 0);
            s.Button.AccentColors.Disabled.ForeColor = dark ? Color.FromArgb(167, 167, 167) : Color.White;

            s.SegmentedControl.TrackColor = dark ? Color.FromArgb(28, 28, 28) : Color.FromArgb(237, 237, 237);       // --sg-track
            s.SegmentedControl.BorderColor = dark ? Color.FromArgb(18, 255, 255, 255) : Color.FromArgb(15, 0, 0, 0); // --sg-border
            s.SegmentedControl.SelectedBackColor = dark ? Color.FromArgb(45, 45, 45) : Color.White;                  // --sg-sel
            s.SegmentedControl.SelectedBorderColor = p.ButtonBorder;
            s.SegmentedControl.SelectedBorderBottomColor = p.ButtonBorderBottom;
            s.SegmentedControl.ForeColor = p.Foreground;

            s.InfoBar.Colors.InformationalBackColor = dark ? Color.FromArgb(43, 43, 43) : Color.FromArgb(246, 246, 246);
            s.InfoBar.Colors.SuccessBackColor = p.OkBack;
            s.InfoBar.Colors.WarningBackColor = p.WarnBack;
            s.InfoBar.Colors.ErrorBackColor = p.ErrBack;
            s.InfoBar.Colors.InformationalIconColor = p.Accent;
            s.InfoBar.Colors.SuccessIconColor = p.OkIcon;
            s.InfoBar.Colors.WarningIconColor = p.WarnIcon;
            s.InfoBar.Colors.ErrorIconColor = p.ErrIcon;
            s.InfoBar.Colors.ForeColor = dark ? Color.White : Color.FromArgb(27, 27, 27);
            s.InfoBar.Colors.BorderColor = p.Divider;

            s.TreeView.BackColor = p.ListBackground;
            s.TreeView.BorderColor = Color.Transparent;
            s.TreeView.CornerStyle = CornerStyle.Square;
            s.TreeView.Colors.Normal.BackColor = p.ListBackground;
            s.TreeView.Colors.Normal.ForeColor = p.Foreground;
            s.TreeView.Colors.Hover.BackColor = p.ListHover;
            s.TreeView.Colors.Hover.ForeColor = p.Foreground;
            s.TreeView.Colors.Selected.BackColor = p.ListSelection;
            s.TreeView.Colors.Selected.ForeColor = p.Foreground;
            s.TreeView.Colors.SelectedUnfocused.BackColor = p.ListHover;
            s.TreeView.Colors.SelectedUnfocused.ForeColor = p.Foreground2;
            s.TreeView.Colors.Header.BackColor = p.FooterBand;
            s.TreeView.Colors.Header.ForeColor = p.Foreground2;

            s.ScrollBar.TrackColor = Color.Transparent;
            s.ScrollBar.ThumbColor = dark ? Color.FromArgb(106, 106, 106) : Color.FromArgb(194, 194, 194);       // --sb-thumb
            s.ScrollBar.ThumbHoverColor = dark ? Color.FromArgb(140, 140, 140) : Color.FromArgb(138, 138, 138);

            s.SplitContainer.Colors.SplitterColor = p.Background;
            s.SplitContainer.Colors.PressedColor = p.Accent;

            s.ProgressBar.CornerStyle = CornerStyle.Round;
            s.ProgressBar.Colors.TrackColor = p.ProgressTrack;
            s.ProgressBar.Colors.BorderColor = Color.Transparent;
            s.ProgressBar.Colors.FillColor = p.AccentFill;

            s.ToggleSwitch.OffTrackColor = Color.Transparent;
            s.ToggleSwitch.OffBorderColor = p.SwitchOffBorder;
            s.ToggleSwitch.OffThumbColor = p.SwitchOffThumb;
            s.ToggleSwitch.OnTrackColor = p.AccentFill;
            s.ToggleSwitch.OnThumbColor = p.AccentForeground;
            s.ToggleSwitch.ForeColor = p.Foreground;

            s.ToolTip.BackColor = dark ? Color.FromArgb(44, 44, 44) : Color.FromArgb(249, 249, 249);
            s.ToolTip.BorderColor = popupStroke;
            s.ToolTip.TitleColor = p.Foreground;
            s.ToolTip.TextColor = p.Foreground2;
            s.ToolTip.CornerStyle = CornerStyle.Round;

            s.MenuBar.BackColor = Color.Transparent;
            s.MenuBar.DropDown.BackColor = popup;
            s.MenuBar.DropDown.ForeColor = p.Foreground;
            s.MenuBar.DropDown.HoverColor = p.ListHover;
            s.MenuBar.DropDown.BorderColor = popupStroke;
            s.MenuBar.DropDown.CornerStyle = CornerStyle.Round;
            s.MenuBar.ItemColors.Normal.BackColor = Color.Transparent;
            s.MenuBar.ItemColors.Normal.ForeColor = p.Foreground;
            s.MenuBar.ItemColors.Hover.BackColor = p.SubtleHover;
            s.MenuBar.ItemColors.Hover.ForeColor = p.Foreground;
            s.MenuBar.ItemColors.Pressed.BackColor = p.SubtlePressed;
            s.MenuBar.ItemColors.Pressed.ForeColor = p.Foreground;
            s.MenuBar.ItemColors.Disabled.BackColor = Color.Transparent;
            s.MenuBar.ItemColors.Disabled.ForeColor = p.DisabledForeground;

            s.TabControl.Colors.BackColor = Color.Transparent;
            s.TabControl.Colors.TabForeColor = p.Foreground2;
            s.TabControl.Colors.HoverBackColor = p.SubtleHover;
            s.TabControl.Colors.HoverForeColor = p.Foreground;
            s.TabControl.Colors.SelectedForeColor = p.Foreground;
            s.TabControl.Colors.SelectedBackColor = p.FillOn(p.Background);
            s.TabControl.Colors.AccentColor = p.AccentFill;
            s.TabControl.Colors.SeparatorColor = Color.Transparent;
        }
    }
}
