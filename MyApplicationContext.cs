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

using ED_X52_MFD_Controller.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ED_X52_MFD_Controller
{
    class MyApplicationContext : ApplicationContext
    {
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;

        public MyApplicationContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeTrayIcon();
            TrayIcon.Visible = true;
        }

        private void InitializeTrayIcon()
        {
            TrayIcon = new NotifyIcon();

            TrayIcon.Icon = Resources.SystemTrayApp;

            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();

            // Context Menu
            TrayIconContextMenu.Items.AddRange(new ToolStripItem[] { this.CloseMenuItem });
            TrayIconContextMenu.Name = "ED X52 MFD Controller";
            TrayIconContextMenu.Size = new Size(153, 70);

            // Close Menu Item
            CloseMenuItem.Name = "Close";
            CloseMenuItem.Size = new Size(152, 22);
            CloseMenuItem.Text = "Close ED X52 MFD Controller";
            CloseMenuItem.Click += new EventHandler(CloseMenuItem_Click);

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
        }

        private void OnApplicationExit(object source, EventArgs e)
        {
            TrayIcon.Visible = false;
        }

        private void CloseMenuItem_Click(object source, EventArgs e)
        {
            if (MessageBox.Show("Close ED X52 MFD Controller?",
                    "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
