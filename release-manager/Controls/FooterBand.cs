using System.ComponentModel;
using System.Windows.Forms;
using ReleaseManager.Services;

namespace ReleaseManager.Controls
{
    /// <summary>
    /// A dialog's footer: the --gb-head band with a 1 px --pn-border top edge, padded 24, holding
    /// the dialog's buttons.
    /// </summary>
    [ToolboxItem(true)]
    public sealed class FooterBand : SurfacePanel
    {
        public FooterBand()
        {
            Surface = SurfaceKind.Custom;
            CornerRadius = 0;
            Dock = DockStyle.Bottom;
            Height = 80;
            ApplyColours();
        }

        protected override void OnThemeChanged(object sender, System.EventArgs e)
        {
            ApplyColours();
            base.OnThemeChanged(sender, e);
        }

        private void ApplyColours() => CustomFill = Theme.Palette.FooterBand;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw.HLine(e.Graphics, 0, 0, Width, Theme.Palette.CardBorder);
        }
    }
}
