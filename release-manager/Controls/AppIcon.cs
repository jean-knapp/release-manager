using System;
using System.Drawing;
using System.IO;
using ModernWinForms;
using ReleaseManager.Services;

namespace ReleaseManager.Controls
{
    /// <summary>
    /// The logo as a window icon, in the theme's variant: the package with its release arrow, or
    /// the box alone where the icon is drawn at 16 px.
    /// </summary>
    public static class AppIcon
    {
        private static Icon _icon;
        private static ThemeMode _iconMode;

        /// <summary>The logo for the current theme, built once per theme.</summary>
        public static Icon Current
        {
            get
            {
                if (_icon != null && _iconMode == Theme.Mode) return _icon;
                try
                {
                    bool dark = Theme.Mode == ThemeMode.Dark;
                    // Two sizes, so Windows picks the box alone for the 16 px title bar and the
                    // full logo for the taskbar.
                    var small = Dib(dark ? Icons.Logo16Dark : Icons.Logo16Light, 16);
                    var large = Dib(dark ? Icons.LogoDark : Icons.LogoLight, 32);
                    using (var stream = new MemoryStream(IcoFile(small, 16, large, 32)))
                    {
                        _icon = new Icon(stream);
                    }
                    _iconMode = Theme.Mode;
                }
                catch (Exception)
                {
                    // The default icon will do.
                }
                return _icon;
            }
        }

        /// <summary>
        /// The glyph as an icon entry: a 32-bit DIB (header, bottom-up BGRA rows, then the AND
        /// mask). System.Drawing's Icon cannot read PNG entries, so this is the classic format.
        /// </summary>
        private static byte[] Dib(string svg, int size)
        {
            using (var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    var image = IconCache.Get(svg, size, Color.Black);
                    if (image != null) g.DrawImage(image, 0, 0, size, size);
                }
                int maskStride = ((size + 31) / 32) * 4;
                writer.Write(40);                // BITMAPINFOHEADER
                writer.Write(size);
                writer.Write(size * 2);          // colour rows plus mask rows
                writer.Write((short)1);
                writer.Write((short)32);
                writer.Write(0);
                writer.Write(size * size * 4 + maskStride * size);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                for (int y = size - 1; y >= 0; y--)
                {
                    for (int x = 0; x < size; x++)
                    {
                        var c = bitmap.GetPixel(x, y);
                        writer.Write(c.B);
                        writer.Write(c.G);
                        writer.Write(c.R);
                        writer.Write(c.A);
                    }
                }
                // The alpha channel carries transparency, so the AND mask stays all opaque.
                writer.Write(new byte[maskStride * size]);
                return stream.ToArray();
            }
        }

        /// <summary>An .ico holding two images.</summary>
        private static byte[] IcoFile(byte[] first, int firstSize, byte[] second, int secondSize)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)0);
                writer.Write((short)1);
                writer.Write((short)2);
                int offset = 6 + 16 * 2;
                foreach (var (data, size) in new[] { (first, firstSize), (second, secondSize) })
                {
                    writer.Write((byte)size);
                    writer.Write((byte)size);
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((short)1);
                    writer.Write((short)32);
                    writer.Write(data.Length);
                    writer.Write(offset);
                    offset += data.Length;
                }
                writer.Write(first);
                writer.Write(second);
                return stream.ToArray();
            }
        }

        /// <summary>Shows the logo in a window's title bar and taskbar button, following theme changes.</summary>
        public static void Apply(ModernForm form)
        {
            form.TitleBar.ShowIcon = true;
            var icon = Current;
            if (icon != null) form.Icon = icon;
            EventHandler changed = null;
            changed = (s, e) =>
            {
                if (form.IsDisposed) { Theme.Changed -= changed; return; }
                var next = Current;
                if (next != null) form.Icon = next;
            };
            Theme.Changed += changed;
        }
    }
}
