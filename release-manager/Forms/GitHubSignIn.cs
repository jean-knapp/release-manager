using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReleaseManager.Release;
using ReleaseManager.Services;

namespace ReleaseManager.Forms
{
    /// <summary>Who Release Manager is signed in to GitHub as, and how.</summary>
    public sealed class GitHubAccount
    {
        public GitHubAccount(string login, GitHubToken token)
        {
            Login = login;
            Token = token;
        }

        /// <summary>The account's login; null when no sign-in was found or GitHub refused it.</summary>
        public string Login { get; }

        public GitHubToken Token { get; }

        /// <summary>Why <see cref="Login"/> is null, for the account card; null when signed in.</summary>
        public string Problem { get; set; }
    }

    /// <summary>
    /// The GitHub sign-in shared by the repository picker, the Settings dialog and the publish
    /// step: finding it, describing it, and replacing it with a pasted token.
    /// </summary>
    public static class GitHubSignIn
    {
        /// <summary>Page that creates a token with the scope publishing needs pre-selected.</summary>
        private const string TokenPageUrl = "https://github.com/settings/tokens/new?scopes=repo&description=Release%20Manager";

        /// <summary>The sign-in in use and the account it belongs to.</summary>
        public static async Task<GitHubAccount> FindAsync()
        {
            GitHubToken token;
            try { token = await GitHub.FindTokenAsync(); }
            catch { token = null; }
            if (token == null) return new GitHubAccount(null, null) { Problem = "No GitHub token or git sign-in was found." };
            try
            {
                var user = await GitHubApi.GetUserAsync(token.Value);
                return new GitHubAccount(user.Login, token);
            }
            catch (GitHubException ex)
            {
                return new GitHubAccount(null, token) { Problem = ex.IsAuthenticationProblem ? "GitHub did not accept the sign-in." : ex.Message };
            }
            catch (Exception ex)
            {
                return new GitHubAccount(null, token) { Problem = "Could not reach GitHub: " + ex.Message };
            }
        }

        /// <summary>"Using git's saved credentials for github.com", for the account cards.</summary>
        public static string UsingText(GitHubAccount account) =>
            account?.Token == null ? "No token found" : "Using " + account.Token.Source;

        /// <summary>
        /// Asks for a personal access token, offering GitHub's page that makes one, checks it with
        /// GitHub and saves it. Returns the account it belongs to, or null when none was added.
        /// </summary>
        public static async Task<GitHubAccount> AddTokenAsync(IWin32Window owner)
        {
            var value = PromptForToken(owner);
            if (value == null) return null;
            try
            {
                var user = await GitHubApi.GetUserAsync(value);
                AppSettings.Current.GitHubToken = value;
                AppSettings.Current.Save();
                return new GitHubAccount(user.Login, new GitHubToken(value, "the token saved in Release Manager"));
            }
            catch (Exception ex)
            {
                Dialogs.Error(owner, "GitHub token", "GitHub did not accept the token.\n\n" + ex.Message);
                return null;
            }
        }

        /// <summary>The token the user pasted, or null when they cancelled.</summary>
        public static string PromptForToken(IWin32Window owner)
        {
            var choice = Dialogs.Show(owner, "GitHub token",
                "Use a personal access token that has the repo scope, so private repositories are listed too. "
                + "Release Manager saves it, encrypted for your Windows account, and publishes with it."
                + Environment.NewLine + Environment.NewLine +
                "GitHub can create one for you with that scope pre-selected.",
                "Open GitHub", "I have a token", "Cancel");
            if (choice == DialogResult.Cancel) return null;
            if (choice == DialogResult.OK)
            {
                try { Process.Start(TokenPageUrl); }
                catch (Exception) { }
            }

            using (var prompt = new TextInputDialog())
            {
                prompt.Caption = "GitHub token";
                prompt.Prompt = "Paste the token";
                prompt.Password = true;
                if (prompt.ShowDialog(owner) != DialogResult.OK) return null;
                var value = prompt.Value.Trim();
                return value.Length == 0 ? null : value;
            }
        }
    }
}
