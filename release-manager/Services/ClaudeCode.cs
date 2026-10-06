using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                // The reply is pasted into the README as it is: no chat around it.
                "--append-system-prompt",
                "Your final reply is inserted verbatim into a README file. It must start with the first sentence of the " +
                "description and end with the last feature bullet: no lead-in such as \"Here's the description\", " +
                "no separator lines, no closing remarks.",
                "--output-format", "text",
                // Reading the source is all it may do.
                "--tools", "Read,Glob,Grep",
                "--allowedTools", "Read,Glob,Grep",
                "--no-session-persistence",
            };
            var output = await RunAsync(executable, args, workingDirectory, string.Empty, cancellation).ConfigureAwait(false);
            var text = StripChatter(StripFence(output));
            if (text.Length == 0) throw new InvalidOperationException("Claude returned an empty description.");
            return text;
        }

        /// <summary>
        /// Has Claude Code add the update code to a project Release Manager cannot edit by itself (no
        /// plain Main: WPF's App.xaml, top-level statements, a startup it does not recognise).
        /// Claude edits the project's files directly, limited to reading and editing them; the
        /// AppUpdater.cs to add is handed over on standard input, so it is exactly the one Release
        /// Manager writes. Returns Claude's summary of what it changed.
        /// </summary>
        public static async Task<string> SetUpUpdatesAsync(string executable, CsProject project, string velopackVersion, string updaterSource, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(executable) || !File.Exists(executable))
                throw new InvalidOperationException("The Claude Code CLI was not found. Install it from https://claude.com/claude-code, then try again.");

            var prompt = new StringBuilder();
            prompt.Append("Add Velopack auto-update to the C# program \"").Append(project.AssemblyName).Append("\" (project file: ").Append(project.Path).Append("). ");
            prompt.Append("Make exactly these changes, and nothing else: ");
            prompt.Append("1. Reference the NuGet package Velopack, version ").Append(velopackVersion).Append(", as a PackageReference in the project file. ");
            prompt.Append("If the project lists its packages in packages.config, move every package there into the project file as a PackageReference with the same version, remove the <HintPath> references into the packages folder for them, and delete packages.config. ");
            prompt.Append("2. Make VelopackApp.Build().Run(); (using Velopack;) the very first statement the program runs at startup: first in Main, ");
            prompt.Append("or for WPF in a static Main you add to App.xaml.cs (mark App.xaml as Page instead of ApplicationDefinition so the generated Main goes away, then call new App().InitializeComponent() and Run()), or first in the top-level statements. ");
            prompt.Append("3. Add the file AppUpdater.cs next to the startup code with exactly the content given on standard input; change only its namespace line if the project's code uses another namespace. ");
            prompt.Append("For a project file that lists its source files (<Compile Include=...>), add it there too. ");
            prompt.Append("4. Call AppUpdater.CheckInBackground(); once the program has started, just before the main window or message loop starts (before Application.Run, or at the end of startup). ");
            prompt.Append("Do not build, run, commit or reformat anything, and do not touch other files. ");
            prompt.Append("When done, reply with one short line per file you changed, saying what changed.");

            var args = new List<string>
            {
                "-p", prompt.ToString(),
                "--output-format", "text",
                // Reading and editing the project's files is all it may do.
                "--tools", "Read,Glob,Grep,Edit,Write",
                "--allowedTools", "Read,Glob,Grep,Edit,Write",
                "--permission-mode", "acceptEdits",
                "--no-session-persistence",
            };
            var output = await RunAsync(executable, args, project.Directory, updaterSource, cancellation).ConfigureAwait(false);
            return StripFence(output);
        }

        // A lead-in Claude sometimes writes before the description ("I have enough detail now.
        // Here's the README description section.") and the closing offer after it.
        private static readonly Regex LeadIn = new Regex(
            @"^(i have|i've|i now have|i'll|i will|here('s| is| are)|below is|sure|okay|ok|great|now that|based on|after reading|having read)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ClosingRemark = new Regex(
            @"^(let me know|i can |i could |want me to|would you like|feel free|if you('d| would) like|happy to)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Rule = new Regex(@"^\s*([-*_])(\s*\1){2,}\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Removes conversational text around the Markdown: everything before a horizontal rule
        /// near the top (when it isn't part of the description), lead-in paragraphs, and closing
        /// remarks after a final rule or at the end.
        /// </summary>
        internal static string StripChatter(string text)
        {
            var lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n').ToList();

            // "Lead-in … \n---\n description": a rule within the first few lines, before any heading.
            for (int i = 0; i < Math.Min(lines.Count, 6); i++)
            {
                if (lines[i].TrimStart().StartsWith("#")) break;
                if (Rule.IsMatch(lines[i])) { lines.RemoveRange(0, i + 1); break; }
            }

            // Lead-in paragraphs without a rule.
            while (true)
            {
                Trim(lines);
                int end = lines.FindIndex(l => l.Trim().Length == 0);
                if (end <= 0) break;
                var first = string.Join(" ", lines.Take(end)).Trim();
                if (!LeadIn.IsMatch(first) || lines.Skip(end).All(l => l.Trim().Length == 0)) break;
                lines.RemoveRange(0, end);
            }

            // Closing remarks: after a final rule, or a last paragraph that is an offer.
            Trim(lines);
            for (int i = lines.Count - 1; i >= Math.Max(0, lines.Count - 6); i--)
            {
                if (lines[i].TrimStart().StartsWith("-") && !Rule.IsMatch(lines[i])) break;   // a bullet: content
                if (Rule.IsMatch(lines[i])) { lines.RemoveRange(i, lines.Count - i); break; }
            }
            Trim(lines);
            int lastBlank = lines.FindLastIndex(l => l.Trim().Length == 0);
            if (lastBlank > 0 && ClosingRemark.IsMatch(string.Join(" ", lines.Skip(lastBlank + 1)).Trim()))
                lines.RemoveRange(lastBlank, lines.Count - lastBlank);

            Trim(lines);
            return string.Join("\n", lines);
        }

        private static void Trim(List<string> lines)
        {
            while (lines.Count > 0 && lines[0].Trim().Length == 0) lines.RemoveAt(0);
            while (lines.Count > 0 && lines[lines.Count - 1].Trim().Length == 0) lines.RemoveAt(lines.Count - 1);
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
