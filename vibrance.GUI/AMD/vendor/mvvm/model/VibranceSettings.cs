#region

using GalaSoft.MvvmLight;

#endregion

namespace gui.app.mvvm.model
{
    public class VibranceSettings : ObservableObject
    {
        private bool _autostartVibranceGui;
        private int _ingameVibranceLevel;
        private bool _keepVibranceOnWhenCsGoIsStarted;
        private int _refreshRate;
        private bool _useMultipleMonitors;
        private int _windowsVibranceLevel;

        public VibranceSettings()
        {
            _windowsVibranceLevel = 100;
            _ingameVibranceLevel = 200;
            _autostartVibranceGui = false;
            _keepVibranceOnWhenCsGoIsStarted = false;
            _useMultipleMonitors = false;
            _refreshRate = 5000;
        }

        public bool AutostartVibranceGui
        {
            get { return _autostartVibranceGui; }

            set { Set(() => AutostartVibranceGui, ref _autostartVibranceGui, value); }
        }

        public bool KeepVibranceOnWhenCsGoIsStarted
        {
            get { return _keepVibranceOnWhenCsGoIsStarted; }

            set { Set(() => KeepVibranceOnWhenCsGoIsStarted, ref _keepVibranceOnWhenCsGoIsStarted, value); }
        }

        public bool UseMultipleMonitors
        {
            get { return _useMultipleMonitors; }

            set { Set(() => UseMultipleMonitors, ref _useMultipleMonitors, value); }
        }

        public int WindowsVibranceLevel
        {
            get { return _windowsVibranceLevel; }

            set { Set(() => WindowsVibranceLevel, ref _windowsVibranceLevel, value); }
        }

        public int IngameVibranceLevel
        {
            get { return _ingameVibranceLevel; }

            set { Set(() => IngameVibranceLevel, ref _ingameVibranceLevel, value); }
        }

        public int RefreshRate
        {
            get { return _refreshRate; }

            set { Set(() => RefreshRate, ref _refreshRate, value); }
        }
    }
}