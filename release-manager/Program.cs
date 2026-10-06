using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReleaseManager.Forms;
using ReleaseManager.Services;
using Velopack;

namespace ReleaseManager
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            // Velopack's install, update and uninstall hooks: answered and quit before anything else.
            VelopackApp.Build().Run();
            Release.Toolchain.RegisterToolAssemblies();

            // A project file on the command line comes from the taskbar jump list (or Explorer).
            var projectPath = args.FirstOrDefault(a => a.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && File.Exists(a));

            using (var running = SingleInstance.Claim(out bool first))
            {
                // A copy is already open: it opens the project, and this one goes away.
                if (!first && SingleInstance.Send(projectPath)) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.ThreadException += (s, e) => ReportFatal(e.Exception, false);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportFatal(e.ExceptionObject as Exception, true);
                TaskScheduler.UnobservedTaskException += (s, e) => { Log(e.Exception); e.SetObserved(); };
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

                var form = new MainForm();
                if (first)
                {
                    SingleInstance.Listen(path =>
                    {
                        try { form.BeginInvoke((Action)(() => form.OpenFromAnotherInstance(path))); }
                        catch (InvalidOperationException) { }
                    });
                }
                if (projectPath != null) form.Shown += (s, e) => form.OpenProject(projectPath, true);
                AppUpdater.CheckInBackground();
                Application.Run(form);
            }
        }

        private static void ReportFatal(Exception exception, bool terminating)
        {
            Log(exception);
            try
            {
                var text = (exception?.Message ?? "Unknown error") + "\n\nDetails were written to:\n" + LogPath;
                Dialogs.Error(null, terminating ? "Release Manager stopped" : "Unexpected error", text);
            }
            catch
            {
                // The dialog itself can fail while the process is coming down; the log still has it.
            }
        }

        private static string LogPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseManager", "error.log");

        private static void Log(Exception exception)
        {
            if (exception == null) return;
            try
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                var entry = new StringBuilder();
                entry.AppendLine("---- " + DateTime.Now.ToString("u") + " ----");
                entry.AppendLine(exception.ToString());
                File.AppendAllText(LogPath, entry.ToString(), Encoding.UTF8);
            }
            catch
            {
                // Logging must never throw.
            }
        }
    }
}
