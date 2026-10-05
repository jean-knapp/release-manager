namespace ReleaseManager.Forms
{
    partial class CreateRepositoryDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.skin = new ModernWinForms.ModernSkin();
            this.content = new ReleaseManager.Controls.SurfacePanel();
            this.headingLabel = new ReleaseManager.Controls.TextLabel();
            this.accountLabel = new ReleaseManager.Controls.TextLabel();
            this.ownerLabel = new ReleaseManager.Controls.TextLabel();
            this.ownerCombo = new ModernWinForms.ModernComboBox();
            this.nameLabel = new ReleaseManager.Controls.TextLabel();
            this.nameBox = new ModernWinForms.ModernTextBox();
            this.urlLabel = new ReleaseManager.Controls.TextLabel();
            this.descriptionLabel = new ReleaseManager.Controls.TextLabel();
            this.optionalLabel = new ReleaseManager.Controls.TextLabel();
            this.descriptionBox = new ModernWinForms.ModernTextBox();
            this.visibilityCard = new ReleaseManager.Controls.SettingsCard();
            this.visibilitySwitch = new ModernWinForms.ModernSegmentedControl();
            this.readmeCard = new ReleaseManager.Controls.SettingsCard();
            this.readmeSwitch = new ReleaseManager.Controls.ToggleSwitchControl();
            this.statusLabel = new ReleaseManager.Controls.TextLabel();
            this.footer = new ReleaseManager.Controls.FooterBand();
            this.createButton = new ReleaseManager.Controls.CommandButton();
            this.cancelButton = new ReleaseManager.Controls.CommandButton();
            this.content.SuspendLayout();
            this.visibilityCard.SuspendLayout();
            this.readmeCard.SuspendLayout();
            this.footer.SuspendLayout();
            this.SuspendLayout();
            //
            // content: padding 20 24 16, 16 px between blocks.
            //
            this.content.Controls.Add(this.headingLabel);
            this.content.Controls.Add(this.accountLabel);
            this.content.Controls.Add(this.ownerLabel);
            this.content.Controls.Add(this.ownerCombo);
            this.content.Controls.Add(this.nameLabel);
            this.content.Controls.Add(this.nameBox);
            this.content.Controls.Add(this.urlLabel);
            this.content.Controls.Add(this.descriptionLabel);
            this.content.Controls.Add(this.optionalLabel);
            this.content.Controls.Add(this.descriptionBox);
            this.content.Controls.Add(this.visibilityCard);
            this.content.Controls.Add(this.readmeCard);
            this.content.Controls.Add(this.statusLabel);
            this.content.CornerRadius = 0;
            this.content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.content.Name = "content";
            this.content.Surface = ReleaseManager.Controls.SurfaceKind.Base;
            //
            // headingLabel
            //
            this.headingLabel.Name = "headingLabel";
            this.headingLabel.Semibold = true;
            this.headingLabel.SizePx = 20F;
            this.headingLabel.Text = "Create a repository for releases";
            //
            // accountLabel
            //
            this.accountLabel.Name = "accountLabel";
            this.accountLabel.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.accountLabel.SizePx = 12F;
            this.accountLabel.Text = "Looking up your account…";
            //
            // ownerLabel
            //
            this.ownerLabel.Name = "ownerLabel";
            this.ownerLabel.SizePx = 14F;
            this.ownerLabel.Text = "Owner";
            //
            // ownerCombo
            //
            this.ownerCombo.Name = "ownerCombo";
            this.ownerCombo.TabIndex = 0;
            this.ownerCombo.SelectedIndexChanged += new System.EventHandler<ModernWinForms.SelectedIndexChangedEventArgs>(this.ownerCombo_SelectedIndexChanged);
            //
            // nameLabel
            //
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.SizePx = 14F;
            this.nameLabel.Text = "Repository name";
            //
            // nameBox
            //
            this.nameBox.Name = "nameBox";
            this.nameBox.PlaceholderText = "my-program";
            this.nameBox.TabIndex = 1;
            this.nameBox.TextChanged += new System.EventHandler(this.field_TextChanged);
            //
            // urlLabel
            //
            this.urlLabel.Name = "urlLabel";
            this.urlLabel.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.urlLabel.SizePx = 12F;
            //
            // descriptionLabel
            //
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.SizePx = 14F;
            this.descriptionLabel.Text = "Description";
            //
            // optionalLabel
            //
            this.optionalLabel.Name = "optionalLabel";
            this.optionalLabel.Role = ReleaseManager.Controls.TextRole.Tertiary;
            this.optionalLabel.SizePx = 14F;
            this.optionalLabel.Text = "(optional)";
            //
            // descriptionBox
            //
            this.descriptionBox.Name = "descriptionBox";
            this.descriptionBox.TabIndex = 2;
            //
            // visibilityCard
            //
            this.visibilityCard.Controls.Add(this.visibilitySwitch);
            this.visibilityCard.Header = "Visibility";
            this.visibilityCard.Name = "visibilityCard";
            this.visibilityCard.TabIndex = 3;
            //
            // visibilitySwitch
            //
            this.visibilitySwitch.Items.Add("Public");
            this.visibilitySwitch.Items.Add("Private");
            this.visibilitySwitch.Name = "visibilitySwitch";
            this.visibilitySwitch.SegmentPadding = 14;
            this.visibilitySwitch.Size = new System.Drawing.Size(150, 32);
            this.visibilitySwitch.TabIndex = 0;
            this.visibilitySwitch.SelectedIndexChanged += new System.EventHandler(this.visibilitySwitch_SelectedIndexChanged);
            //
            // readmeCard
            //
            this.readmeCard.Controls.Add(this.readmeSwitch);
            this.readmeCard.Description = "Gives the first release a commit to tag.";
            this.readmeCard.Header = "Start with a README";
            this.readmeCard.Name = "readmeCard";
            this.readmeCard.TabIndex = 4;
            //
            // readmeSwitch
            //
            this.readmeSwitch.Checked = true;
            this.readmeSwitch.Name = "readmeSwitch";
            this.readmeSwitch.ShowState = true;
            this.readmeSwitch.TabIndex = 0;
            //
            // statusLabel
            //
            this.statusLabel.MultiLine = true;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Role = ReleaseManager.Controls.TextRole.Error;
            this.statusLabel.SizePx = 12F;
            //
            // footer
            //
            this.footer.Controls.Add(this.createButton);
            this.footer.Controls.Add(this.cancelButton);
            this.footer.Name = "footer";
            //
            // createButton
            //
            this.createButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Accent;
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(120, 32);
            this.createButton.TabIndex = 5;
            this.createButton.Text = "Create";
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            //
            // cancelButton
            //
            this.cancelButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Standard;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(120, 32);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            //
            // CreateRepositoryDialog
            //
            this.ClientSize = new System.Drawing.Size(560, 536);
            this.Controls.Add(this.content);
            this.Controls.Add(this.footer);
            this.MinimumSize = new System.Drawing.Size(520, 440);
            this.Name = "CreateRepositoryDialog";
            this.Skin = this.skin;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create a repository on GitHub";
            this.TitleBar.ShowMaximizeBox = false;
            this.TitleBar.ShowMinimizeBox = false;
            this.content.ResumeLayout(false);
            this.visibilityCard.ResumeLayout(false);
            this.readmeCard.ResumeLayout(false);
            this.footer.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private ModernWinForms.ModernSkin skin;
        private ReleaseManager.Controls.SurfacePanel content;
        private ReleaseManager.Controls.TextLabel headingLabel;
        private ReleaseManager.Controls.TextLabel accountLabel;
        private ReleaseManager.Controls.TextLabel ownerLabel;
        private ModernWinForms.ModernComboBox ownerCombo;
        private ReleaseManager.Controls.TextLabel nameLabel;
        private ModernWinForms.ModernTextBox nameBox;
        private ReleaseManager.Controls.TextLabel urlLabel;
        private ReleaseManager.Controls.TextLabel descriptionLabel;
        private ReleaseManager.Controls.TextLabel optionalLabel;
        private ModernWinForms.ModernTextBox descriptionBox;
        private ReleaseManager.Controls.SettingsCard visibilityCard;
        private ModernWinForms.ModernSegmentedControl visibilitySwitch;
        private ReleaseManager.Controls.SettingsCard readmeCard;
        private ReleaseManager.Controls.ToggleSwitchControl readmeSwitch;
        private ReleaseManager.Controls.TextLabel statusLabel;
        private ReleaseManager.Controls.FooterBand footer;
        private ReleaseManager.Controls.CommandButton createButton;
        private ReleaseManager.Controls.CommandButton cancelButton;
    }
}
