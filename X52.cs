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
using DirectOutputCSharpWrapper;

namespace ED_X52_MFD_Controller
{
    class X52 : IDisposable
    {
        const int DEFAULT_PAGE = 0;

        DirectOutput DeviceInterface;
        IntPtr device;

        String Commander;
        String PlayMode;
        String StarSystem;

        public X52()
        {
            DeviceInterface = new DirectOutput();
            DeviceInterface.Initialize("ED X52 MFD Controller");
            DeviceInterface.Enumerate(DirectOutputDeviceEnumerate);
            DeviceInterface.RegisterDeviceCallback(DirectOutputDeviceChanged);
        }

        public void Dispose()
        {
            DeviceInterface.Deinitialize();
        }

        private void DirectOutputDeviceEnumerate(IntPtr dev, IntPtr target)
        {
            if (this.device != new IntPtr(0))
            {
                throw new Exception("Attempting to initialize DirectOutput device when it is alrady initialized.");
            }

            Console.WriteLine(String.Format("Adding new DirectOutput device {0} of type: {1}", dev, DeviceInterface.GetDeviceType(dev)));

            this.device = dev;
            DeviceInterface.AddPage(device, DEFAULT_PAGE, DirectOutput.IsActive);
            DeviceInterface.RegisterPageCallback(this.device, DirectOutputDevicePageChanged);
            RefreshDeviceData();
        }

        private void DirectOutputDeviceChanged(IntPtr dev, bool added, IntPtr target)
        {
            if (added)
            {
                DirectOutputDeviceEnumerate(dev, target);
            }
            else
            {
                Console.WriteLine(String.Format("Device {0} removed from the system.", dev));
                if (dev == this.device)
                {
                    this.device = new IntPtr(0);
                }
            }
        }

        private void DirectOutputDevicePageChanged(IntPtr dev, Int32 page, bool activated, IntPtr target)
        {
            if (page == DEFAULT_PAGE && activated == true)
            {
                RefreshDeviceData();
            }
        }

        public void UpdateCommander(String name)
        {
            if (name != Commander)
            {
                Commander = name;
                if (device != new IntPtr(0))
                {
                    DeviceInterface.SetString(device, DEFAULT_PAGE, (int)Strings.FirstLine, "Cmdr " + name);
                }
            }
        }

        public void UpdatePlayMode(String mode)
        {
            if (mode != PlayMode)
            {
                PlayMode = mode;
                if (device != new IntPtr(0))
                {
                    DeviceInterface.SetString(device, DEFAULT_PAGE, (int)Strings.SecondLine, mode);
                }
            }
        }

        public void UpdateSystem(String name)
        {
            if (name != StarSystem)
            {
                StarSystem = name;
                if (device != new IntPtr(0))
                {
                    DeviceInterface.SetString(device, DEFAULT_PAGE, (int)Strings.ThirdLine, name);
                }
            }
        }

        public void RefreshDeviceData()
        {
            if (Commander != null)
            {
                DeviceInterface.SetString(device, DEFAULT_PAGE, (int)Strings.FirstLine, "Cmdr " + Commander);
            }
            if (PlayMode != null)
            {
                DeviceInterface.SetString(device, DEFAULT_PAGE, (int)Strings.SecondLine, PlayMode);
            }
            if (StarSystem != null)
            {
                DeviceInterface.SetString(device, DEFAULT_PAGE, (int)Strings.ThirdLine, StarSystem);
            }
        }
    }
}
