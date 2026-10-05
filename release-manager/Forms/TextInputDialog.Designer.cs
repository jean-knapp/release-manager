namespace ReleaseManager.Forms
{
    partial class TextInputDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.skin = new ModernWinForms.ModernSkin();
            this.content = new ReleaseManager.Controls.SurfacePanel();
            this.promptLabel = new ReleaseManager.Controls.TextLabel();
            this.valueBox = new ModernWinForms.ModernTextBox();
            this.okButton = new ReleaseManager.Controls.CommandButton();
            this.cancelButton = new ReleaseManager.Controls.CommandButton();
            this.content.SuspendLayout();
            this.SuspendLayout();
            //
            // promptLabel
            //
            this.promptLabel.Location = new System.Drawing.Point(24, 22);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.promptLabel.Size = new System.Drawing.Size(404, 20);
            this.promptLabel.SizePx = 13F;
            this.promptLabel.TabIndex = 0;
            this.promptLabel.Text = "Value";
            //
            // valueBox
            //
            this.valueBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.valueBox.Location = new System.Drawing.Point(24, 48);
            this.valueBox.Name = "valueBox";
            this.valueBox.Size = new System.Drawing.Size(404, 34);
            this.valueBox.TabIndex = 1;
            this.valueBox.TextChanged += new System.EventHandler(this.valueBox_TextChanged);
            //
            // okButton
            //
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Accent;
            this.okButton.CornerRadius = 5;
            this.okButton.Location = new System.Drawing.Point(230, 100);
            this.okButton.Name = "okButton";
            this.okButton.PaddingX = 20;
            this.okButton.Size = new System.Drawing.Size(92, 34);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.TextSizePx = 13.5F;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // cancelButton
            //
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Standard;
            this.cancelButton.CornerRadius = 5;
            this.cancelButton.Location = new System.Drawing.Point(330, 100);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.PaddingX = 20;
            this.cancelButton.Size = new System.Drawing.Size(98, 34);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.TextSizePx = 13.5F;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            //
            // TextInputDialog
            //
            this.ClientSize = new System.Drawing.Size(452, 183);
            this.content.Location = new System.Drawing.Point(0, 32);
            this.content.Size = new System.Drawing.Size(452, 151);
            this.content.Controls.Add(this.promptLabel);
            this.content.Controls.Add(this.valueBox);
            this.content.Controls.Add(this.okButton);
            this.content.Controls.Add(this.cancelButton);
            this.content.CornerRadius = 0;
            this.content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.content.Name = "content";
            this.content.Surface = ReleaseManager.Controls.SurfaceKind.Base;
            this.Controls.Add(this.content);
            this.MinimumSize = new System.Drawing.Size(360, 183);
            this.Name = "TextInputDialog";
            this.Skin = this.skin;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Release Manager";
            this.TitleBar.ShowMaximizeBox = false;
            this.TitleBar.ShowMinimizeBox = false;
            this.content.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private ModernWinForms.ModernSkin skin;
        private ReleaseManager.Controls.SurfacePanel content;
        private ReleaseManager.Controls.TextLabel promptLabel;
        private ModernWinForms.ModernTextBox valueBox;
        private ReleaseManager.Controls.CommandButton okButton;
        private ReleaseManager.Controls.CommandButton cancelButton;
    }
}
