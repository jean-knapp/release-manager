namespace ReleaseManager.Views
{
    partial class ProjectView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.leftScroll = new ModernWinForms.ModernScrollableControl();
            this.releaseHeading = new ReleaseManager.Controls.TextLabel();
            this.releaseCard = new ReleaseManager.Controls.SurfacePanel();
            this.versionLabel = new ReleaseManager.Controls.TextLabel();
            this.versionBox = new ModernWinForms.ModernTextBox();
            this.versionHint = new ReleaseManager.Controls.TextLabel();
            this.repoLabel = new ReleaseManager.Controls.TextLabel();
            this.repoHint = new ReleaseManager.Controls.TextLabel();
            this.repoBox = new ModernWinForms.ModernTextBox();
            this.chooseRepoButton = new ReleaseManager.Controls.CommandButton();
            this.packIdLabel = new ReleaseManager.Controls.TextLabel();
            this.packIdBox = new ModernWinForms.ModernTextBox();
            this.outputLabel = new ReleaseManager.Controls.TextLabel();
            this.outputBox = new ModernWinForms.ModernTextBox();
            this.browseButton = new ReleaseManager.Controls.CommandButton();
            this.obfuscationHeading = new ReleaseManager.Controls.TextLabel();
            this.obfuscateCard = new ReleaseManager.Controls.SettingsCard();
            this.obfuscateToggle = new ReleaseManager.Controls.ToggleSwitchControl();
            this.protectionsCard = new ReleaseManager.Controls.SurfacePanel();
            this.protectionsLabel = new ReleaseManager.Controls.TextLabel();
            this.updatesHeading = new ReleaseManager.Controls.TextLabel();
            this.updatesCard = new ReleaseManager.Controls.SettingsCard();
            this.updatesToggle = new ReleaseManager.Controls.ToggleSwitchControl();
            this.signInCard = new ReleaseManager.Controls.SettingsCard();
            this.signInToggle = new ReleaseManager.Controls.ToggleSwitchControl();
            this.repositoryHeading = new ReleaseManager.Controls.TextLabel();
            this.licenseCard = new ReleaseManager.Controls.SettingsCard();
            this.licenseCombo = new ModernWinForms.ModernComboBox();
            this.readmeCard = new ReleaseManager.Controls.SettingsCard();
            this.readmeButton = new ReleaseManager.Controls.CommandButton();
            this.rightPanel = new ReleaseManager.Controls.SurfacePanel();
            this.titleLabel = new ReleaseManager.Controls.TextLabel();
            this.metaLabel = new ReleaseManager.Controls.TextLabel();
            this.openBuildButton = new ReleaseManager.Controls.CommandButton();
            this.openReleasesButton = new ReleaseManager.Controls.CommandButton();
            this.headerDivider = new ReleaseManager.Controls.SurfacePanel();
            this.runAllButton = new ReleaseManager.Controls.CommandButton();
            this.warnBar = new ModernWinForms.ModernInfoBar();
            this.errorBar = new ModernWinForms.ModernInfoBar();
            this.stepsHeading = new ReleaseManager.Controls.TextLabel();
            this.stepsSummary = new ReleaseManager.Controls.TextLabel();
            this.stepList = new ReleaseManager.Controls.StepList();
            this.logHeading = new ReleaseManager.Controls.TextLabel();
            this.copyLogButton = new ReleaseManager.Controls.CommandButton();
            this.clearLogButton = new ReleaseManager.Controls.CommandButton();
            this.logCard = new ReleaseManager.Controls.SurfacePanel();
            this.logView = new ReleaseManager.Controls.LogView();
            this.leftScroll.SuspendLayout();
            this.releaseCard.SuspendLayout();
            this.obfuscateCard.SuspendLayout();
            this.updatesCard.SuspendLayout();
            this.signInCard.SuspendLayout();
            this.licenseCard.SuspendLayout();
            this.readmeCard.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.logCard.SuspendLayout();
            this.SuspendLayout();
            //
            // leftScroll
            //
            this.leftScroll.Content.Controls.Add(this.releaseHeading);
            this.leftScroll.Content.Controls.Add(this.releaseCard);
            this.leftScroll.Content.Controls.Add(this.obfuscationHeading);
            this.leftScroll.Content.Controls.Add(this.obfuscateCard);
            this.leftScroll.Content.Controls.Add(this.protectionsCard);
            this.leftScroll.Content.Controls.Add(this.updatesHeading);
            this.leftScroll.Content.Controls.Add(this.updatesCard);
            this.leftScroll.Content.Controls.Add(this.signInCard);
            this.leftScroll.Content.Controls.Add(this.repositoryHeading);
            this.leftScroll.Content.Controls.Add(this.licenseCard);
            this.leftScroll.Content.Controls.Add(this.readmeCard);
            this.leftScroll.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftScroll.Name = "leftScroll";
            this.leftScroll.Size = new System.Drawing.Size(400, 760);
            this.leftScroll.TabIndex = 0;
            //
            // releaseHeading
            //
            this.releaseHeading.Name = "releaseHeading";
            this.releaseHeading.Semibold = true;
            this.releaseHeading.SizePx = 14F;
            this.releaseHeading.Size = new System.Drawing.Size(300, 20);
            this.releaseHeading.Text = "Release";
            //
            // releaseCard
            //
            this.releaseCard.Controls.Add(this.versionLabel);
            this.releaseCard.Controls.Add(this.versionBox);
            this.releaseCard.Controls.Add(this.versionHint);
            this.releaseCard.Controls.Add(this.repoLabel);
            this.releaseCard.Controls.Add(this.repoBox);
            this.releaseCard.Controls.Add(this.repoHint);
            this.releaseCard.Controls.Add(this.chooseRepoButton);
            this.releaseCard.Controls.Add(this.packIdLabel);
            this.releaseCard.Controls.Add(this.packIdBox);
            this.releaseCard.Controls.Add(this.outputLabel);
            this.releaseCard.Controls.Add(this.outputBox);
            this.releaseCard.Controls.Add(this.browseButton);
            this.releaseCard.CornerRadius = 6;
            this.releaseCard.Name = "releaseCard";
            this.releaseCard.Surface = ReleaseManager.Controls.SurfaceKind.Card;
            this.releaseCard.TabIndex = 0;
            //
            // versionLabel
            //
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.SizePx = 14F;
            this.versionLabel.Size = new System.Drawing.Size(200, 20);
            this.versionLabel.Text = "Version";
            //
            // versionBox
            //
            this.versionBox.Name = "versionBox";
            this.versionBox.PlaceholderText = "1.0.0";
            this.versionBox.Size = new System.Drawing.Size(140, 32);
            this.versionBox.TabIndex = 0;
            //
            // versionHint
            //
            this.versionHint.Name = "versionHint";
            this.versionHint.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.versionHint.SizePx = 12F;
            this.versionHint.Size = new System.Drawing.Size(300, 16);
            this.versionHint.Text = "Three numbers, like 1.4.2.";
            //
            // repoLabel
            //
            this.repoLabel.Name = "repoLabel";
            this.repoLabel.SizePx = 14F;
            this.repoLabel.Size = new System.Drawing.Size(200, 20);
            this.repoLabel.Text = "Release repository";
            //
            // repoHint
            //
            this.repoHint.Name = "repoHint";
            this.repoHint.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.repoHint.SizePx = 12F;
            this.repoHint.Size = new System.Drawing.Size(300, 16);
            this.repoHint.Text = "Where releases are published and the installed program looks for updates.";
            //
            // repoBox
            //
            this.repoBox.Name = "repoBox";
            this.repoBox.PlaceholderText = "https://github.com/you/repo";
            this.repoBox.Size = new System.Drawing.Size(240, 32);
            this.repoBox.TabIndex = 1;
            //
            // chooseRepoButton
            //
            this.chooseRepoButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Standard;
            this.chooseRepoButton.Name = "chooseRepoButton";
            this.chooseRepoButton.Size = new System.Drawing.Size(90, 32);
            this.chooseRepoButton.TabIndex = 2;
            this.chooseRepoButton.Text = "Choose…";
            this.chooseRepoButton.Click += new System.EventHandler(this.chooseRepoButton_Click);
            //
            // packIdLabel
            //
            this.packIdLabel.Name = "packIdLabel";
            this.packIdLabel.SizePx = 14F;
            this.packIdLabel.Size = new System.Drawing.Size(200, 20);
            this.packIdLabel.Text = "Package id";
            //
            // packIdBox
            //
            this.packIdBox.Name = "packIdBox";
            this.packIdBox.Size = new System.Drawing.Size(320, 32);
            this.packIdBox.TabIndex = 3;
            //
            // outputLabel
            //
            this.outputLabel.Name = "outputLabel";
            this.outputLabel.SizePx = 14F;
            this.outputLabel.Size = new System.Drawing.Size(200, 20);
            this.outputLabel.Text = "Output folder";
            //
            // outputBox
            //
            this.outputBox.Name = "outputBox";
            this.outputBox.PlaceholderText = "Releases (next to the solution)";
            this.outputBox.Size = new System.Drawing.Size(240, 32);
            this.outputBox.TabIndex = 4;
            //
            // browseButton
            //
            this.browseButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Standard;
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(90, 32);
            this.browseButton.TabIndex = 5;
            this.browseButton.Text = "Browse…";
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            //
            // obfuscationHeading
            //
            this.obfuscationHeading.Name = "obfuscationHeading";
            this.obfuscationHeading.Semibold = true;
            this.obfuscationHeading.SizePx = 14F;
            this.obfuscationHeading.Size = new System.Drawing.Size(300, 20);
            this.obfuscationHeading.Text = "Obfuscation";
            //
            // obfuscateCard
            //
            this.obfuscateCard.Controls.Add(this.obfuscateToggle);
            this.obfuscateCard.Header = "Obfuscate the build";
            this.obfuscateCard.Name = "obfuscateCard";
            this.obfuscateCard.TabIndex = 1;
            //
            // obfuscateToggle
            //
            this.obfuscateToggle.Name = "obfuscateToggle";
            this.obfuscateToggle.ShowState = true;
            this.obfuscateToggle.TabIndex = 0;
            //
            // protectionsCard
            //
            this.protectionsCard.Controls.Add(this.protectionsLabel);
            this.protectionsCard.CornerRadius = 6;
            this.protectionsCard.Name = "protectionsCard";
            this.protectionsCard.Surface = ReleaseManager.Controls.SurfaceKind.Card;
            this.protectionsCard.TabIndex = 2;
            //
            // protectionsLabel
            //
            this.protectionsLabel.Name = "protectionsLabel";
            this.protectionsLabel.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.protectionsLabel.SizePx = 12F;
            this.protectionsLabel.Size = new System.Drawing.Size(200, 16);
            this.protectionsLabel.Text = "Protections";
            //
            // updatesHeading
            //
            this.updatesHeading.Name = "updatesHeading";
            this.updatesHeading.Semibold = true;
            this.updatesHeading.SizePx = 14F;
            this.updatesHeading.Size = new System.Drawing.Size(300, 20);
            this.updatesHeading.Text = "Auto-update";
            //
            // updatesCard
            //
            this.updatesCard.Controls.Add(this.updatesToggle);
            this.updatesCard.Header = "Update from GitHub releases";
            this.updatesCard.Name = "updatesCard";
            this.updatesCard.TabIndex = 3;
            //
            // updatesToggle
            //
            this.updatesToggle.Name = "updatesToggle";
            this.updatesToggle.ShowState = true;
            this.updatesToggle.TabIndex = 0;
            //
            // signInCard
            //
            this.signInCard.Controls.Add(this.signInToggle);
            this.signInCard.Description = "Uses the GitHub account on the user's PC to read releases.";
            this.signInCard.Header = "Sign in for private repositories";
            this.signInCard.Name = "signInCard";
            this.signInCard.TabIndex = 4;
            //
            // signInToggle
            //
            this.signInToggle.Name = "signInToggle";
            this.signInToggle.ShowState = true;
            this.signInToggle.TabIndex = 0;
            //
            // repositoryHeading
            //
            this.repositoryHeading.Name = "repositoryHeading";
            this.repositoryHeading.Semibold = true;
            this.repositoryHeading.SizePx = 14F;
            this.repositoryHeading.Size = new System.Drawing.Size(300, 20);
            this.repositoryHeading.Text = "Repository";
            //
            // licenseCard
            //
            this.licenseCard.Controls.Add(this.licenseCombo);
            this.licenseCard.Header = "License";
            this.licenseCard.Name = "licenseCard";
            this.licenseCard.TabIndex = 5;
            //
            // licenseCombo
            //
            this.licenseCombo.Name = "licenseCombo";
            this.licenseCombo.Size = new System.Drawing.Size(136, 32);
            this.licenseCombo.TabIndex = 0;
            //
            // readmeCard
            //
            this.readmeCard.Controls.Add(this.readmeButton);
            this.readmeCard.Header = "README description";
            this.readmeCard.Name = "readmeCard";
            this.readmeCard.TabIndex = 6;
            //
            // readmeButton
            //
            this.readmeButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Standard;
            this.readmeButton.Name = "readmeButton";
            this.readmeButton.Size = new System.Drawing.Size(80, 32);
            this.readmeButton.TabIndex = 0;
            this.readmeButton.Text = "Edit…";
            this.readmeButton.Click += new System.EventHandler(this.readmeButton_Click);
            //
            // rightPanel
            //
            this.rightPanel.Controls.Add(this.titleLabel);
            this.rightPanel.Controls.Add(this.metaLabel);
            this.rightPanel.Controls.Add(this.openBuildButton);
            this.rightPanel.Controls.Add(this.openReleasesButton);
            this.rightPanel.Controls.Add(this.headerDivider);
            this.rightPanel.Controls.Add(this.runAllButton);
            this.rightPanel.Controls.Add(this.warnBar);
            this.rightPanel.Controls.Add(this.errorBar);
            this.rightPanel.Controls.Add(this.stepsHeading);
            this.rightPanel.Controls.Add(this.stepsSummary);
            this.rightPanel.Controls.Add(this.stepList);
            this.rightPanel.Controls.Add(this.logHeading);
            this.rightPanel.Controls.Add(this.copyLogButton);
            this.rightPanel.Controls.Add(this.clearLogButton);
            this.rightPanel.Controls.Add(this.logCard);
            this.rightPanel.CornerRadius = 0;
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Surface = ReleaseManager.Controls.SurfaceKind.Base;
            this.rightPanel.TabIndex = 1;
            //
            // titleLabel
            //
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Semibold = true;
            this.titleLabel.SizePx = 28F;
            this.titleLabel.Size = new System.Drawing.Size(400, 36);
            this.titleLabel.Text = "Release";
            //
            // metaLabel
            //
            this.metaLabel.Name = "metaLabel";
            this.metaLabel.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.metaLabel.SizePx = 12F;
            this.metaLabel.Size = new System.Drawing.Size(400, 16);
            //
            // openBuildButton
            //
            this.openBuildButton.Name = "openBuildButton";
            this.openBuildButton.Size = new System.Drawing.Size(120, 32);
            this.openBuildButton.TabIndex = 0;
            this.openBuildButton.Text = "Build folder";
            this.openBuildButton.Click += new System.EventHandler(this.openBuildButton_Click);
            //
            // openReleasesButton
            //
            this.openReleasesButton.Name = "openReleasesButton";
            this.openReleasesButton.Size = new System.Drawing.Size(150, 32);
            this.openReleasesButton.TabIndex = 1;
            this.openReleasesButton.Text = "GitHub releases";
            this.openReleasesButton.Click += new System.EventHandler(this.openReleasesButton_Click);
            //
            // headerDivider
            //
            this.headerDivider.CornerRadius = 0;
            this.headerDivider.Name = "headerDivider";
            this.headerDivider.Size = new System.Drawing.Size(1, 20);
            this.headerDivider.Surface = ReleaseManager.Controls.SurfaceKind.Custom;
            //
            // runAllButton
            //
            this.runAllButton.Appearance = ReleaseManager.Controls.ButtonAppearance.Accent;
            this.runAllButton.Name = "runAllButton";
            this.runAllButton.Size = new System.Drawing.Size(140, 32);
            this.runAllButton.TabIndex = 2;
            this.runAllButton.Text = "Run all steps";
            this.runAllButton.Click += new System.EventHandler(this.runAllButton_Click);
            //
            // warnBar
            //
            this.warnBar.ActionText = "Set up signing…";
            this.warnBar.IsClosable = true;
            this.warnBar.Name = "warnBar";
            this.warnBar.Severity = ModernWinForms.ModernInfoBarSeverity.Warning;
            this.warnBar.Title = "The installer is not signed.";
            this.warnBar.Visible = false;
            this.warnBar.ActionClick += new System.EventHandler(this.warnBar_ActionClick);
            this.warnBar.Closed += new System.EventHandler(this.warnBar_Closed);
            //
            // errorBar
            //
            this.errorBar.Name = "errorBar";
            this.errorBar.Severity = ModernWinForms.ModernInfoBarSeverity.Error;
            this.errorBar.Visible = false;
            this.errorBar.ActionClick += new System.EventHandler(this.errorBar_ActionClick);
            //
            // stepsHeading
            //
            this.stepsHeading.Name = "stepsHeading";
            this.stepsHeading.Semibold = true;
            this.stepsHeading.SizePx = 14F;
            this.stepsHeading.Size = new System.Drawing.Size(60, 20);
            this.stepsHeading.Text = "Steps";
            //
            // stepsSummary
            //
            this.stepsSummary.Name = "stepsSummary";
            this.stepsSummary.Role = ReleaseManager.Controls.TextRole.Secondary;
            this.stepsSummary.SizePx = 12F;
            this.stepsSummary.Size = new System.Drawing.Size(300, 20);
            //
            // stepList
            //
            this.stepList.Name = "stepList";
            this.stepList.TabIndex = 3;
            //
            // logHeading
            //
            this.logHeading.Name = "logHeading";
            this.logHeading.Semibold = true;
            this.logHeading.SizePx = 14F;
            this.logHeading.Size = new System.Drawing.Size(200, 20);
            this.logHeading.Text = "Log";
            //
            // copyLogButton
            //
            this.copyLogButton.Name = "copyLogButton";
            this.copyLogButton.Size = new System.Drawing.Size(90, 32);
            this.copyLogButton.TabIndex = 4;
            this.copyLogButton.Text = "Copy";
            this.copyLogButton.Click += new System.EventHandler(this.copyLogButton_Click);
            //
            // clearLogButton
            //
            this.clearLogButton.Name = "clearLogButton";
            this.clearLogButton.Size = new System.Drawing.Size(90, 32);
            this.clearLogButton.TabIndex = 5;
            this.clearLogButton.Text = "Clear";
            this.clearLogButton.Click += new System.EventHandler(this.clearLogButton_Click);
            //
            // logCard
            //
            this.logCard.Controls.Add(this.logView);
            this.logCard.CornerRadius = 6;
            this.logCard.Name = "logCard";
            this.logCard.Padding = new System.Windows.Forms.Padding(1, 8, 1, 8);
            this.logCard.Surface = ReleaseManager.Controls.SurfaceKind.Custom;
            this.logCard.TabIndex = 6;
            //
            // logView
            //
            this.logView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logView.Name = "logView";
            this.logView.TabIndex = 0;
            //
            // ProjectView
            //
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.leftScroll);
            this.Name = "ProjectView";
            this.Size = new System.Drawing.Size(1440, 800);
            this.leftScroll.ResumeLayout(false);
            this.releaseCard.ResumeLayout(false);
            this.obfuscateCard.ResumeLayout(false);
            this.updatesCard.ResumeLayout(false);
            this.signInCard.ResumeLayout(false);
            this.licenseCard.ResumeLayout(false);
            this.readmeCard.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.logCard.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private ModernWinForms.ModernScrollableControl leftScroll;
        private ReleaseManager.Controls.TextLabel releaseHeading;
        private ReleaseManager.Controls.SurfacePanel releaseCard;
        private ReleaseManager.Controls.TextLabel versionLabel;
        private ModernWinForms.ModernTextBox versionBox;
        private ReleaseManager.Controls.TextLabel versionHint;
        private ReleaseManager.Controls.TextLabel repoLabel;
        private ReleaseManager.Controls.TextLabel repoHint;
        private ModernWinForms.ModernTextBox repoBox;
        private ReleaseManager.Controls.CommandButton chooseRepoButton;
        private ReleaseManager.Controls.TextLabel packIdLabel;
        private ModernWinForms.ModernTextBox packIdBox;
        private ReleaseManager.Controls.TextLabel outputLabel;
        private ModernWinForms.ModernTextBox outputBox;
        private ReleaseManager.Controls.CommandButton browseButton;
        private ReleaseManager.Controls.TextLabel obfuscationHeading;
        private ReleaseManager.Controls.SettingsCard obfuscateCard;
        private ReleaseManager.Controls.ToggleSwitchControl obfuscateToggle;
        private ReleaseManager.Controls.SurfacePanel protectionsCard;
        private ReleaseManager.Controls.TextLabel protectionsLabel;
        private ReleaseManager.Controls.TextLabel updatesHeading;
        private ReleaseManager.Controls.SettingsCard updatesCard;
        private ReleaseManager.Controls.ToggleSwitchControl updatesToggle;
        private ReleaseManager.Controls.SettingsCard signInCard;
        private ReleaseManager.Controls.ToggleSwitchControl signInToggle;
        private ReleaseManager.Controls.TextLabel repositoryHeading;
        private ReleaseManager.Controls.SettingsCard licenseCard;
        private ModernWinForms.ModernComboBox licenseCombo;
        private ReleaseManager.Controls.SettingsCard readmeCard;
        private ReleaseManager.Controls.CommandButton readmeButton;
        private ReleaseManager.Controls.SurfacePanel rightPanel;
        private ReleaseManager.Controls.TextLabel titleLabel;
        private ReleaseManager.Controls.TextLabel metaLabel;
        private ReleaseManager.Controls.CommandButton openBuildButton;
        private ReleaseManager.Controls.CommandButton openReleasesButton;
        private ReleaseManager.Controls.SurfacePanel headerDivider;
        private ReleaseManager.Controls.CommandButton runAllButton;
        private ModernWinForms.ModernInfoBar warnBar;
        private ModernWinForms.ModernInfoBar errorBar;
        private ReleaseManager.Controls.TextLabel stepsHeading;
        private ReleaseManager.Controls.TextLabel stepsSummary;
        private ReleaseManager.Controls.StepList stepList;
        private ReleaseManager.Controls.TextLabel logHeading;
        private ReleaseManager.Controls.CommandButton copyLogButton;
        private ReleaseManager.Controls.CommandButton clearLogButton;
        private ReleaseManager.Controls.SurfacePanel logCard;
        private ReleaseManager.Controls.LogView logView;
    }
}
