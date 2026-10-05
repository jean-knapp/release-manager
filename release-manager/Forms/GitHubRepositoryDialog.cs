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
    /// Picks a repository from the GitHub account Release Manager publishes with: its own, the ones
    /// it collaborates on and the ones its organisations hold, newest push first - or creates a new
    /// one to publish to.
    /// </summary>
    public partial class GitHubRepositoryDialog : ModernForm
    {
        private readonly List<GitHubRepository> _all = new List<GitHubRepository>();
        private GitHubToken _token;
        private bool _working;

        public GitHubRepositoryDialog()
        {
            InitializeComponent();
            Theme.Apply(skin);
            refreshButton.IconSvg = Icons.Refresh;
            newButton.IconSvg = Icons.Plus;
            accountCard.IconSvg = Icons.Github;
            clearFilterButton.SvgIcon = Icons.Cross;
            AppIcon.Apply(this);
            list.RowDoubleClick += list_RowDoubleClick;
            list.SelectionChanged += (s, e) => UpdateButtons();
            ApplyTheme();
            Theme.Changed += OnThemeChanged;
        }

        /// <summary>The repository the field holds now, selected when the list arrives.</summary>
        public string CurrentRepositoryUrl { get; set; }

        /// <summary>Name and description offered when the user creates a new repository.</summary>
        public string SuggestedName { get; set; }
        public string SuggestedDescription { get; set; }

        /// <summary>The repository the user chose or created, once the dialog returns OK.</summary>
        public GitHubRepository SelectedRepository { get; private set; }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            filterBox.Font = Fonts.Ui(14f);
            LayoutDialog();
            filterBox.Focus();
            await SignInAndLoadAsync(null);
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            Theme.Apply(skin);
            ApplyTheme();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Theme.Changed -= OnThemeChanged;
            base.OnFormClosed(e);
        }

        private void ApplyTheme()
        {
            // The search glyph takes the box's ForeColor, and is cached at the colour it was first
            // drawn in, so it is set again after each theme change. Typed text keeps the skin's --fg.
            filterBox.ForeColor = Theme.Palette.Foreground2;
            filterBox.LeadingSvgIcon = null;
            filterBox.LeadingSvgIcon = Icons.Search;
            filterBox.PlaceholderWhileFocused = true;
            listCard.CustomFill = Theme.Palette.ListBackground;
            listCard.CustomBorder = Theme.Palette.CardBorder;
            listCard.Invalidate();
        }

        // ------------------------------------------------------------------ sign-in and loading

        /// <summary>Lists the repositories of the account git (or the saved token) signs in as, or of a token just pasted.</summary>
        private async Task SignInAndLoadAsync(GitHubAccount pasted)
        {
            SetWorking(true);
            list.EmptyText = "Reading your repositories…";
            list.SetEntries(null);
            try
            {
                var account = pasted ?? await GitHubSignIn.FindAsync();
                _token = account.Login != null ? account.Token : null;
                ShowAccount(account);
                if (_token == null)
                {
                    list.EmptyText = account.Token == null
                        ? "Sign in to see your repositories."
                        : "GitHub did not accept the sign-in. Use a token with the repo scope.";
                    list.Invalidate();
                    return;
                }

                var repositories = await GitHubApi.GetRepositoriesAsync(_token.Value);
                _all.Clear();
                _all.AddRange(repositories.OrderByDescending(r => r.PushedUtc ?? DateTime.MinValue));
                list.EmptyText = _all.Count == 0 ? "This account has no repositories yet." : "No repository matches the filter.";
                ApplyFilter(Current());
            }
            catch (Exception ex)
            {
                list.EmptyText = ex is GitHubException gh && gh.IsAuthenticationProblem
                    ? ex.Message + " Use a token with the repo scope."
                    : "Could not read your repositories: " + ex.Message;
                list.Invalidate();
            }
            finally
            {
                SetWorking(false);
            }
        }

        private void ShowAccount(GitHubAccount account)
        {
            if (account.Login != null)
            {
                accountCard.Header = "Signed in as " + account.Login;
                accountCard.Description = GitHubSignIn.UsingText(account);
                tokenButton.Text = "Use a different token…";
            }
            else
            {
                accountCard.Header = "Not signed in";
                accountCard.Description = account.Problem;
                tokenButton.Text = "Sign in…";
            }
            LayoutDialog();
        }

        /// <summary>The listed repository the field already points at, if any.</summary>
        private GitHubRepository Current()
        {
            var current = GitHub.NormalizeRepositoryUrl(CurrentRepositoryUrl);
            if (current == null) return null;
            return _all.FirstOrDefault(r => string.Equals(GitHub.NormalizeRepositoryUrl(r.HtmlUrl), current, StringComparison.OrdinalIgnoreCase));
        }

        private void SetWorking(bool working)
        {
            _working = working;
            Cursor = working ? Cursors.AppStarting : Cursors.Default;
            tokenButton.Enabled = !working;
            refreshButton.Enabled = !working;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            chooseButton.Enabled = !_working && list.SelectedEntry != null;
            chooseButton.Invalidate();
            newButton.Enabled = !_working && _token != null;
            newButton.Invalidate();
            tokenButton.Invalidate();
            refreshButton.Invalidate();
        }

        // ------------------------------------------------------------------ filtering and choosing

        private void filterBox_TextChanged(object sender, EventArgs e)
        {
            clearFilterButton.Visible = filterBox.Text.Length > 0;
            ApplyFilter(list.SelectedEntry);
        }

        private void clearFilterButton_Click(object sender, EventArgs e)
        {
            filterBox.Text = string.Empty;
            filterBox.Focus();
        }

        private void ApplyFilter(GitHubRepository keep)
        {
            var filter = (filterBox.Text ?? string.Empty).Trim();
            IEnumerable<GitHubRepository> shown = _all;
            if (filter.Length > 0)
            {
                shown = _all.Where(r =>
                    (r.FullName ?? string.Empty).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Description ?? string.Empty).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            list.SetEntries(shown.ToList(), keep);
            UpdateButtons();
        }

        private void list_RowDoubleClick(object sender, RowMouseEventArgs e) => Choose();

        private void chooseButton_Click(object sender, EventArgs e) => Choose();

        private void Choose()
        {
            var entry = list.SelectedEntry;
            if (_working || entry == null) return;
            SelectedRepository = entry;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>Creates a repository with the account the list was read with, and chooses it.</summary>
        private void newButton_Click(object sender, EventArgs e)
        {
            if (_working || _token == null) return;
            using (var dialog = new CreateRepositoryDialog(_token, SuggestedName, SuggestedDescription))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.CreatedRepository == null) return;
                SelectedRepository = dialog.CreatedRepository;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private async void refreshButton_Click(object sender, EventArgs e) => await SignInAndLoadAsync(null);

        private async void tokenButton_Click(object sender, EventArgs e)
        {
            var account = await GitHubSignIn.AddTokenAsync(this);
            if (account != null) await SignInAndLoadAsync(account);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                cancelButton_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Enter && (filterBox.ContainsFocus || list.ContainsFocus))
            {
                Choose();
                return true;
            }
            // The filter keeps the caret while the arrows walk the list.
            if ((keyData == Keys.Down || keyData == Keys.Up) && filterBox.ContainsFocus && list.Count > 0)
            {
                list.Focus();
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
            int width = content.ClientSize.Width - 48;
            refreshButton.Width = refreshButton.PreferredWidth;
            refreshButton.SetBounds(24 + width - refreshButton.Width, 20, refreshButton.Width, 32);
            headingLabel.SetBounds(24, 20, Math.Max(0, refreshButton.Left - 12 - 24), 28);
            subheadLabel.SetBounds(24, 52, Math.Max(0, refreshButton.Left - 12 - 24), 16);

            tokenButton.Width = tokenButton.PreferredWidth;
            int y = 68 + 16;
            accountCard.SetBounds(24, y, width, accountCard.MeasureHeight(width));
            y = accountCard.Bottom + 16;
            filterBox.SetBounds(24, y, width, 32);
            y += 32 + 16;
            listCard.SetBounds(24, y, width, Math.Max(80, content.ClientSize.Height - 16 - y));

            footer.Height = 80;
            newButton.Appearance = ButtonAppearance.Subtle;
            newButton.Width = newButton.PreferredWidth;
            newButton.SetBounds(24, 24, newButton.Width, 32);
            cancelButton.SetBounds(footer.ClientSize.Width - 24 - 120, 24, 120, 32);
            chooseButton.SetBounds(cancelButton.Left - 8 - 120, 24, 120, 32);
        }
    }
}
