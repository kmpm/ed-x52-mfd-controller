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
        static X52 x52;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (x52 = new X52())
            {
                LogMonitor monitor = new LogMonitor();
                monitor.DataUpdated += OnNewLogData;
                monitor.Start();

                Application.Run(new MyApplicationContext());
            }
        }

        static void OnNewLogData(object source, LogMonitorEventArgs args)
        {
            if (args.System != null)
            {
                x52.UpdateSystem(args.System);
            }

            if (args.Commander != null) 
            {
                x52.UpdateCommander(args.Commander);
            }

            if (args.PlayMode != null)
            {
                x52.UpdatePlayMode(Regex.Replace(args.PlayMode, @"\b(\w)", m => m.Value.ToUpper()));
            }
        }
    }
}
