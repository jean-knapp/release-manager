using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReleaseManager.Forms;
using ReleaseManager.Release;

namespace ReleaseManager.Services
{
    /// <summary>
    /// The Claude Code CLI refused to work because it is not signed in, or its sign-in expired.
    /// <see cref="Exception.Message"/> is what the CLI said.
    /// </summary>
    public sealed class ClaudeSignInRequiredException : InvalidOperationException
    {
        public ClaudeSignInRequiredException(string cliMessage) : base(cliMessage) { }
    }

    /// <summary>
    /// Runs the Claude Code CLI (<c>claude -p</c>) the way the Git Client does: found on PATH or in
    /// its default install folders, never interactive, and signed in through the CLI's own
    /// <c>claude auth login</c>, so Release Manager never sees the account's credentials.
    /// </summary>
    public static class ClaudeCode
    {
        // What the CLI prints when it has no usable sign-in: never signed in, an expired OAuth
        // session, or a bad API key.
        private static readonly Regex SignInProblem = new Regex(
            @"not (logged|signed) in|log ?in again|please run /login|run `?claude (auth )?login|failed to authenticate|" +
            @"oauth (session|token)|session expired|invalid api key|authentication_error|unauthori[sz]ed",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>claude on PATH, then the native installer's and npm's default locations; null when missing.</summary>
        public static string FindExecutable()
        {
            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(dir)) continue;
                foreach (var name in new[] { "claude.exe", "claude.cmd" })
                {
                    try
                    {
                        var candidate = Path.Combine(dir.Trim().Trim('"'), name);
                        if (File.Exists(candidate)) return candidate;
                    }
                    catch (ArgumentException)
                    {
                        // A malformed PATH entry.
                    }
                }
            }
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            foreach (var candidate in new[] { Path.Combine(profile, ".local", "bin", "claude.exe"), Path.Combine(appData, "npm", "claude.cmd") })
            {
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }

        public static bool LooksLikeSignInProblem(string cliOutput) =>
            !string.IsNullOrEmpty(cliOutput) && SignInProblem.IsMatch(cliOutput);

        /// <summary>
        /// Asks Claude Code to read the project and describe the program for its README: a short
        /// paragraph and a list of features, in Markdown.
        /// </summary>
        public static async Task<string> DescribeProgramAsync(string executable, CsProject project, string workingDirectory, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(executable) || !File.Exists(executable))
                throw new InvalidOperationException("The Claude Code CLI was not found. Install it from https://claude.com/claude-code, then try again.");

            var prompt = new StringBuilder();
            prompt.Append("Read the source of the C# program \"").Append(project.AssemblyName).Append("\" (project file: ")
                .Append(project.Path).Append(") and write the description section of its README on GitHub: what the program is and what it does, ")
                .Append("for someone deciding whether to download it. ");
            prompt.Append("Format: GitHub Markdown. Start with one short paragraph of two to four sentences. Then a \"## Features\" heading and four to eight bullet points of one short sentence each. ");
            prompt.Append("Write plainly and factually, in sentence case, with no emoji and no marketing superlatives. ");
            prompt.Append("Leave out a title, installation or download instructions, badges, the license and build instructions: those are added separately. ");
            prompt.Append("Output only the Markdown: no preamble, no explanation, no code fences.");

            var args = new List<string>
            {
                "-p", prompt.ToString(),
                "--output-format", "text",
                // Reading the source is all it may do.
                "--tools", "Read,Glob,Grep",
                "--allowedTools", "Read,Glob,Grep",
                "--no-session-persistence",
            };
            var output = await RunAsync(executable, args, workingDirectory, string.Empty, cancellation).ConfigureAwait(false);
            var text = StripFence(output);
            if (text.Length == 0) throw new InvalidOperationException("Claude returned an empty description.");
            return text;
        }

        private static string StripFence(string output)
        {
            var text = (output ?? string.Empty).Replace("\r\n", "\n").Trim();
            if (text.StartsWith("```", StringComparison.Ordinal))
            {
                var firstBreak = text.IndexOf('\n');
                text = firstBreak >= 0 ? text.Substring(firstBreak + 1) : string.Empty;
                var fence = text.LastIndexOf("```", StringComparison.Ordinal);
                if (fence >= 0) text = text.Substring(0, fence);
            }
            return text.Trim();
        }

        private static async Task<string> RunAsync(string executable, IList<string> args, string workingDirectory, string stdin, CancellationToken cancellation)
        {
            var result = await RunRawAsync(executable, args, workingDirectory, stdin, cancellation).ConfigureAwait(false);
            if (result.ExitCode != 0)
            {
                var error = result.Error.Trim();
                if (error.Length == 0) error = result.Output.Trim();
                if (error.Length == 0) error = "claude exited with code " + result.ExitCode;
                if (LooksLikeSignInProblem(result.Error + "\n" + result.Output)) throw new ClaudeSignInRequiredException(error);
                throw new InvalidOperationException(error);
            }
            return result.Output;
        }

        /// <summary>Runs the CLI and returns its exit code and both streams, whatever the exit code.</summary>
        private static async Task<(int ExitCode, string Output, string Error)> RunRawAsync(string executable, IList<string> args, string workingDirectory, string stdin, CancellationToken cancellation)
        {
            var psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory,
                StandardOutputEncoding = new UTF8Encoding(false),
                StandardErrorEncoding = new UTF8Encoding(false),
            };
            var arguments = ProcessRunner.JoinArguments(args);
            if (executable.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) || executable.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
            {
                psi.FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";
                psi.Arguments = "/d /c " + ProcessRunner.Quote(executable) + " " + arguments;
            }
            else
            {
                psi.FileName = executable;
                psi.Arguments = arguments;
            }
            // Make sure the CLI never tries to open an interactive session.
            psi.EnvironmentVariables["CI"] = "1";

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var exit = new TaskCompletionSource<bool>();
            process.Exited += (s, e) => exit.TrySetResult(true);
            process.Start();
            using (process)
            using (cancellation.Register(() => { try { if (!process.HasExited) process.Kill(); } catch { } }))
            {
                var stdinTask = Task.Run(async () =>
                {
                    try
                    {
                        using (var writer = new StreamWriter(process.StandardInput.BaseStream, new UTF8Encoding(false)))
                        {
                            await writer.WriteAsync(stdin ?? string.Empty).ConfigureAwait(false);
                        }
                    }
                    catch { }
                });
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                await Task.WhenAll(exit.Task, stdout, stderr, stdinTask).ConfigureAwait(false);
                process.WaitForExit();
                cancellation.ThrowIfCancellationRequested();
                return (process.ExitCode, stdout.Result, stderr.Result);
            }
        }

        /// <summary>
        /// Explains that Claude Code needs signing in and offers to open a terminal running
        /// <c>claude auth login</c>, which sends the user to the browser.
        /// </summary>
        public static void OfferSignIn(IWin32Window owner, string executable, string cliMessage)
        {
            var message =
                "Claude Code is not signed in on this computer, or its sign-in has expired." +
                "\n\nSign in opens a terminal running  claude auth login , which opens your browser to sign in to your Anthropic " +
                "account. When the terminal says you are signed in, close it and try again." +
                (string.IsNullOrWhiteSpace(cliMessage) ? string.Empty : "\n\nClaude Code said: " + cliMessage.Trim());
            if (Dialogs.Show(owner, "Sign in to Claude Code", message, "Sign in…", null, "Cancel") != DialogResult.OK) return;
            try
            {
                var shell = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";
                var command = "title Sign in to Claude Code & \"" + executable + "\" auth login & echo. & echo You can close this window now. & pause >nul";
                Process.Start(new ProcessStartInfo(shell, "/d /c \"" + command + "\"")
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                })?.Dispose();
            }
            catch (Exception ex)
            {
                Dialogs.Error(owner, "Sign in to Claude Code", "Could not open the sign-in window.\n\n" + ex.Message + "\n\nRun  claude auth login  in a terminal instead.");
            }
        }
    }
}
