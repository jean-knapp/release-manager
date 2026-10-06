using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Velopack;
using Velopack.Sources;

namespace ReleaseManager
{
    /// <summary>
    /// Keeps the installed program up to date from its GitHub releases. Added by Release Manager,
    /// which keeps <see cref="RepositoryUrl"/> in step with the repository it publishes to.
    /// </summary>
    internal static class AppUpdater
    {
        public const string RepositoryUrl = "https://github.com/jean-knapp/release-manager";

        /// <summary>
        /// Off, releases are read anonymously, which only works for a public repository. On, the
        /// GitHub sign-in already on the PC (git's credential store, then the GitHub CLI) is used, so
        /// people with access to a private repository get updates too. Set in Release Manager.
        /// </summary>
        public const bool UseGitHubSignIn = false;

        /// <summary>
        /// Looks for a newer release without holding up start-up. Once one is downloaded, asks to
        /// restart into it; declined, it is installed when the program closes.
        /// </summary>
        public static void CheckInBackground()
        {
            Task.Run(async () =>
            {
                try
                {
                    var manager = new UpdateManager(new GithubSource(RepositoryUrl, null, false));
                    // Run from Visual Studio or a copied folder, the program is not installed: nothing to update.
                    if (!manager.IsInstalled) return;
                    var token = UseGitHubSignIn ? FindGitHubToken() : null;
                    if (token != null) manager = new UpdateManager(new GithubSource(RepositoryUrl, token, false));
                    var update = await manager.CheckForUpdatesAsync().ConfigureAwait(false);
                    if (update == null) return;
                    await manager.DownloadUpdatesAsync(update).ConfigureAwait(false);
                    OfferRestart(manager, update);
                }
                catch (Exception)
                {
                    // Offline, rate-limited or a release still uploading: the next start tries again.
                }
            });
        }

        private static void OfferRestart(UpdateManager manager, UpdateInfo update)
        {
            var target = update.TargetFullRelease;
            Form owner = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
            if (owner == null || owner.IsDisposed || !owner.IsHandleCreated)
            {
                manager.WaitExitThenApplyUpdates(target, true, false);
                return;
            }
            owner.BeginInvoke(new Action(() =>
            {
                var answer = MessageBox.Show(owner,
                    "Version " + target.Version + " has been downloaded.\n\nRestart now to update? Otherwise it is installed when you close the program.",
                    Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (answer == DialogResult.Yes) manager.ApplyUpdatesAndRestart(target);
                else manager.WaitExitThenApplyUpdates(target, true, false);
            }));
        }

        /// <summary>
        /// The GitHub token git's credential helper keeps for github.com (Git Credential Manager
        /// stores the sign-in there), else the GitHub CLI's; null when neither has one. Neither is
        /// allowed to open a sign-in window.
        /// </summary>
        private static string FindGitHubToken()
        {
            var stored = Run("git", "-c credential.interactive=never credential fill", "protocol=https\nhost=github.com\n\n");
            foreach (var line in (stored ?? string.Empty).Split('\n'))
            {
                if (line.StartsWith("password=", StringComparison.Ordinal)) return line.Substring(9).Trim();
            }
            var cli = Run("gh", "auth token", null);
            return string.IsNullOrWhiteSpace(cli) ? null : cli.Trim();
        }

        private static string Run(string file, string arguments, string input)
        {
            try
            {
                var start = new ProcessStartInfo(file, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                start.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
                start.EnvironmentVariables["GCM_INTERACTIVE"] = "never";
                using (var process = Process.Start(start))
                {
                    if (input != null) process.StandardInput.Write(input);
                    process.StandardInput.Close();
                    process.StandardError.ReadToEndAsync();
                    var output = process.StandardOutput.ReadToEnd();
                    if (!process.WaitForExit(10000)) return null;
                    return process.ExitCode == 0 ? output : null;
                }
            }
            catch (Exception)
            {
                // git or gh is not installed.
                return null;
            }
        }
    }
}
