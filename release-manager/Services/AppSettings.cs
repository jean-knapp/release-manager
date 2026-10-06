using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace ReleaseManager.Services
{
    /// <summary>The obfuscation protections ConfuserEx can apply, by its ids.</summary>
    public static class Protections
    {
        /// <summary>Every protection offered, with a short explanation, in ConfuserEx's ids.</summary>
        public static readonly (string Id, string Name, string Explanation)[] All =
        {
            ("rename", "Rename", "renames types, methods and fields"),
            ("ctrl flow", "Control flow", "scrambles the order of method bodies"),
            ("constants", "Constants", "encrypts strings and numbers"),
            ("ref proxy", "Reference proxy", "hides calls behind proxies"),
            ("anti tamper", "Anti tamper", "encrypts method bodies until they run"),
            ("anti debug", "Anti debug", "exits when a debugger attaches"),
            ("anti dump", "Anti dump", "resists dumping from memory"),
            ("anti ildasm", "Anti ILDasm", "stops ILDasm opening it"),
            ("invalid metadata", "Invalid metadata", "confuses decompilers"),
        };

        /// <summary>The set PrepareRelease used.</summary>
        public static List<string> Defaults() =>
            new List<string> { "anti debug", "anti dump", "anti ildasm", "anti tamper", "rename", "ctrl flow", "invalid metadata" };

        public static string NameOf(string id) => All.FirstOrDefault(p => p.Id == id).Name ?? id;
    }

    /// <summary>What Release Manager remembers about one program between sessions.</summary>
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public sealed class ProjectSettings
    {
        /// <summary>The .csproj, by full path.</summary>
        public string ProjectPath { get; set; }

        /// <summary>The version typed for the next release.</summary>
        public string Version { get; set; }

        public string RepositoryUrl { get; set; }

        /// <summary>Velopack's package id; the assembly name when empty.</summary>
        public string PackId { get; set; }

        public bool Obfuscate { get; set; } = true;

        /// <summary>
        /// ConfuserEx protection ids. Starts null: XmlSerializer adds to a list that already has
        /// items, so defaults set here would come back doubled.
        /// </summary>
        public List<string> Protections { get; set; }

        /// <summary>Where builds and packages go; the default when empty (see ReleaseFolders).</summary>
        public string OutputFolder { get; set; }

        public string ReleaseNotes { get; set; }

        /// <summary>
        /// Whether the installed program updates itself from GitHub releases. Null until the user
        /// chooses, when the project's own code decides (on when it already has the updater).
        /// </summary>
        public bool? AutoUpdate { get; set; }

        /// <summary>The updater reads releases signed in with the GitHub account on the user's PC.</summary>
        public bool UpdateSignIn { get; set; }

        /// <summary>The LICENSE published with each release; None leaves the repository's file alone.</summary>
        public Release.LicenseKind License { get; set; }

        /// <summary>What the program is and does (Markdown), kept in the README when publishing.</summary>
        public string ReadmeAbout { get; set; }
    }

    /// <summary>User preferences, window state and projects, persisted as XML under %APPDATA%\ReleaseManager.</summary>
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public sealed class AppSettings
    {
        private static AppSettings _current;

        public static AppSettings Current => _current ?? (_current = Load());

        /// <summary>%APPDATA%\ReleaseManager, or the RELEASEMANAGER_DATA folder when set, so tests keep their state apart.</summary>
        public static string Folder
        {
            get
            {
                var overridden = Environment.GetEnvironmentVariable("RELEASEMANAGER_DATA");
                return string.IsNullOrWhiteSpace(overridden)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseManager")
                    : overridden;
            }
        }

        public static string FilePath => Path.Combine(Folder, "settings.xml");

        public ThemeMode Theme { get; set; } = ThemeMode.Dark;

        public int WindowX { get; set; } = -1;
        public int WindowY { get; set; } = -1;
        public int WindowWidth { get; set; } = 1360;
        public int WindowHeight { get; set; } = 880;
        public bool WindowMaximized { get; set; }

        /// <summary>The projects open as tabs, in tab order.</summary>
        public List<string> OpenProjects { get; set; } = new List<string>();
        public int ActiveTab { get; set; } = -1;

        public List<string> RecentProjects { get; set; } = new List<string>();

        public List<ProjectSettings> Projects { get; set; } = new List<ProjectSettings>();

        /// <summary>The GitHub token, encrypted for this Windows user with DPAPI.</summary>
        public string ProtectedGitHubToken { get; set; }

        [XmlIgnore]
        public string GitHubToken
        {
            get
            {
                if (string.IsNullOrEmpty(ProtectedGitHubToken)) return null;
                try
                {
                    var bytes = ProtectedData.Unprotect(Convert.FromBase64String(ProtectedGitHubToken), Entropy, DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                ProtectedGitHubToken = string.IsNullOrEmpty(value)
                    ? null
                    : Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(value), Entropy, DataProtectionScope.CurrentUser));
            }
        }

        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("ReleaseManager.GitHubToken");

        public ProjectSettings ProjectFor(string projectPath)
        {
            var found = Projects.FirstOrDefault(p => string.Equals(p.ProjectPath, projectPath, StringComparison.OrdinalIgnoreCase));
            if (found != null) return found;
            found = new ProjectSettings { ProjectPath = projectPath, Protections = Protections.Defaults() };
            Projects.Add(found);
            return found;
        }

        public void AddRecent(string projectPath)
        {
            RecentProjects.RemoveAll(p => string.Equals(p, projectPath, StringComparison.OrdinalIgnoreCase));
            RecentProjects.Insert(0, projectPath);
            if (RecentProjects.Count > 15) RecentProjects.RemoveRange(15, RecentProjects.Count - 15);
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
                    using (var stream = File.OpenRead(FilePath))
                    {
                        var loaded = (AppSettings)serializer.Deserialize(stream);
                        foreach (var project in loaded.Projects)
                        {
                            if (project.Protections == null) project.Protections = Protections.Defaults();
                        }
                        return loaded;
                    }
                }
            }
            catch
            {
                // Corrupt settings are not worth crashing over; fall back to defaults.
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                var serializer = new XmlSerializer(typeof(AppSettings));
                var temp = FilePath + ".tmp";
                using (var stream = File.Create(temp)) serializer.Serialize(stream, this);
                if (File.Exists(FilePath)) File.Replace(temp, FilePath, null);
                else File.Move(temp, FilePath);
            }
            catch
            {
                // Ignore persistence failures (read-only profile, etc.).
            }
        }
    }
}
