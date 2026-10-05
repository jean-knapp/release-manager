using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace ReleaseManager.Services
{
    /// <summary>
    /// Keeps one Release Manager per Windows user: a second launch (from the jump list, say) hands
    /// its project file to the running copy over a named pipe and exits, so two windows never
    /// write the same settings file.
    /// </summary>
    public static class SingleInstance
    {
        private static readonly string Name = "ReleaseManager." + Environment.UserName;

        /// <summary>True when this is the first copy; dispose the returned mutex when the program ends.</summary>
        public static Mutex Claim(out bool first) => new Mutex(true, Name + ".Running", out first);

        /// <summary>Hands <paramref name="projectPath"/> (or nothing, to just bring it forward) to the running copy.</summary>
        public static bool Send(string projectPath)
        {
            try
            {
                using (var pipe = new NamedPipeClientStream(".", Name + ".Open", PipeDirection.Out))
                {
                    pipe.Connect(3000);
                    var bytes = Encoding.UTF8.GetBytes(projectPath ?? string.Empty);
                    pipe.Write(bytes, 0, bytes.Length);
                    pipe.Flush();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Listens for later launches on a background thread and passes each path to <paramref name="open"/>.</summary>
        public static void Listen(Action<string> open)
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        using (var pipe = new NamedPipeServerStream(Name + ".Open", PipeDirection.In, 1))
                        {
                            pipe.WaitForConnection();
                            using (var reader = new StreamReader(pipe, Encoding.UTF8)) open(reader.ReadToEnd());
                        }
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(500);
                    }
                }
            })
            { IsBackground = true, Name = "Release Manager single instance" };
            thread.Start();
        }
    }
}
