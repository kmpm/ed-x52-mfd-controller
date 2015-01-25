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
        static DirectOutput x52;
        static IntPtr DirectOutputDevice;

        static String CurrentSystem;
        static String Commander;
        static String PlayMode;

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

            LogMonitor monitor = new LogMonitor();
            monitor.DataUpdated += OnNewLogData;
            monitor.Start();

            Application.Run(new MyApplicationContext());

            x52.Deinitialize();
        }

        static void OnNewLogData(object source, LogMonitorEventArgs args)
        {
            if (DirectOutputDevice != new IntPtr(0))
            {
                if (args.System != null && args.System != CurrentSystem)
                {
                    CurrentSystem = args.System;
                    x52.SetString(DirectOutputDevice, 0, 2, CurrentSystem);
                }

                if (args.Commander != null && args.Commander != Commander)
                {
                    Commander = args.Commander;
                    x52.SetString(DirectOutputDevice, 0, 0, String.Format("Cmdr {0}", Commander));
                }

                if (args.PlayMode != null && args.PlayMode != PlayMode)
                {
                    PlayMode = args.PlayMode;
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
            if (DirectOutputDevice == new IntPtr(0))
            {
                DirectOutputDevice = device;
                x52.AddPage(DirectOutputDevice, 0, DirectOutput.IsActive);
            }
        }
    }
}
