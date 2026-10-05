using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModernWinForms;
using ReleaseManager.Controls;
using ReleaseManager.Release;
using ReleaseManager.Services;

namespace ReleaseManager.Forms
{
    /// <summary>
    /// Creates a GitHub repository to publish releases to, under the signed-in account or one of
    /// its organisations. Public with a README by default: the installed program's updater reads
    /// releases without a token, and GitHub cannot tag a release in an empty repository.
    /// </summary>
    public partial class CreateRepositoryDialog : ModernForm
    {
        private readonly GitHubToken _token;
        private readonly string _suggestedName;
        private readonly string _suggestedDescription;
        private List<GitHubOwner> _owners = new List<GitHubOwner>();
        private bool _working;

        public CreateRepositoryDialog(GitHubToken token, string suggestedName, string suggestedDescription)
        {
            InitializeComponent();
            Theme.Apply(skin);
            _token = token;
            _suggestedName = SanitizeName(suggestedName);
            _suggestedDescription = suggestedDescription;
            nameBox.LeadingText = "/";
            visibilitySwitch.SelectedIndex = 0;
            AppIcon.Apply(this);
            Theme.Changed += OnThemeChanged;
        }

        /// <summary>The repository GitHub created, once the dialog returns OK.</summary>
        public GitHubRepository CreatedRepository { get; private set; }

        private GitHubOwner Owner => ownerCombo.SelectedIndex >= 0 && ownerCombo.SelectedIndex < _owners.Count ? _owners[ownerCombo.SelectedIndex] : null;

        private bool IsPrivate => visibilitySwitch.SelectedIndex == 1;

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // The inner text boxes only exist once the form is up, so the suggestions are filled in
            // here rather than in the constructor.
            foreach (var box in new[] { nameBox, descriptionBox }) box.Font = Fonts.Ui(14f);
            ownerCombo.Font = Fonts.Ui(14f);
            visibilitySwitch.Font = Fonts.Ui(14f);
            visibilitySwitch.AutoSize = true;
            nameBox.Text = _suggestedName;
            descriptionBox.Text = _suggestedDescription ?? string.Empty;
            UpdateTexts();
            LayoutDialog();
            nameBox.Focus();
            await LoadOwnersAsync();
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (!IsDisposed) Theme.Apply(skin);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Theme.Changed -= OnThemeChanged;
            base.OnFormClosed(e);
        }

        // ------------------------------------------------------------------ owners

        private async Task LoadOwnersAsync()
        {
            SetWorking(true);
            try
            {
                _owners = await GitHubApi.GetOwnersAsync(_token.Value);
                ownerCombo.Items.Clear();
                foreach (var owner in _owners) ownerCombo.Items.Add(owner.Login);
                if (_owners.Count > 0) ownerCombo.SelectedIndex = 0;
                accountLabel.Text = "Signed in as " + _owners.FirstOrDefault()?.Login;
            }
            catch (Exception ex)
            {
                accountLabel.Text = ex is GitHubException gh && gh.IsAuthenticationProblem
                    ? "GitHub did not accept the sign-in."
                    : "Could not reach GitHub.";
                statusLabel.Text = ex.Message;
            }
            finally
            {
                SetWorking(false);
                UpdateTexts();
            }
        }

        private void ownerCombo_SelectedIndexChanged(object sender, SelectedIndexChangedEventArgs e) => UpdateTexts();

        // ------------------------------------------------------------------ form state

        private void field_TextChanged(object sender, EventArgs e) => UpdateTexts();

        private void visibilitySwitch_SelectedIndexChanged(object sender, EventArgs e) => UpdateTexts();

        /// <summary>The live URL preview, the visibility's meaning and whether Create is available.</summary>
        private void UpdateTexts()
        {
            var name = SanitizeName(nameBox.Text);
            urlLabel.Text = name.Length == 0
                ? "Type a name for the repository."
                : "github.com/" + (Owner?.Login ?? "…") + "/" + name;
            visibilityCard.Description = IsPrivate
                ? "Only you and collaborators can see it. Auto-update needs a token to read private releases."
                : "Anyone can see it and download releases.";
            createButton.Enabled = !_working && Owner != null && name.Length > 0;
            createButton.Invalidate();
            LayoutDialog();
        }

        private void SetWorking(bool working)
        {
            _working = working;
            Cursor = working ? Cursors.AppStarting : Cursors.Default;
            UpdateTexts();
        }

        /// <summary>GitHub only accepts letters, digits, dots, hyphens and underscores.</summary>
        private static string SanitizeName(string name)
        {
            var clean = new System.Text.StringBuilder();
            foreach (var c in (name ?? string.Empty).Trim())
            {
                clean.Append(c < 128 && (char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_') ? c : '-');
            }
            return clean.ToString().Trim('-');
        }

        // ------------------------------------------------------------------ create

        private async void createButton_Click(object sender, EventArgs e)
        {
            if (!createButton.Enabled) return;
            var name = SanitizeName(nameBox.Text);
            if (name.Length == 0) return;
            nameBox.Text = name;

            SetWorking(true);
            statusLabel.Role = TextRole.Secondary;
            statusLabel.Text = "Creating " + Owner.Login + "/" + name + " on GitHub…";
            try
            {
                CreatedRepository = await GitHubApi.CreateRepositoryAsync(
                    _token.Value, Owner, name, descriptionBox.Text, IsPrivate, readmeSwitch.Checked);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                SetWorking(false);
                statusLabel.Role = TextRole.Error;
                statusLabel.Text = ex is GitHubException gh && gh.IsAuthenticationProblem
                    ? ex.Message + " Creating repositories needs a token with the repo scope."
                    : ex.Message;
                LayoutDialog();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && !_working)
            {
                cancelButton_Click(cancelButton, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Enter && createButton.Enabled)
            {
                createButton_Click(createButton, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ------------------------------------------------------------------ layout

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (content != null) LayoutDialog();
        }

        private void LayoutDialog()
        {
            int x = 24, width = content.ClientSize.Width - 48;
            headingLabel.SetBounds(x, 20, width, 28);
            accountLabel.SetBounds(x, 52, width, 16);
            int y = 68 + 16;

            // Owner | name, 180 px and the rest, 8 apart; labels 6 px above their fields.
            int nameLeft = x + 180 + 8;
            ownerLabel.SetBounds(x, y, 180, 20);
            nameLabel.SetBounds(nameLeft, y, width - 188, 20);
            y += 26;
            ownerCombo.SetBounds(x, y, 180, 32);
            nameBox.SetBounds(nameLeft, y, x + width - nameLeft, 32);
            y += 32 + 8;
            urlLabel.SetBounds(x, y, width, 16);
            y += 16 + 16;

            descriptionLabel.Width = descriptionLabel.PreferredWidth + 2;
            descriptionLabel.SetBounds(x, y, descriptionLabel.Width, 20);
            optionalLabel.SetBounds(descriptionLabel.Right + 2, y, 120, 20);
            y += 26;
            descriptionBox.SetBounds(x, y, width, 32);
            y += 32 + 16;

            visibilityCard.SetBounds(x, y, width, visibilityCard.MeasureHeight(width));
            y = visibilityCard.Bottom + 2;
            readmeCard.SetBounds(x, y, width, readmeCard.MeasureHeight(width));
            y = readmeCard.Bottom + 8;
            statusLabel.SetBounds(x, y, width, Math.Max(0, content.ClientSize.Height - y - 4));

            footer.Height = 80;
            cancelButton.SetBounds(footer.ClientSize.Width - 24 - 120, 24, 120, 32);
            createButton.SetBounds(cancelButton.Left - 8 - 120, 24, 120, 32);
        }
    }
}
