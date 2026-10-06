using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModernWinForms;
using ReleaseManager.Controls;
using ReleaseManager.Forms;
using ReleaseManager.Release;
using ReleaseManager.Services;

namespace ReleaseManager.Views
{
    /// <summary>
    /// One program's release page. On the left, its settings: the release fields, obfuscation,
    /// auto-update and the repository's license and README description. On the right, the page
    /// header with "Run all steps", InfoBars for warnings and failures, the five steps (set up
    /// auto-update, build, obfuscate, package, publish) and the build log.
    /// </summary>
    public partial class ProjectView : ModernUserControl
    {
        private const string SigningDocsUrl = "https://docs.velopack.io/packaging/signing";

        // Left column: padding 20 16 24 24, 24 px between groups, 8 under each heading.
        private const int LeftWidth = 400;
        private const int LeftPadLeft = 24;
        private const int LeftPadRight = 16;
        private const int LeftPadTop = 20;
        private const int LeftPadBottom = 24;

        private CsProject _project;
        private ProjectSettings _settings;
        private ReleasePipeline _pipeline;
        private UpdaterStatus _updaterStatus;

        private readonly StepItem _updatesStep;
        private readonly StepItem _buildStep;
        private readonly StepItem _obfuscateStep;
        private readonly StepItem _packStep;
        private readonly StepItem _publishStep;
        private readonly List<TokenCheckBox> _protectionBoxes = new List<TokenCheckBox>();

        private CancellationTokenSource _cancel;
        private bool _busy;
        private bool _loading;

        private StepItem _running;
        private string _runningStatus;
        private StepItem _failed;
        private string _failedMessage;
        private DateTime _failedAt;
        private bool _failedNeedsToken;
        private bool _signingWarningDismissed;

        /// <summary>The next auto-update setup goes to Claude Code (chosen from the error bar).</summary>
        private bool _setUpWithClaude;
        private bool _lastSetUpUsedClaude;

        public event EventHandler TitleChanged;
        public event EventHandler StateChanged;

        public ProjectView()
        {
            InitializeComponent();

            _updatesStep = stepList.Add("Set up auto-update");
            _buildStep = stepList.Add("Build");
            _obfuscateStep = stepList.Add("Obfuscate");
            _packStep = stepList.Add("Package");
            _publishStep = stepList.Add("Publish to GitHub");
            foreach (var step in stepList.Items) step.Action += Step_Action;

            openBuildButton.IconSvg = Icons.Folder;
            openReleasesButton.IconSvg = Icons.Github;
            copyLogButton.IconSvg = Icons.Copy;
            clearLogButton.IconSvg = Icons.Delete;

            foreach (var (id, name, _) in Protections.All)
            {
                var box = new TokenCheckBox { Text = name, Tag = id };
                box.CheckedChanged += Protection_CheckedChanged;
                _protectionBoxes.Add(box);
                protectionsCard.Controls.Add(box);
            }
            // The design orders the grid by row: Rename, Constants / Anti tamper, Anti dump / ...
            var order = new[] { "rename", "constants", "anti tamper", "anti dump", "invalid metadata", "ctrl flow", "ref proxy", "anti debug", "anti ildasm" };
            _protectionBoxes.Sort((a, b) => Array.IndexOf(order, (string)a.Tag).CompareTo(Array.IndexOf(order, (string)b.Tag)));

            foreach (LicenseKind kind in Enum.GetValues(typeof(LicenseKind))) licenseCombo.Items.Add(Licenses.NameOf(kind));
            licenseCombo.SelectedIndexChanged += LicenseCombo_SelectedIndexChanged;

            obfuscateToggle.CheckedChanged += ObfuscateToggle_CheckedChanged;
            updatesToggle.CheckedChanged += UpdatesToggle_CheckedChanged;
            signInToggle.CheckedChanged += SignInToggle_CheckedChanged;
            versionBox.TextChanged += (s, e) => OnFieldChanged(() => _settings.Version = versionBox.Text.Trim());
            repoBox.TextChanged += (s, e) => OnFieldChanged(() => _settings.RepositoryUrl = repoBox.Text.Trim());
            repoBox.TextChanged += (s, e) => UpdateRepositoryHint();
            repoHint.MultiLine = true;
            packIdBox.TextChanged += (s, e) => OnFieldChanged(() => _settings.PackId = packIdBox.Text.Trim());
            outputBox.TextChanged += (s, e) => OnFieldChanged(() => _settings.OutputFolder = outputBox.Text.Trim());

            logView.LineWritten += LogView_LineWritten;
            ApplyTheme();
            Theme.Changed += (s, e) => { if (!IsDisposed) { ApplyTheme(); Invalidate(true); } };
        }

        // ------------------------------------------------------------------ public surface

        public string ProjectPath => _project?.Path;
        public string TabTitle => _project?.AssemblyName ?? "Project";
        public bool IsBusy => _busy;

        /// <summary>The project's icon file (ApplicationIcon), for the tab; null when it has none.</summary>
        public string IconPath => _project?.IconPath;

        /// <summary>The line the main window's status bar shows for this project.</summary>
        public string StatusText
        {
            get
            {
                if (_pipeline == null) return string.Empty;
                if (_running != null) return RunningVerb(_running) + " " + VersionText + (string.IsNullOrEmpty(_runningStatus) ? string.Empty : " · " + _runningStatus);
                if (_failed != null) return FailedName(_failed) + " failed · " + _failedAt.ToString("HH:mm:ss");
                var state = _pipeline.State;
                if (IsPublished) return "Published v" + state.PublishedVersion + " · " + state.PublishedAt.ToString("HH:mm:ss");
                if (_pipeline.HasPackage) return "Packaged " + state.PackedVersion + " · " + state.PackedAt.ToString("HH:mm:ss");
                if (_pipeline.HasBuild) return "Built " + state.BuiltVersion + " · " + state.BuiltAt.ToString("HH:mm:ss");
                return "Ready";
            }
        }

        public void Attach(CsProject project, ProjectSettings settings)
        {
            _project = project;
            _settings = settings;
            _pipeline = new ReleasePipeline(project, settings, logView);
            RefreshUpdaterStatus();

            _loading = true;
            try
            {
                versionBox.Text = SuggestVersion();
                repoBox.Text = settings.RepositoryUrl ?? string.Empty;
                packIdBox.Text = settings.PackId ?? string.Empty;
                packIdBox.PlaceholderText = ReleasePipeline.SafePackId(project.AssemblyName);
                outputBox.Text = settings.OutputFolder ?? string.Empty;
                obfuscateToggle.SetCheckedQuiet(settings.Obfuscate);
                var chosen = new HashSet<string>(settings.Protections ?? new List<string>());
                foreach (var box in _protectionBoxes) box.SetCheckedQuiet(chosen.Contains((string)box.Tag));

                // Until the user chooses, the project's own code decides: a project that already
                // has the updater keeps it on.
                bool hasUpdater = _updaterStatus != null && (_updaterStatus.IsInstalled || _updaterStatus.UpdaterFile != null);
                updatesToggle.SetCheckedQuiet(settings.AutoUpdate ?? hasUpdater);
                if (_updaterStatus != null && _updaterStatus.UpdaterHasSignInSetting) settings.UpdateSignIn = _updaterStatus.UpdaterUsesSignIn;
                signInToggle.SetCheckedQuiet(settings.UpdateSignIn);
                licenseCombo.SelectedIndex = (int)settings.License;
            }
            finally
            {
                _loading = false;
            }

            // The source code's own remote: shown next to the release repository, and used to fill it when empty.
            _sourceRepository = null;
            UpdateRepositoryHint();
            _ = DetectRepositoryAsync();
            RefreshCards();
            RefreshSteps();
            LayoutContent();
        }

        /// <summary>Re-reads the project file, after the update code changed it.</summary>
        public void ReloadProject()
        {
            if (_pipeline == null) return;
            _pipeline.Reload();
            _project = _pipeline.Project;
            RefreshUpdaterStatus();
            RefreshSteps();
        }

        public void CancelRunning() => _cancel?.Cancel();

        // ------------------------------------------------------------------ running steps

        private void Step_Action(object sender, EventArgs e)
        {
            var step = (StepItem)sender;
            if (_busy)
            {
                // While a step runs, its own button is the only one enabled, and it cancels.
                if (step == _running) CancelRunning();
                return;
            }
            _ = RunAsync(step);
        }

        private void runAllButton_Click(object sender, EventArgs e)
        {
            if (_busy || _pipeline == null) return;
            var steps = new List<StepItem>();
            if (updatesToggle.Checked) steps.Add(_updatesStep);
            steps.Add(_buildStep);
            if (obfuscateToggle.Checked) steps.Add(_obfuscateStep);
            steps.Add(_packStep);
            steps.Add(_publishStep);
            _ = RunAsync(steps.ToArray());
        }

        /// <summary>Runs steps in order, stopping at the first that fails or is cancelled.</summary>
        private async Task RunAsync(params StepItem[] steps)
        {
            if (_busy || _pipeline == null) return;
            SetBusy(true);
            _cancel = new CancellationTokenSource();
            bool ok = true;
            try
            {
                foreach (var step in steps)
                {
                    if (!await RunStepAsync(step))
                    {
                        ok = false;
                        break;
                    }
                }
            }
            finally
            {
                _cancel.Dispose();
                _cancel = null;
                SetBusy(false);
                RefreshSteps();
                if (ok) System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private async Task<bool> RunStepAsync(StepItem step)
        {
            if (step == _updatesStep)
            {
                bool signIn = signInToggle.Checked;
                // Claude Code takes over when Release Manager cannot edit the startup itself, or when
                // asked to after the built-in setup failed.
                bool withClaude = _setUpWithClaude || UpdatesNeedClaude;
                _setUpWithClaude = false;
                Func<CancellationToken, Task> work = withClaude
                    ? (Func<CancellationToken, Task>)(c => _pipeline.SetUpUpdatesWithClaudeAsync(signIn, c))
                    : c => _pipeline.SetUpUpdatesAsync(signIn, c);
                _lastSetUpUsedClaude = withClaude;
                return await ExecuteAsync(step, work, () => { _project = _pipeline.Project; RefreshUpdaterStatus(); });
            }
            if (step == _buildStep) return await ExecuteAsync(step, BuildAsync, () => _project = _pipeline.Project);
            if (step == _obfuscateStep)
            {
                // Obfuscating again starts from a clean build: the copy on disk is already protected.
                if (_pipeline.State.Obfuscated && !await ExecuteAsync(_buildStep, BuildAsync, () => _project = _pipeline.Project)) return false;
                return await ExecuteAsync(step, c => _pipeline.ObfuscateAsync(c));
            }
            if (step == _packStep) return await ExecuteAsync(step, c => _pipeline.PackAsync(c));
            if (step == _publishStep) return await ExecuteAsync(step, PublishAsync);
            return false;
        }

        private Task BuildAsync(CancellationToken cancellation)
        {
            var version = ReleaseVersion.Parse(versionBox.Text);
            if (version == null) throw new ReleaseException("Type the version to build as three numbers, like 1.4.2.");
            _settings.Version = ReleaseVersion.Text(version);
            AppSettings.Current.Save();
            return _pipeline.BuildAsync(version, cancellation);
        }

        private async Task PublishAsync(CancellationToken cancellation)
        {
            logView.Write(LogKind.Info, "Looking for a GitHub token…");
            GitHubToken token;
            try { token = await GitHub.FindTokenAsync(); }
            catch { token = null; }
            if (token == null)
            {
                _failedNeedsToken = true;
                throw new ReleaseException("No GitHub token was found.");
            }
            await _pipeline.PublishAsync(token, cancellation);
        }

        /// <summary>
        /// Runs one step: its row shows the progress ring and the latest log line, a failure marks
        /// the row and raises the error InfoBar. Returns whether it finished.
        /// </summary>
        private async Task<bool> ExecuteAsync(StepItem step, Func<CancellationToken, Task> work, Action onSuccess = null)
        {
            _running = step;
            _runningStatus = null;
            _failed = null;
            _failedNeedsToken = false;
            errorBar.Visible = false;
            RefreshSteps();
            LayoutContent();
            try
            {
                await work(_cancel.Token);
                onSuccess?.Invoke();
                return true;
            }
            catch (OperationCanceledException)
            {
                logView.Write(LogKind.Warning, "Cancelled.");
                return false;
            }
            catch (ClaudeSignInRequiredException ex)
            {
                logView.Write(LogKind.Error, "Claude Code is not signed in: " + ex.Message);
                ClaudeCode.OfferSignIn(FindForm(), ClaudeCode.FindExecutable(), ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                logView.Write(LogKind.Error, ex.Message);
                // The project may be half set up; read it again so the row says what is missing.
                if (step == _updatesStep) RefreshUpdaterStatus();
                _failed = step;
                _failedMessage = Services.Text.Printable(ex.Message);
                _failedAt = DateTime.Now;
                ShowError();
                return false;
            }
            finally
            {
                _running = null;
                _runningStatus = null;
                RefreshSteps();
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            foreach (var control in new Control[] { releaseCard, obfuscateCard, protectionsCard, updatesCard, signInCard, licenseCard, readmeCard })
                control.Enabled = !busy;
            Cursor = busy ? Cursors.AppStarting : Cursors.Default;
            RefreshHeader();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private static readonly Regex VpkPrefix = new Regex(@"^\[\d\d:\d\d:\d\d \w{3}\]\s*");

        private void LogView_LineWritten(object sender, LogLineEventArgs e)
        {
            if (e.Text.IndexOf("No signing parameters provided", StringComparison.OrdinalIgnoreCase) >= 0 && !_signingWarningDismissed)
            {
                var setup = (_pipeline != null ? _pipeline.PackId : "The installer") + "-win-Setup.exe";
                warnBar.Message = "No signing parameters were provided, so " + setup + " may trigger a SmartScreen warning.";
                if (!warnBar.Visible)
                {
                    warnBar.Visible = true;
                    LayoutContent();
                }
            }
            if (_running == null || e.Kind == LogKind.Command || e.Kind == LogKind.Error) return;
            var text = VpkPrefix.Replace(e.Text.Trim(), string.Empty);
            if (text.Length == 0) return;
            _runningStatus = text;
            _running.Status = text;
            stepList.Apply(_running);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        // ------------------------------------------------------------------ step rows

        private string VersionText => (ReleaseVersion.Parse(versionBox.Text) is Version v ? ReleaseVersion.Text(v) : versionBox.Text.Trim());

        private bool IsPublished
        {
            get
            {
                var state = _pipeline?.State;
                return state != null && state.PublishedVersion != null && state.PublishedVersion == state.PackedVersion;
            }
        }

        private static string Time(DateTime when) => when == DateTime.MinValue ? string.Empty : when.ToString("HH:mm:ss");

        private void RefreshSteps()
        {
            if (_pipeline == null) return;
            var state = _pipeline.State;
            bool built = _pipeline.HasBuild;
            bool packed = _pipeline.HasPackage;
            bool obfuscate = obfuscateToggle.Checked;
            int protections = _settings.Protections?.Count ?? 0;

            // 01 Set up auto-update
            var s = _updatesStep;
            Reset(s);
            if (!updatesToggle.Checked)
            {
                Set(s, StepStatus.Skipped, "Skipped · auto-update is off", "Run", false);
            }
            else if (UpdaterIsCurrent(out var reason))
            {
                Set(s, StepStatus.Done, _updaterStatus.HasOwnUpdateCode
                    ? "The project has its own update code"
                    : "Update code is in the project · AppUpdater.cs points to " + GitHub.FullName(_updaterStatus.UpdaterRepositoryUrl), "Run again", true);
                s.Time = Time(state.UpdaterAt);
            }
            else if (UpdatesNeedClaude)
            {
                // Release Manager cannot edit this startup itself: Claude Code can.
                Set(s, StepStatus.Idle, reason, "Set up with Claude", true);
            }
            else
            {
                Set(s, StepStatus.Idle, reason, "Run", _updaterStatus != null && _updaterStatus.Blockers.Count == 0);
            }

            // 02 Build
            s = _buildStep;
            Reset(s);
            if (built)
            {
                Set(s, StepStatus.Done, "Built " + state.BuiltVersion + " · " + state.BuiltFiles + " files, " + ReleasePipeline.FormatSize(state.BuiltBytes), "Rebuild", true);
                s.Time = Time(state.BuiltAt);
            }
            else Set(s, StepStatus.Idle, "Compile " + VersionText + " in Release and clean it up", "Build", true);

            // 03 Obfuscate
            s = _obfuscateStep;
            Reset(s);
            if (!obfuscate) Set(s, StepStatus.Skipped, "Skipped · obfuscation is off", "Run", false);
            else if (built && state.Obfuscated)
            {
                Set(s, StepStatus.Done, "Obfuscated with " + protections + " protection" + (protections == 1 ? string.Empty : "s")
                    + (state.NamesKept > 0 ? " · names of " + state.NamesKept + " saved type" + (state.NamesKept == 1 ? string.Empty : "s") + " kept" : string.Empty),
                    "Run again", true);
                s.Time = Time(state.ObfuscatedAt);
            }
            else if (!built) Set(s, StepStatus.Idle, "Build first", "Obfuscate", false);
            else Set(s, StepStatus.Idle, "Protect " + _project.ExeName + " with " + protections + " protection" + (protections == 1 ? string.Empty : "s"), "Obfuscate", true);

            // 04 Package
            s = _packStep;
            Reset(s);
            if (!built) Set(s, StepStatus.Idle, "Build first", "Package", false);
            else if (packed)
            {
                Set(s, StepStatus.Done, "Packaged " + state.PackedVersion + (state.SetupFile != null ? " · " + Path.GetFileName(state.SetupFile) : string.Empty), "Repackage", true);
                s.Time = Time(state.PackedAt);
            }
            else Set(s, StepStatus.Idle, obfuscate && !state.Obfuscated ? "Make the installer (obfuscate first for a protected build)" : "Make the installer and the update packages", "Package", true);

            // 05 Publish
            s = _publishStep;
            Reset(s);
            if (!packed) Set(s, StepStatus.Idle, "Package first", "Publish", false);
            else if (IsPublished)
            {
                Set(s, StepStatus.Done, "Published v" + state.PublishedVersion, "Publish", true);
                s.Time = Time(state.PublishedAt);
            }
            else Set(s, StepStatus.Idle, "Upload v" + state.PackedVersion + " to GitHub Releases", "Publish", true);

            // A failure stays on its row until something runs again.
            if (_failed != null)
            {
                Set(_failed, StepStatus.Failed, FirstLine(_failedMessage), "Retry", true);
                _failed.ActionIsAccent = true;
                _failed.Time = Time(_failedAt);
            }
            if (_running != null)
            {
                Set(_running, StepStatus.Running, _runningStatus ?? RunningVerb(_running) + "…", "Cancel", true);
                _running.Time = string.Empty;
            }
            if (_busy)
            {
                foreach (var item in stepList.Items) if (item != _running) item.ActionEnabled = false;
            }
            foreach (var item in stepList.Items) stepList.Apply(item);

            RefreshSummary();
            RefreshHeader();
        }

        private static void Reset(StepItem step)
        {
            step.Time = string.Empty;
            step.ActionIsAccent = false;
        }

        private static void Set(StepItem step, StepStatus state, string status, string action, bool enabled)
        {
            step.State = state;
            step.Status = status;
            step.ActionText = action;
            step.ActionEnabled = enabled;
        }

        private void RefreshSummary()
        {
            var counted = stepList.Items.Where(i => i.State != StepStatus.Skipped).ToList();
            if (_running != null)
            {
                stepsSummary.Text = "Step " + (counted.IndexOf(_running) + 1) + " of " + counted.Count + " running";
            }
            else
            {
                stepsSummary.Text = counted.Count(i => i.State == StepStatus.Done) + " of " + counted.Count + " complete";
            }
        }

        private static string RunningVerb(StepItem step)
        {
            switch (step.Title)
            {
                case "Set up auto-update": return "Setting up auto-update";
                case "Build": return "Building";
                case "Obfuscate": return "Obfuscating";
                case "Package": return "Packaging";
                default: return "Publishing";
            }
        }

        /// <summary>The step's name in "… failed": "Publish failed", "Build failed".</summary>
        private static string FailedName(StepItem step)
        {
            switch (step.Title)
            {
                case "Set up auto-update": return "Auto-update setup";
                case "Obfuscate": return "Obfuscation";
                case "Package": return "Packaging";
                case "Publish to GitHub": return "Publish";
                default: return step.Title;
            }
        }

        private static string FirstLine(string text) =>
            (text ?? string.Empty).Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0) ?? string.Empty;

        // ------------------------------------------------------------------ header and InfoBars

        private void RefreshHeader()
        {
            if (_pipeline == null) return;
            titleLabel.Text = "Release " + VersionText;
            var repository = GitHub.FullName(repoBox.Text) ?? "No GitHub repository yet";
            metaLabel.Text = repository + " · " + HeaderState();

            bool everythingDone = stepList.Items.All(i => i.State == StepStatus.Done || i.State == StepStatus.Skipped);
            if (_busy)
            {
                runAllButton.Text = "Running…";
                runAllButton.IconSvg = Icons.Run;
            }
            else if (everythingDone && _failed == null)
            {
                runAllButton.Text = "Run all again";
                runAllButton.IconSvg = Icons.Refresh;
            }
            else
            {
                runAllButton.Text = "Run all steps";
                runAllButton.IconSvg = Icons.Run;
            }
            runAllButton.Enabled = !_busy;
            openReleasesButton.Enabled = GitHub.NormalizeRepositoryUrl(repoBox.Text) != null;
            runAllButton.Invalidate();
            LayoutHeader();
        }

        private string HeaderState()
        {
            if (_running != null) return RunningVerb(_running) + "…";
            if (_failed != null) return FailedName(_failed) + " failed at " + _failedAt.ToString("HH:mm");
            var state = _pipeline.State;
            if (IsPublished) return "Published " + When(state.PublishedAt);
            if (_pipeline.HasPackage) return "Packaged " + When(state.PackedAt);
            if (_pipeline.HasBuild) return "Built " + When(state.BuiltAt);
            return "Not built yet";
        }

        /// <summary>"today at 22:17", "yesterday at 09:05" or "on 3 Oct at 22:17".</summary>
        private static string When(DateTime time)
        {
            if (time.Date == DateTime.Today) return "today at " + time.ToString("HH:mm");
            if (time.Date == DateTime.Today.AddDays(-1)) return "yesterday at " + time.ToString("HH:mm");
            return "on " + time.ToString("d MMM") + " at " + time.ToString("HH:mm");
        }

        private void ShowError()
        {
            errorBar.Title = FailedName(_failed) + " failed.";
            errorBar.Message = _failedNeedsToken
                ? "No GitHub token was found. Sign in to GitHub with git or add a personal access token, then retry."
                : _failedMessage.Replace("\r\n", "\n").Replace("\n\n", " ").Replace('\n', ' ');
            errorBar.ActionText = _failedNeedsToken ? "Add token…" : OffersClaude ? "Set up with Claude" : string.Empty;
            errorBar.Visible = true;
            LayoutContent();
        }

        /// <summary>The built-in auto-update setup failed: Claude Code can try instead.</summary>
        private bool OffersClaude => _failed == _updatesStep && !_lastSetUpUsedClaude;

        private async void errorBar_ActionClick(object sender, EventArgs e)
        {
            if (OffersClaude)
            {
                if (_busy) return;
                _setUpWithClaude = true;
                await RunAsync(_updatesStep);
                return;
            }
            if (!_failedNeedsToken) return;
            if (await GitHubSignIn.AddTokenAsync(FindForm()) == null) return;
            logView.Write(LogKind.Info, "GitHub token saved. Retry the publish step to use it.");
        }

        private void warnBar_ActionClick(object sender, EventArgs e) => StartShell(SigningDocsUrl, null);

        private void warnBar_Closed(object sender, EventArgs e)
        {
            _signingWarningDismissed = true;
            LayoutContent();
        }

        // ------------------------------------------------------------------ settings fields

        private string SuggestVersion()
        {
            if (!string.IsNullOrWhiteSpace(_settings.Version)) return _settings.Version.Trim();
            var current = ReleaseVersion.Parse(_project.CurrentVersion);
            return current != null ? ReleaseVersion.Text(current) : "1.0.0";
        }

        private void OnFieldChanged(Action apply)
        {
            if (_loading || _settings == null) return;
            apply();
            AppSettings.Current.Save();
            RefreshCards();
            RefreshSteps();
            TitleChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>The cards' descriptions, which follow their switches.</summary>
        private void RefreshCards()
        {
            if (_settings == null) return;
            var published = _pipeline?.State.PublishedVersion;
            versionHint.Text = "Three numbers, like 1.4.2." + (published != null ? " Last published: " + published + "." : string.Empty);

            int count = _protectionBoxes.Count(b => b.Checked);
            obfuscateCard.Description = obfuscateToggle.Checked ? count + " of " + _protectionBoxes.Count + " protections on" : "The build is published as compiled";
            updatesCard.Description = updatesToggle.Checked
                ? "The installed program checks for a new release on start. The first step adds the update code to the project."
                : "The installed program does not check for updates.";
            licenseCard.Description = Licenses.DescribeOf(_settings.License);

            var about = (_settings.ReadmeAbout ?? string.Empty).Trim();
            readmeCard.Description = about.Length == 0
                ? "Not written yet. Shown in the README under the download button."
                : "Written. Published to the README with the next release.";
            readmeButton.Text = about.Length == 0 ? "Write…" : "Edit…";
            readmeButton.Width = Math.Max(80, readmeButton.PreferredWidth);
        }

        private void ObfuscateToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (!_loading && _settings != null)
            {
                _settings.Obfuscate = obfuscateToggle.Checked;
                AppSettings.Current.Save();
            }
            RefreshCards();
            RefreshSteps();
            LayoutContent();
        }

        private void Protection_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading || _settings == null) return;
            _settings.Protections = _protectionBoxes.Where(b => b.Checked).Select(b => (string)b.Tag).ToList();
            AppSettings.Current.Save();
            RefreshCards();
            RefreshSteps();
        }

        private void UpdatesToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (!_loading && _settings != null)
            {
                _settings.AutoUpdate = updatesToggle.Checked;
                AppSettings.Current.Save();
            }
            RefreshCards();
            RefreshSteps();
            LayoutContent();
        }

        private void SignInToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading || _settings == null) return;
            _settings.UpdateSignIn = signInToggle.Checked;
            AppSettings.Current.Save();
            RefreshSteps();
        }

        private void LicenseCombo_SelectedIndexChanged(object sender, ModernWinForms.SelectedIndexChangedEventArgs e)
        {
            if (_loading || _settings == null || licenseCombo.SelectedIndex < 0) return;
            _settings.License = (LicenseKind)licenseCombo.SelectedIndex;
            AppSettings.Current.Save();
            RefreshCards();
            LayoutContent();
        }

        private void readmeButton_Click(object sender, EventArgs e)
        {
            if (_project == null) return;
            using (var dialog = new ReadmeAboutDialog(_project, _settings.ReadmeAbout))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                _settings.ReadmeAbout = dialog.About;
                AppSettings.Current.Save();
            }
            RefreshCards();
            LayoutContent();
        }

        private void chooseRepoButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new GitHubRepositoryDialog
            {
                CurrentRepositoryUrl = repoBox.Text,
                SuggestedName = _project?.AssemblyName,
                SuggestedDescription = _project != null ? "Releases of " + _project.AssemblyName : null,
            })
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK || dialog.SelectedRepository == null) return;
                repoBox.Text = GitHub.NormalizeRepositoryUrl(dialog.SelectedRepository.HtmlUrl) ?? dialog.SelectedRepository.HtmlUrl;
                repoBox.Focus();
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Where releases are built and packaged" })
            {
                if (!string.IsNullOrWhiteSpace(outputBox.Text) && Directory.Exists(outputBox.Text)) dialog.SelectedPath = outputBox.Text;
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK) outputBox.Text = dialog.SelectedPath;
            }
        }

        // The GitHub repository the project's source code is pushed to (git origin); null when none.
        private string _sourceRepository;

        /// <summary>
        /// Reads the source code's git remote. It fills an empty release repository (most projects
        /// publish where their code lives), but never replaces one that was set: releases can go to
        /// another repository, e.g. a public one for a private codebase.
        /// </summary>
        private async Task DetectRepositoryAsync()
        {
            if (_project == null) return;
            var project = _project;
            var url = await GitHub.RepositoryFromGitAsync(project.Directory);
            if (project != _project) return;
            _sourceRepository = url;
            if (url != null && string.IsNullOrWhiteSpace(repoBox.Text)) repoBox.Text = url;
            UpdateRepositoryHint();
        }

        /// <summary>Says what the release repository is for, and how it relates to the source code's remote.</summary>
        private void UpdateRepositoryHint()
        {
            var release = GitHub.NormalizeRepositoryUrl(repoBox.Text);
            var source  = _sourceRepository;
            string text;
            if (release == null)
                text = "Where releases are published and the installed program looks for updates." +
                       (source != null ? " The source code is in " + GitHub.FullName(source) + "." : string.Empty);
            else if (source == null)
                text = "Releases, the README download button and update checks use this repository.";
            else if (string.Equals(release, source, StringComparison.OrdinalIgnoreCase))
                text = "The same repository as the source code (git origin). Choose another to publish releases separately.";
            else
                text = "Releases are published here, apart from the source code in " + GitHub.FullName(source) +
                       ". The README, LICENSE and update checks use this repository.";
            if (repoHint.Text == text) return;
            repoHint.Text = text;
            PerformLayout();
        }

        // ------------------------------------------------------------------ update code

        private void RefreshUpdaterStatus()
        {
            if (_project == null) return;
            try { _updaterStatus = UpdaterSetup.Inspect(_project); }
            catch { _updaterStatus = null; }
        }

        /// <summary>
        /// The update code is missing and Release Manager cannot add it by itself: there is no plain
        /// Main to edit, or its packages are in a form it does not handle. A library is not a case
        /// for Claude either: it has nothing to update.
        /// </summary>
        private bool UpdatesNeedClaude =>
            _updaterStatus != null && !_updaterStatus.HasOwnUpdateCode && !_updaterStatus.IsInstalled
            && _updaterStatus.Blockers.Count > 0 && _project != null && _project.IsExecutable;

        /// <summary>Whether the project's update code matches the settings; when not, why not.</summary>
        private bool UpdaterIsCurrent(out string reason)
        {
            reason = null;
            var status = _updaterStatus;
            if (status == null)
            {
                reason = "Could not read the project's sources";
                return false;
            }
            if (status.HasOwnUpdateCode) return true;
            if (status.Blockers.Count > 0)
            {
                reason = status.Blockers[0];
                return false;
            }
            if (!status.IsInstalled)
            {
                reason = status.MigratesPackagesConfig
                    ? "Moves the NuGet packages from packages.config to PackageReference, then adds the update code"
                    : "Adds the update code to the project";
                return false;
            }
            var wanted = GitHub.NormalizeRepositoryUrl(repoBox.Text);
            if (wanted != null && !string.Equals(GitHub.NormalizeRepositoryUrl(status.UpdaterRepositoryUrl), wanted, StringComparison.OrdinalIgnoreCase))
            {
                reason = "AppUpdater.cs points to " + (GitHub.FullName(status.UpdaterRepositoryUrl) ?? "another repository") + " · run to point it at " + GitHub.FullName(wanted);
                return false;
            }
            if (status.UpdaterUsesSignIn != signInToggle.Checked)
            {
                reason = signInToggle.Checked ? "Run to turn on sign-in for private repositories" : "Run to turn off sign-in for private repositories";
                return false;
            }
            return true;
        }

        // ------------------------------------------------------------------ header buttons and log

        private void copyLogButton_Click(object sender, EventArgs e)
        {
            if (!logView.HasContent) return;
            try { Clipboard.SetText(logView.AllText()); } catch { }
        }

        private void clearLogButton_Click(object sender, EventArgs e) => logView.Clear();

        /// <summary>
        /// Shows the build in Explorer with the program selected; before the first build, the
        /// release folder it will land in.
        /// </summary>
        private void openBuildButton_Click(object sender, EventArgs e)
        {
            if (_project == null || _settings == null) return;
            try
            {
                var folders = new ReleaseFolders(_project, _settings.OutputFolder);
                var exe = Path.Combine(folders.Build, _project.ExeName);
                if (File.Exists(exe))
                {
                    StartShell("explorer.exe", "/select,\"" + exe + "\"");
                    return;
                }
                if (Directory.Exists(folders.Build))
                {
                    StartShell("explorer.exe", "\"" + folders.Build + "\"");
                    return;
                }
                Directory.CreateDirectory(folders.Root);
                logView.Write(LogKind.Info, "Nothing is built yet. Opened the release folder; the build goes into its \"build\" folder.");
                StartShell("explorer.exe", "\"" + folders.Root + "\"");
            }
            catch (Exception ex)
            {
                Dialogs.Error(FindForm(), "Build folder", ex.Message);
            }
        }

        /// <summary>Opens the repository's releases page in the browser.</summary>
        private void openReleasesButton_Click(object sender, EventArgs e)
        {
            var repository = GitHub.NormalizeRepositoryUrl(repoBox.Text);
            if (repository == null) return;
            StartShell(repository + "/releases", null);
        }

        private void StartShell(string file, string arguments)
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file, arguments ?? string.Empty) { UseShellExecute = true }); }
            catch (Exception ex) { Dialogs.Error(FindForm(), "Open", ex.Message); }
        }

        // ------------------------------------------------------------------ theme and layout

        private void ApplyTheme()
        {
            var p = Theme.Palette;
            foreach (var box in new[] { versionBox, repoBox, packIdBox, outputBox }) box.Font = Fonts.Ui(14f);
            licenseCombo.Font = Fonts.Ui(14f);
            headerDivider.CustomFill = p.Divider;
            logCard.CustomFill = p.ListBackground;
            logCard.CustomBorder = p.CardBorder;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutContent();
        }

        private void LayoutContent()
        {
            if (rightPanel == null) return;
            LayoutLeft();
            LayoutRight();
        }

        private void LayoutLeft()
        {
            leftScroll.Width = LeftWidth;
            // Laid out twice when the content turns out taller than the column: the scroll bar
            // then takes its 12 px from the cards' width.
            int contentHeight = LayoutLeftAt(LeftWidth - LeftPadLeft - LeftPadRight);
            if (contentHeight > leftScroll.Height) contentHeight = LayoutLeftAt(LeftWidth - LeftPadLeft - LeftPadRight - 12);
            leftScroll.ContentWidth = LeftWidth - (contentHeight > leftScroll.Height ? 12 : 0);
            leftScroll.ContentHeight = contentHeight;
        }

        /// <summary>Places the left column's groups at <paramref name="width"/> and returns the height they take.</summary>
        private int LayoutLeftAt(int width)
        {
            int x = LeftPadLeft;
            int y = LeftPadTop;

            // Release: one card, padding 16, 16 px between fields, label 6 px above its control.
            Heading(releaseHeading, x, ref y, width);
            releaseCard.SetBounds(x, y, width, 10);
            int inner = width - 32;
            int fy = 16;
            versionLabel.SetBounds(16, fy, inner, 20); fy += 26;
            versionBox.SetBounds(16, fy, 140, 32); fy += 38;
            versionHint.SetBounds(16, fy, inner, 16); fy += 16 + 16;
            repoLabel.SetBounds(16, fy, inner, 20); fy += 26;
            chooseRepoButton.Width = chooseRepoButton.PreferredWidth;
            repoBox.SetBounds(16, fy, inner - chooseRepoButton.Width - 8, 32);
            chooseRepoButton.SetBounds(16 + inner - chooseRepoButton.Width, fy, chooseRepoButton.Width, 32); fy += 38;
            int hintHeight = TextRenderer.MeasureText(repoHint.Text, Fonts.Ui(repoHint.SizePx), new Size(inner, int.MaxValue),
                                                      TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height;
            repoHint.SetBounds(16, fy, inner, Math.Max(16, hintHeight)); fy += repoHint.Height + 16;
            packIdLabel.SetBounds(16, fy, inner, 20); fy += 26;
            packIdBox.SetBounds(16, fy, inner, 32); fy += 32 + 16;
            outputLabel.SetBounds(16, fy, inner, 20); fy += 26;
            browseButton.Width = browseButton.PreferredWidth;
            outputBox.SetBounds(16, fy, inner - browseButton.Width - 8, 32);
            browseButton.SetBounds(16 + inner - browseButton.Width, fy, browseButton.Width, 32); fy += 32 + 16;
            releaseCard.Height = fy;
            y += fy + 24;

            // Obfuscation: the switch card, and the protections 2 px under it while it is on.
            Heading(obfuscationHeading, x, ref y, width);
            y = Card(obfuscateCard, x, y, width);
            protectionsCard.Visible = obfuscateToggle.Checked;
            if (protectionsCard.Visible)
            {
                y += 2;
                protectionsCard.SetBounds(x, y, width, 10);
                protectionsLabel.SetBounds(16, 12, width - 32, 16);
                int columnWidth = (width - 32 - 16) / 2;
                int top = 12 + 16 + 12;
                for (int i = 0; i < _protectionBoxes.Count; i++)
                {
                    _protectionBoxes[i].SetBounds(16 + (i % 2) * (columnWidth + 16), top + (i / 2) * (20 + 12), columnWidth, 20);
                }
                int rows = (_protectionBoxes.Count + 1) / 2;
                protectionsCard.Height = top + rows * 20 + (rows - 1) * 12 + 16;
                y += protectionsCard.Height;
            }
            y += 24;

            // Auto-update: the switch card, and the sign-in card 2 px under it while it is on.
            Heading(updatesHeading, x, ref y, width);
            y = Card(updatesCard, x, y, width);
            signInCard.Visible = updatesToggle.Checked;
            if (signInCard.Visible) y = Card(signInCard, x, y + 2, width);
            y += 24;

            // Repository: the license and the README description.
            Heading(repositoryHeading, x, ref y, width);
            y = Card(licenseCard, x, y, width);
            y = Card(readmeCard, x, y + 2, width);
            return y + LeftPadBottom;
        }

        private static void Heading(TextLabel label, int x, ref int y, int width)
        {
            label.SetBounds(x, y, width, 20);
            y += 20 + 8;
        }

        private static int Card(SettingsCard card, int x, int y, int width)
        {
            card.SetBounds(x, y, width, card.MeasureHeight(width));
            return y + card.Height;
        }

        private void LayoutHeader()
        {
            int left = 24, top = 20;
            int right = rightPanel.ClientSize.Width - 24;
            int blockBottom = top + 36 + 2 + 16;

            runAllButton.Width = Math.Max(120, runAllButton.PreferredWidth);
            openReleasesButton.Width = openReleasesButton.PreferredWidth;
            openBuildButton.Width = openBuildButton.PreferredWidth;
            int buttonTop = blockBottom - 32;
            runAllButton.SetBounds(right - runAllButton.Width, buttonTop, runAllButton.Width, 32);
            headerDivider.SetBounds(runAllButton.Left - 8 - 1, buttonTop + 6, 1, 20);
            openReleasesButton.SetBounds(headerDivider.Left - 8 - openReleasesButton.Width, buttonTop, openReleasesButton.Width, 32);
            openBuildButton.SetBounds(openReleasesButton.Left - 4 - openBuildButton.Width, buttonTop, openBuildButton.Width, 32);

            int textWidth = Math.Max(0, openBuildButton.Left - 16 - left);
            titleLabel.SetBounds(left, top, textWidth, 36);
            metaLabel.SetBounds(left, top + 38, textWidth, 16);
        }

        private void LayoutRight()
        {
            LayoutHeader();
            int left = 24;
            int width = Math.Max(200, rightPanel.ClientSize.Width - 48);
            int y = 20 + 36 + 2 + 16 + 16;

            foreach (var bar in new[] { warnBar, errorBar })
            {
                if (!bar.Visible) continue;
                bar.SetBounds(left, y, width, bar.Height);
                bar.Width = width;
                y += bar.Height + 16;
            }

            stepsHeading.Width = stepsHeading.PreferredWidth + 2;
            stepsHeading.SetBounds(left, y, stepsHeading.Width, 20);
            stepsSummary.SetBounds(stepsHeading.Right + 6, y + 1, Math.Max(0, width - stepsHeading.Width - 6), 20);
            y += 20 + 8;
            stepList.SetBounds(left, y, width, stepList.ContentHeight);
            y += stepList.Height + 16;

            clearLogButton.Width = clearLogButton.PreferredWidth;
            copyLogButton.Width = copyLogButton.PreferredWidth;
            clearLogButton.SetBounds(left + width - clearLogButton.Width, y, clearLogButton.Width, 32);
            copyLogButton.SetBounds(clearLogButton.Left - 8 - copyLogButton.Width, y, copyLogButton.Width, 32);
            logHeading.SetBounds(left, y + 6, 200, 20);
            y += 32 + 8;
            logCard.SetBounds(left, y, width, Math.Max(80, rightPanel.ClientSize.Height - 16 - y));
        }
    }
}
