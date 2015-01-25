using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using DirectOutputCSharpWrapper;

namespace ED_X52_MFD_Controller
{
    static class Program
    {
        static FileInfo CurrentFile;

        static String CurrentSystem;
        static String Commander;
        static String PlayMode;

        static Regex systemRegex;
        static Regex commanderRegex;
        static FileSystemWatcher logWatcher;

        static bool ResetMonitor;
        static Thread MonitorThread;

        static DirectOutput x52;
        static IntPtr DirectOutputDevice;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the DirectOutput library
            x52 = new DirectOutput(Path.GetFullPath("DirectOutput.dll"));
            x52.Initialize("ED X52 MFD Controller");
            x52.Enumerate(DirectOutputDeviceEnumerate);

            systemRegex = new Regex(@".*System:.+\((.*)\) Body.*");
            commanderRegex = new Regex(@"FindBestIsland:(.*):(.*)");

            ResetMonitor = false;
            InitializeLogMonitor();
            MonitorThread = new Thread(MontiorLog);
            MonitorThread.Start();

            using (ProcessIcon pi = new ProcessIcon())
            {
                pi.Display();

                Application.Run();
            }

            x52.Deinitialize();
        }

        static void InitializeLogMonitor()
        {
            // TODO: Better to just figure out which file it is and open it here?
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Frontier_Developments\\Products\\FORC-FDEV-D-1010\\Logs"));

            CurrentFile = (from f in dir.GetFiles("netLog.*")
                           orderby f.LastWriteTime descending
                           select f).First();
            Console.WriteLine(String.Format("Now monitoring log file '{0}'", CurrentFile.Name));

            logWatcher = new FileSystemWatcher(dir.FullName);
            logWatcher.Filter = "netLog.*";
            logWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
            logWatcher.Created += new FileSystemEventHandler(OnNewLogFileCreate);
            logWatcher.EnableRaisingEvents = true;
        }

        private static void OnNewLogFileCreate(object source, FileSystemEventArgs e)
        {
            CurrentFile = new FileInfo(e.FullPath);
            Console.WriteLine(String.Format("Now monitoring log file '{0}'", e.Name));

            // Reset the monitoring thread
            ResetMonitor = true;
            MonitorThread.Join();
            ResetMonitor = false;
            MonitorThread = new Thread(MontiorLog);
            MonitorThread.Start();
        }

        static void MontiorLog()
        {
            using (FileStream fs = new FileStream(CurrentFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!ResetMonitor)
                    {
                        while (!sr.EndOfStream && !ResetMonitor)
                            ProcessLine(sr.ReadLine());
                        while (sr.EndOfStream && !ResetMonitor)
                            Thread.Sleep(100);
                        if (!ResetMonitor) ProcessLine(sr.ReadLine());
                    }
                }
            }
        }

        static void ProcessLine(String line)
        {
            Match match = systemRegex.Match(line);
            if (match.Success)
            {
                if (CurrentSystem != match.Groups[1].Value && DirectOutputDevice != null)
                {
                    CurrentSystem = match.Groups[1].Value;
                    x52.SetString(DirectOutputDevice, 0, 2, CurrentSystem);
                    return;
                }
            }

            match = commanderRegex.Match(line);
            if (match.Success)
            {
                if (Commander != match.Groups[1].Value && DirectOutputDevice != null)
                {
                    Commander = match.Groups[1].Value;
                    x52.SetString(DirectOutputDevice, 0, 0, String.Format("Cmdr {0}", Commander));
                }

                if (PlayMode != match.Groups[2].Value && DirectOutputDevice != null)
                {
                    PlayMode = match.Groups[2].Value;
                    x52.SetString(DirectOutputDevice, 0, 1, PlayMode);
                }
            }
        }

        public static void DirectOutputDeviceEnumerate(IntPtr device, IntPtr target)
        {
            DirectOutputDevice = device;
            x52.AddPage(DirectOutputDevice, 0, DirectOutput.IsActive);
        }

        public static void DirectOutputDeviceChanged(IntPtr device, bool added, IntPtr target)
        {
            if (DirectOutputDevice == null)
            {
                DirectOutputDevice = device;
                x52.AddPage(DirectOutputDevice, 0, DirectOutput.IsActive);
            }
        }
    }
}
