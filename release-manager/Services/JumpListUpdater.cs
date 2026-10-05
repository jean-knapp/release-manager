using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Shell;

namespace ReleaseManager.Services
{
    /// <summary>
    /// The taskbar jump list: the recent projects as tasks, each launching Release Manager with the
    /// project file, which a running copy picks up (see <see cref="SingleInstance"/>).
    /// </summary>
    public static class JumpListUpdater
    {
        private const int MaxItems = 10;

        public static void Update(IEnumerable<string> recentProjects)
        {
            try
            {
                var exe = Application.ExecutablePath;
                var list = new JumpList { ShowRecentCategory = false, ShowFrequentCategory = false };
                foreach (var path in (recentProjects ?? Enumerable.Empty<string>()).Where(File.Exists).Take(MaxItems))
                {
                    list.JumpItems.Add(new JumpTask
                    {
                        Title = Path.GetFileNameWithoutExtension(path),
                        Description = path,
                        ApplicationPath = exe,
                        Arguments = "\"" + path + "\"",
                        IconResourcePath = exe,
                        // No custom category: the shell rejects tasks in one ("InvalidItem") unless
                        // the program owns the file type, so they are listed under Tasks.
                    });
                }
                list.Apply();
            }
            catch (Exception)
            {
                // The jump list is a convenience; a shell that refuses it changes nothing else.
            }
        }
    }
}
