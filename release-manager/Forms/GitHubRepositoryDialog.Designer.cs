namespace ReleaseManager.Forms
{
    partial class GitHubRepositoryDialog
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
            this.subheadLabel = new ReleaseManager.Controls.TextLabel();
            this.refreshButton = new ReleaseManager.Controls.CommandButton();
            this.accountCard = new ReleaseManager.Controls.SettingsCard();
            this.tokenButton = new ReleaseManager.Controls.CommandButton();
            this.filterBox = new ModernWinForms.ModernTextBox();
            this.clearFilterButton = new ModernWinForms.ModernTextBoxButton();
            this.listCard = new ReleaseManager.Controls.SurfacePanel();
            this.list = new ReleaseManager.Controls.GitHubRepositoryListControl();
            this.footer = new ReleaseManager.Controls.FooterBand();
            this.newButton = new ReleaseManager.Controls.CommandButton();
            this.chooseButton = new ReleaseManager.Controls.CommandButton();
            this.cancelButton = new ReleaseManager.Controls.CommandButton();
            this.content.SuspendLayout();
            this.accountCard.SuspendLayout();
            this.listCard.SuspendLayout();
            this.footer.SuspendLayout();
            this.SuspendLayout();
            //
            // content: padding 20 24 16, 16 px between blocks.
            //
            this.content.Controls.Add(this.headingLabel);
            this.content.Controls.Add(this.subheadLabel);
            this.content.Controls.Add(this.refreshButton);
            this.content.Controls.Add(this.accountCard);
            this.content.Controls.Add(this.filterBox);
            this.content.Controls.Add(this.listCard);
            this.content.CornerRadius = 0;
            this.content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.content.Name = "content";
            this.content.Surface = ReleaseManager.Controls.SurfaceKind.Base;
            //
            // headingLabel
            //
            this.headingLabel.Location = new System.Drawing.Point(24, 20);
            this.headingLabel.Name = "headingLabel";
            this.headingLabel.Semibold = true;
            this.headingLabel.Size = new System.Drawing.Size(400, 28);
            this.headingLabel.SizePx = 20F;
            this.headingLabel.Text = "Choose a repository";
            //
            // subheadLabel
            //
            this.subheadLabel.Location = new System.Drawing.Point(24, 52);
            this.subheadLabel.Name = "subheadLabel";
            this.subheadLabel.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.subheadLabel.Size = new System.Drawing.Size(400, 16);
            this.subheadLabel.SizePx = 12F;
            this.subheadLabel.Text = "Your repositories, newest push first";
            //
            // refreshButton
            //
            this.refreshButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Standard;
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(104, 32);
            this.refreshButton.TabIndex = 3;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            //
            // accountCard: min-height 56, padding 8 8 8 16.
            //
            this.accountCard.Controls.Add(this.tokenButton);
            this.accountCard.Header = "Looking for a GitHub sign-in…";
            this.accountCard.MinHeight = 56;
            this.accountCard.Name = "accountCard";
            this.accountCard.PaddingRight = 8;
            this.accountCard.PaddingY = 8;
            this.accountCard.TabIndex = 4;
            //
            // tokenButton
            //
            this.tokenButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Standard;
            this.tokenButton.Name = "tokenButton";
            this.tokenButton.Size = new System.Drawing.Size(180, 32);
            this.tokenButton.TabIndex = 0;
            this.tokenButton.Text = "Use a different token…";
            this.tokenButton.Click += new System.EventHandler(this.tokenButton_Click);
            //
            // filterBox
            //
            this.filterBox.Buttons.Add(this.clearFilterButton);
            this.filterBox.Name = "filterBox";
            this.filterBox.PlaceholderText = "Filter by name or description";
            this.filterBox.Size = new System.Drawing.Size(612, 32);
            this.filterBox.TabIndex = 0;
            this.filterBox.TextChanged += new System.EventHandler(this.filterBox_TextChanged);
            //
            // clearFilterButton
            //
            this.clearFilterButton.ToolTipText = "Clear the filter";
            this.clearFilterButton.Visible = false;
            this.clearFilterButton.Click += new System.EventHandler(this.clearFilterButton_Click);
            //
            // listCard: --tv-bg, 1 px --pn-border, radius 6, padding 4.
            //
            this.listCard.Controls.Add(this.list);
            this.listCard.CornerRadius = 6;
            this.listCard.Name = "listCard";
            this.listCard.Padding = new System.Windows.Forms.Padding(1, 4, 1, 4);
            this.listCard.Surface = ReleaseManager.Controls.SurfaceKind.Custom;
            //
            // list
            //
            this.list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list.Name = "list";
            this.list.TabIndex = 1;
            //
            // footer
            //
            this.footer.Controls.Add(this.newButton);
            this.footer.Controls.Add(this.chooseButton);
            this.footer.Controls.Add(this.cancelButton);
            this.footer.Name = "footer";
            //
            // newButton
            //
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(160, 32);
            this.newButton.TabIndex = 5;
            this.newButton.Text = "New repository…";
            this.newButton.Click += new System.EventHandler(this.newButton_Click);
            //
            // chooseButton
            //
            this.chooseButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Accent;
            this.chooseButton.Name = "chooseButton";
            this.chooseButton.Size = new System.Drawing.Size(120, 32);
            this.chooseButton.TabIndex = 2;
            this.chooseButton.Text = "Choose";
            this.chooseButton.Click += new System.EventHandler(this.chooseButton_Click);
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
            // GitHubRepositoryDialog
            //
            this.ClientSize = new System.Drawing.Size(660, 620);
            this.Controls.Add(this.content);
            this.Controls.Add(this.footer);
            this.MinimumSize = new System.Drawing.Size(560, 480);
            this.Name = "GitHubRepositoryDialog";
            this.Skin = this.skin;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose a repository";
            this.TitleBar.ShowMaximizeBox = false;
            this.TitleBar.ShowMinimizeBox = false;
            this.content.ResumeLayout(false);
            this.accountCard.ResumeLayout(false);
            this.listCard.ResumeLayout(false);
            this.footer.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private ModernWinForms.ModernSkin skin;
        private ReleaseManager.Controls.SurfacePanel content;
        private ReleaseManager.Controls.TextLabel headingLabel;
        private ReleaseManager.Controls.TextLabel subheadLabel;
        private ReleaseManager.Controls.CommandButton refreshButton;
        private ReleaseManager.Controls.SettingsCard accountCard;
        private ReleaseManager.Controls.CommandButton tokenButton;
        private ModernWinForms.ModernTextBox filterBox;
        private ModernWinForms.ModernTextBoxButton clearFilterButton;
        private ReleaseManager.Controls.SurfacePanel listCard;
        private ReleaseManager.Controls.GitHubRepositoryListControl list;
        private ReleaseManager.Controls.FooterBand footer;
        private ReleaseManager.Controls.CommandButton newButton;
        private ReleaseManager.Controls.CommandButton chooseButton;
        private ReleaseManager.Controls.CommandButton cancelButton;
    }
}
