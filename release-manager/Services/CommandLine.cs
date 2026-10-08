using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ReleaseManager.Release;

namespace ReleaseManager.Services
{
    /// <summary>
    /// Releases a project without the window, for scripts and agents (Claude Code):
    ///
    ///   "Release Manager.exe" --release path\to\app.csproj [--version 1.4.2] [--notes "text"] [--steps build,obfuscate,pack,publish] [--replace]
    ///
    /// It uses the project's saved settings (repository, obfuscation and protections, license, README text), the same
    /// steps as the window, and the same GitHub token lookup. The version defaults to the one after the last published
    /// release; --steps defaults to every step (obfuscate only when the project's settings turn it on); --replace removes
    /// packages already at or above the version from the local packages folder, as the window offers to.
    /// Progress goes to the console (and to cli.log next to the settings); the exit code is 0 when every step finished.
    /// </summary>
    public static class CommandLine
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int processId);

        public static bool Wants(string[] args) => args.Any(a => string.Equals(a, "--release", StringComparison.OrdinalIgnoreCase));

        public static int Run(string[] args)
        {
            AttachConsole(-1);   // a terminal that started us shows the output; redirected output works either way
            var log = new ConsoleLog();
            try
            {
                return RunAsync(args, log).GetAwaiter().GetResult();
            }
            catch (ReleaseException ex)
            {
                log.Write(LogKind.Error, ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                log.Write(LogKind.Error, ex.GetType().Name + ": " + ex.Message);
                return 1;
            }
        }

        private static string Option(string[] args, string name)
        {
            int i = Array.FindIndex(args, a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }

        private static async Task<int> RunAsync(string[] args, ConsoleLog log)
        {
            var path = Option(args, "--release");
            if (path == null || !File.Exists(path)) throw new ReleaseException("--release needs the path of an existing .csproj.");
            path = Path.GetFullPath(path);
            var project = CsProject.Load(path);
            var settings = AppSettings.Current.ProjectFor(path);
            var pipeline = new ReleasePipeline(project, settings, log);

            var notes = Option(args, "--notes");
            if (notes != null) settings.ReleaseNotes = notes;

            var published = ReleaseVersion.Parse(pipeline.State.PublishedVersion);
            var version = ReleaseVersion.Parse(Option(args, "--version"))
                ?? (published != null ? ReleaseVersion.NextPatch(published) : ReleaseVersion.Parse(settings.Version) ?? new Version(1, 0, 0));
            if (published != null && version <= published && !args.Contains("--replace"))
                throw new ReleaseException("Version " + ReleaseVersion.Text(version) + " is not above the published " + ReleaseVersion.Text(published) + ". Raise it with --version.");
            settings.Version = ReleaseVersion.Text(version);
            AppSettings.Current.Save();

            var steps = (Option(args, "--steps") ?? "build,obfuscate,pack,publish").Split(',').Select(s => s.Trim().ToLowerInvariant()).ToList();
            var cancel = CancellationToken.None;
            log.Write(LogKind.Info, "Releasing " + project.AssemblyName + " " + ReleaseVersion.Text(version) + " to " + (settings.RepositoryUrl ?? "(no repository set)")
                + ": " + string.Join(", ", steps));

            if (steps.Contains("build")) await pipeline.BuildAsync(version, cancel).ConfigureAwait(false);
            if (steps.Contains("obfuscate") && settings.Obfuscate && !pipeline.State.Obfuscated) await pipeline.ObfuscateAsync(cancel).ConfigureAwait(false);
            if (steps.Contains("pack"))
            {
                var built = ReleaseVersion.Parse(pipeline.State.BuiltVersion);
                var blocking = built != null ? pipeline.PackagedAtOrAbove(built) : new List<Version>();
                if (blocking.Count > 0)
                {
                    if (!args.Contains("--replace"))
                        throw new ReleaseException("The packages folder already has " + string.Join(", ", blocking.Select(ReleaseVersion.Text))
                            + " at or above " + ReleaseVersion.Text(built) + ". Add --replace to remove them locally (nothing on GitHub changes), or raise the version.");
                    pipeline.RemovePackagedReleases(blocking);
                }
                await pipeline.PackAsync(cancel).ConfigureAwait(false);
            }
            if (steps.Contains("publish"))
            {
                log.Write(LogKind.Info, "Looking for a GitHub token…");
                GitHubToken token = null;
                try { token = await GitHub.FindTokenAsync().ConfigureAwait(false); }
                catch (Exception) { }
                if (token == null) throw new ReleaseException("No GitHub token was found: save one in Release Manager, set GITHUB_TOKEN, or sign in to GitHub with git.");
                await pipeline.PublishAsync(token, cancel).ConfigureAwait(false);
            }
            log.Write(LogKind.Success, "Done.");
            return 0;
        }

        private sealed class ConsoleLog : IReleaseLog
        {
            private readonly object _gate = new object();
            private readonly string _file = Path.Combine(AppSettings.Folder, "cli.log");

            public void Write(LogKind kind, string text)
            {
                var line = DateTime.Now.ToString("HH:mm:ss") + " " + Prefix(kind) + text;
                lock (_gate)
                {
                    try { Console.WriteLine(line); } catch (IOException) { }
                    try { Directory.CreateDirectory(AppSettings.Folder); File.AppendAllText(_file, line + Environment.NewLine); } catch (Exception) { }
                }
            }

            private static string Prefix(LogKind kind)
            {
                switch (kind)
                {
                    case LogKind.Command: return "> ";
                    case LogKind.Output: return "  ";
                    case LogKind.Warning: return "WARNING ";
                    case LogKind.Error: return "ERROR ";
                    case LogKind.Success: return "OK ";
                    default: return string.Empty;
                }
            }
        }
    }
}
