#region

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#endregion

namespace vibrance.GUI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VIBRANCE_INFO
    {
        public bool isInitialized;
        public int activeOutput;
        public int defaultHandle;
        public int userVibranceSettingDefault;
        public int userVibranceSettingActive;
        public String szGpuName;
        public bool shouldRun;
        public bool keepActive;
        public int sleepInterval;
        public List<int> displayHandles;
        public bool affectPrimaryMonitorOnly;
    }
}