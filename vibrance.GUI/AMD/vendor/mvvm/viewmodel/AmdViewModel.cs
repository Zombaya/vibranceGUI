﻿#region

using System;
using System.IO;
using gui.app.mvvm.model;
using gui.app.utils;
using vibrance.GUI.AMD.vendor;

#endregion

namespace gui.app.mvvm.viewmodel
{
    public class AmdViewModel
    {
        public AmdViewModel(Action<string> addLogItem, AmdAdapter gpuAdapter)
        {
            MinimumVibranceLevel = 100;
            MaximumVibranceLevel = 200;

            VibranceSettingsViewModel = new VibranceSettingsViewModel(addLogItem, gpuAdapter);

            if (VibranceSettingsViewModel.SettingsExists())
            {
                VibranceSettingsViewModel.LoadVibranceSettings();
            }
            else
            {
                VibranceSettingsViewModel.SaveVibranceSettings();
            }

            var watcher = new FileSystemWatcher();
            watcher.Path = CommonUtils.GetVibrance_GUI_AppDataPath();
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = VibranceSettingsViewModel.SettingsName;
            watcher.Changed += (source, e) => VibranceSettingsViewModel.LoadVibranceSettings();
            watcher.EnableRaisingEvents = true;
        }

        public int MinimumVibranceLevel { get; private set; }
        public int MaximumVibranceLevel { get; private set; }
        public VibranceSettingsViewModel VibranceSettingsViewModel { get; }
    }
}