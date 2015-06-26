namespace vibrance.GUI
{
    internal interface ISettingsController
    {
        bool setVibranceSettings(string ingameLevel, string windowsLevel, string keepActive, string refreshRate,
            string affectPrimaryMonitorOnly);

        bool setVibranceSetting(string szKeyName, string value);

        void readVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceIngameLevel,
            out int vibranceWindowsLevel, out bool keepActive, out int refreshRate, out bool affectPrimaryMonitorOnly);
    }
}