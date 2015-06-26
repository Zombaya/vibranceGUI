#region

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace vibrance.GUI
{
    internal class SettingsController : ISettingsController
    {
        private const string szSectionName = "Settings";
        private const string szKeyNameInactive = "inactiveValue";
        private const string szKeyNameActive = "activeValue";
        private const string szKeyNameKeepActive = "keepActive";
        private const string szKeyNameRefreshRate = "refreshRate";
        private const string szKeyNameAffectPrimaryMonitorOnly = "affectPrimaryMonitorOnly";

        private readonly string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                           "\\vibranceGUI\\vibranceGUI.ini";

        public bool setVibranceSettings(string ingameLevel, string windowsLevel, string keepActive, string refreshRate,
            string affectPrimaryMonitorOnly)
        {
            if (!prepareFile())
            {
                return false;
            }

            WritePrivateProfileString(szSectionName, szKeyNameActive, ingameLevel, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameInactive, windowsLevel, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameKeepActive, keepActive, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameRefreshRate, refreshRate, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameAffectPrimaryMonitorOnly, affectPrimaryMonitorOnly,
                fileName);

            return (Marshal.GetLastWin32Error() == 0);
        }

        public bool setVibranceSetting(string szKeyName, string value)
        {
            if (!prepareFile())
            {
                return false;
            }

            WritePrivateProfileString(szSectionName, szKeyName, value, fileName);

            return (Marshal.GetLastWin32Error() == 0);
        }

        public void readVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceIngameLevel,
            out int vibranceWindowsLevel, out bool keepActive, out int refreshRate, out bool affectPrimaryMonitorOnly)
        {
            var defaultLevel = 0;
            var maxLevel = 0;
            var defaultRefreshRate = 0;
            var minRefreshRate = 0;
            if (graphicsAdapter == GraphicsAdapter.NVIDIA)
            {
                defaultLevel = NvidiaVibranceProxy.NVAPI_DEFAULT_LEVEL;
                maxLevel = NvidiaVibranceProxy.NVAPI_MAX_LEVEL;
                defaultRefreshRate = NvidiaVibranceProxy.NVAPI_DEFAULT_REFRESH_RATE;
                minRefreshRate = NvidiaVibranceProxy.NVAPI_MIN_REFRESH_RATE;
            }


            if (!isFileExisting(fileName))
            {
                vibranceIngameLevel = defaultLevel;
                vibranceWindowsLevel = defaultLevel;
                refreshRate = defaultRefreshRate;
                keepActive = false;
                affectPrimaryMonitorOnly = false;
                return;
            }

            var szDefault = "";

            var szValueActive = new StringBuilder(1024);

            GetPrivateProfileString(szSectionName,
                szKeyNameActive,
                szDefault,
                szValueActive,
                Convert.ToUInt32(szValueActive.Capacity),
                fileName);

            var szValueInactive = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameInactive,
                szDefault,
                szValueInactive,
                Convert.ToUInt32(szValueInactive.Capacity),
                fileName);

            var szValueRefreshRate = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameRefreshRate,
                szDefault,
                szValueRefreshRate,
                Convert.ToUInt32(szValueRefreshRate.Capacity),
                fileName);


            var szValueKeepActive = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameKeepActive,
                szDefault,
                szValueKeepActive,
                Convert.ToUInt32(szValueKeepActive.Capacity),
                fileName);

            var szValueAffectPrimaryMonitorOnly = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameAffectPrimaryMonitorOnly,
                szDefault,
                szValueAffectPrimaryMonitorOnly,
                Convert.ToUInt32(szValueAffectPrimaryMonitorOnly.Capacity),
                fileName);

            try
            {
                vibranceWindowsLevel = int.Parse(szValueInactive.ToString());
                vibranceIngameLevel = int.Parse(szValueActive.ToString());
                refreshRate = int.Parse(szValueRefreshRate.ToString());
                keepActive = bool.Parse(szValueKeepActive.ToString());
                affectPrimaryMonitorOnly = bool.Parse(szValueAffectPrimaryMonitorOnly.ToString());
            }
            catch (Exception)
            {
                vibranceIngameLevel = defaultLevel;
                vibranceWindowsLevel = defaultLevel;
                refreshRate = defaultRefreshRate;
                keepActive = false;
                affectPrimaryMonitorOnly = false;
                return;
            }

            if (vibranceWindowsLevel < defaultLevel || vibranceWindowsLevel > maxLevel)
                vibranceWindowsLevel = defaultLevel;
            if (vibranceIngameLevel < defaultLevel || vibranceIngameLevel > maxLevel)
                vibranceIngameLevel = maxLevel;
            if (refreshRate < minRefreshRate)
                refreshRate = defaultRefreshRate;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern bool WritePrivateProfileString(string lpAppName,
            string lpKeyName, string lpString, string lpFileName);

        private bool prepareFile()
        {
            if (!isFileExisting(fileName))
            {
                var sw = new StreamWriter(fileName);
                sw.Close();
                if (!isFileExisting(fileName))
                {
                    return false;
                }
            }

            return true;
        }

        private bool isFileExisting(string szFilename)
        {
            return File.Exists(szFilename);
        }
    }
}