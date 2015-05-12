/**
 *  ED X52 MFD Controller - Adds information parsed from the Elite Dangerous log files to the Saitek X52 Multi-Function Display.
 *  Copyright (C) 2015 Misha Kononov
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 **/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace ED_X52_MFD_Controller
{
    class LogMonitor
    {
        FileInfo CurrentFile;
        FileSystemWatcher logWatcher;
        bool ResetMonitor;
        Thread MonitorThread;

        Regex SystemRegex;
        Regex PlayerRegex;

        public LogMonitor()
        {
            SystemRegex = new Regex(@".*System:.+\((.*)\) Body.*");
            PlayerRegex = new Regex(@"FindBestIsland:(.*):(.*)");

            //Get logpath from settings
            var logPath = ED_X52_MFD_Controller.Properties.Settings.Default.LogPath;

            //if no folder given or it doesen't exist, pick one automatically
            if (String.IsNullOrEmpty(logPath) || !Directory.Exists(logPath)) { 
                //There might be different version installed, pick the last one for the directory
                //TODO: This should be some kind of configurable value.
                var productPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Frontier_Developments\\Products\\");
                var dirList = Directory.EnumerateDirectories(productPath, "FORC-FDEV*");
                var versionPath = dirList.Last();
                logPath = Path.Combine(versionPath, "Logs");
            }
    
            DirectoryInfo dir = new DirectoryInfo(logPath);

            CurrentFile = (from f in dir.GetFiles("netLog.*")
                           orderby f.LastWriteTime descending
                           select f).First();

            logWatcher = new FileSystemWatcher(dir.FullName);
            logWatcher.Filter = "netLog.*";
            logWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
            logWatcher.Created += new FileSystemEventHandler(OnNewLogFileCreate);
            logWatcher.EnableRaisingEvents = true;
        }

        public void Start()
        {
            Console.WriteLine(String.Format("Now monitoring log file '{0}'", CurrentFile.Name));
            ResetMonitor = false;
            MonitorThread = new Thread(MontiorLog);
            MonitorThread.Start();
        }

        private void OnNewLogFileCreate(object source, FileSystemEventArgs e)
        {
            CurrentFile = new FileInfo(e.FullPath);

            // Reset the monitoring thread
            ResetMonitor = true;
            MonitorThread.Join();

            this.Start();
        }

        private void MontiorLog()
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

        private void ProcessLine(String line)
        {
            Match match = SystemRegex.Match(line);
            if (match.Success)
            {
                LogMonitorEventArgs args = new LogMonitorEventArgs();
                args.System = match.Groups[1].Value;
                OnDataUpdated(args);
                return;
            }

            match = PlayerRegex.Match(line);
            if (match.Success)
            {
                LogMonitorEventArgs args = new LogMonitorEventArgs();
                args.Commander = match.Groups[1].Value;
                args.PlayMode = match.Groups[2].Value;
                OnDataUpdated(args);
            }
        }

        protected virtual void OnDataUpdated(LogMonitorEventArgs e)
        {
            EventHandler<LogMonitorEventArgs> handler = DataUpdated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<LogMonitorEventArgs> DataUpdated;
    }

    public class LogMonitorEventArgs : EventArgs
    {
        public String System;
        public String Commander;
        public String PlayMode;
    }
}