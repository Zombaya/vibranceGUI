#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using gui.app.utils;

#endregion

namespace vibrance.GUI
{
    public partial class NvidiaVibranceGUI : Form
    {
        private const string appName = "vibranceGUI";
        private const string twitterLink = "https://twitter.com/juvlarN";

        private const string paypalDonationLink =
            "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=JDQFNKNNEW356";

        private readonly AutoResetEvent resetEvent;
        private bool allowVisible;
        private RegistryController registryController;
        public bool silenced = false;
        private NvidiaVibranceProxy v;

        public NvidiaVibranceGUI()
        {
            const string nvidiaAdapterName = "vibranceDLL.dll";
            var resourceName = string.Format("{0}.NVIDIA.{1}", typeof (Program).Namespace, nvidiaAdapterName);

            var dllPath = CommonUtils.LoadUnmanagedLibraryFromResource(
                Assembly.GetExecutingAssembly(),
                resourceName,
                nvidiaAdapterName);

            allowVisible = true;

            InitializeComponent();

            Marshal.PrelinkAll(typeof (NvidiaVibranceProxy));
            resetEvent = new AutoResetEvent(false);
            backgroundWorker.WorkerReportsProgress = true;
            settingsBackgroundWorker.WorkerReportsProgress = true;

            backgroundWorker.RunWorkerAsync();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        public void SetAllowVisible(bool value)
        {
            allowVisible = value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setGuiEnabledFlag(false);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int vibranceIngameLevel = NvidiaVibranceProxy.NVAPI_MAX_LEVEL,
                vibranceWindowsLevel = NvidiaVibranceProxy.NVAPI_DEFAULT_LEVEL,
                refreshRate = 5000;
            bool keepActive = false, affectPrimaryMonitorOnly = false;

            while (!IsHandleCreated)
            {
                Thread.Sleep(500);
            }

            if (InvokeRequired)
            {
                Invoke(
                    (MethodInvoker)
                        delegate
                        {
                            readVibranceSettings(out vibranceIngameLevel, out vibranceWindowsLevel, out keepActive,
                                out refreshRate, out affectPrimaryMonitorOnly);
                        });
            }
            else
            {
                readVibranceSettings(out vibranceIngameLevel, out vibranceWindowsLevel, out keepActive, out refreshRate,
                    out affectPrimaryMonitorOnly);
            }

            v = new NvidiaVibranceProxy(silenced);
            if (v.vibranceInfo.isInitialized)
            {
                backgroundWorker.ReportProgress(1);

                setGuiEnabledFlag(true);

                v.setShouldRun(true);
                v.setKeepActive(keepActive);
                v.setVibranceIngameLevel(vibranceIngameLevel);
                v.setVibranceWindowsLevel(vibranceWindowsLevel);
                v.setSleepInterval(refreshRate);
                v.setAffectPrimaryMonitorOnly(affectPrimaryMonitorOnly);
                v.handleDVC();
                var unload = v.unloadLibraryEx();

                backgroundWorker.ReportProgress(2, unload);
                resetEvent.Set();
                Application.DoEvents();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (v != null && v.vibranceInfo.isInitialized)
            {
                setGuiEnabledFlag(true);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cleanUp();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void trackBarIngameLevel_Scroll(object sender, EventArgs e)
        {
            var setting = NvidiaSettingsWrapper.find(trackBarIngameLevel.Value);
            if (setting == null)
                return;
            v.setVibranceIngameLevel(trackBarIngameLevel.Value);
            labelIngameLevel.Text = setting.getPercentage;
            if (!settingsBackgroundWorker.IsBusy)
            {
                settingsBackgroundWorker.RunWorkerAsync();
            }
        }

        private void trackBarWindowsLevel_Scroll(object sender, EventArgs e)
        {
            var setting = NvidiaSettingsWrapper.find(trackBarWindowsLevel.Value);
            if (setting == null)
                return;
            v.setVibranceWindowsLevel(trackBarWindowsLevel.Value);
            labelWindowsLevel.Text = setting.getPercentage;
            if (!settingsBackgroundWorker.IsBusy)
            {
                settingsBackgroundWorker.RunWorkerAsync();
            }
        }

        private void settingsBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
            int ingameLevel = 0, windowsLevel = 0, refreshRate = 0;
            bool keepActive = false, affectPrimaryMonitorOnly = false;
            Invoke((MethodInvoker) delegate
            {
                ingameLevel = trackBarIngameLevel.Value;
                windowsLevel = trackBarWindowsLevel.Value;
                keepActive = checkBoxKeepActive.Checked;
                refreshRate = 5000;
                affectPrimaryMonitorOnly = checkBoxPrimaryMonitorOnly.Checked;
            });
            saveVibranceSettings(ingameLevel, windowsLevel, keepActive, refreshRate, affectPrimaryMonitorOnly);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                listBoxLog.Items.Add("vibranceInfo.isInitialized: " + v.vibranceInfo.isInitialized);
                listBoxLog.Items.Add("vibranceInfo.szGpuName: " + v.vibranceInfo.szGpuName);
                listBoxLog.Items.Add("vibranceInfo.activeOutput: " + v.vibranceInfo.activeOutput);
                listBoxLog.Items.Add("vibranceInfo.defaultHandle: " + v.vibranceInfo.defaultHandle);

                listBoxLog.Items.Add("vibranceInfo.userVibranceSettingActive: " +
                                     v.vibranceInfo.userVibranceSettingActive);
                listBoxLog.Items.Add("vibranceInfo.userVibranceSettingDefault: " +
                                     v.vibranceInfo.userVibranceSettingDefault);
                listBoxLog.Items.Add("");
                listBoxLog.Items.Add("");
                statusLabel.Text = "Running!";
                statusLabel.ForeColor = Color.Green;
            }
            else if (e.ProgressPercentage == 2)
            {
                listBoxLog.Items.Add("NVAPI Unloaded: " + e.UserState);
            }
        }

        private void settingsBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            //         allowVisible = true;
            //         this.Show();

            //this.WindowState = FormWindowState.Normal;
            //this.Visible = true;

            //this.Refresh();
            //this.ShowInTaskbar = true;
        }

        private void checkBoxKeepActive_CheckedChanged(object sender, EventArgs e)
        {
            if (v != null)
            {
                v.setKeepActive(checkBoxKeepActive.Checked);
                if (!settingsBackgroundWorker.IsBusy)
                {
                    settingsBackgroundWorker.RunWorkerAsync();
                }
                if (checkBoxKeepActive.Checked)
                    listBoxLog.Items.Add("Vibrance stays at ingame level when tabbed out.");
            }
        }

        private void checkBoxPrimaryMonitorOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (v != null)
            {
                v.setAffectPrimaryMonitorOnly(checkBoxPrimaryMonitorOnly.Checked);
                if (!settingsBackgroundWorker.IsBusy)
                {
                    settingsBackgroundWorker.RunWorkerAsync();
                }
                if (checkBoxPrimaryMonitorOnly.Checked)
                    listBoxLog.Items.Add("VibranceGUI will only affect your primary monitor now.");
            }
        }

        private void checkBoxAutostart_CheckedChanged(object sender, EventArgs e)
        {
            var autostartController = new RegistryController();
            if (checkBoxAutostart.Checked)
            {
                var pathToExe = "\"" + Application.ExecutablePath + "\" -minimized";
                if (!autostartController.isProgramRegistered(appName))
                {
                    if (autostartController.registerProgram(appName, pathToExe))
                        listBoxLog.Items.Add("Registered to Autostart!");
                    else
                        listBoxLog.Items.Add("Registering to Autostart failed!");
                }
                else if (!autostartController.isStartupPathUnchanged(appName, pathToExe))
                {
                    if (autostartController.registerProgram(appName, pathToExe))
                        listBoxLog.Items.Add("Updated Autostart Path!");
                    else
                        listBoxLog.Items.Add("Updating Autostart Path failed!");
                }
            }
            else
            {
                if (autostartController.unregisterProgram(appName))
                    listBoxLog.Items.Add("Unregistered from Autostart!");
                else
                    listBoxLog.Items.Add("Unregistering from Autostart failed!");
            }
        }

        private void twitterToolStripTextBox_Click(object sender, EventArgs e)
        {
            Process.Start(twitterLink);
        }

        private void linkLabelTwitter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(twitterLink);
        }

        private void setGuiEnabledFlag(bool flag)
        {
            Invoke((MethodInvoker) delegate
            {
                this.checkBoxKeepActive.Enabled = flag;
                this.trackBarWindowsLevel.Enabled = flag;
                this.trackBarIngameLevel.Enabled = flag;
                this.checkBoxAutostart.Enabled = flag;
                this.checkBoxPrimaryMonitorOnly.Enabled = flag;
                //this.checkBoxMonitors.Enabled = flag;
            });
        }

        private void cleanUp()
        {
            try
            {
                statusLabel.Text = "Closing...";
                statusLabel.ForeColor = Color.Red;
                Update();
                listBoxLog.Items.Add("Initiating observer thread exit... ");
                if (v != null && v.vibranceInfo.isInitialized)
                {
                    v.setShouldRun(false);
                    resetEvent.WaitOne();
                    listBoxLog.Items.Add("Unloading NVAPI... ");
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        public static void Log(Exception ex)
        {
            using (var w = File.AppendText("vibranceGUI_log.txt"))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("Exception Found:\nType: {0}", ex.GetType().FullName);
                w.WriteLine("Message: {0}", ex.Message);
                w.WriteLine("Source: {0}", ex.Source);
                w.WriteLine("Stacktrace: {0}", ex.StackTrace);
                w.WriteLine("Exception String: {0}", ex);

                w.WriteLine("-------------------------------");
            }
        }

        private void readVibranceSettings(out int vibranceIngameLevel, out int vibranceWindowsLevel, out bool keepActive,
            out int refreshRate, out bool affectPrimaryMonitorOnly)
        {
            registryController = new RegistryController();
            checkBoxAutostart.Checked = registryController.isProgramRegistered(appName);

            var settingsController = new SettingsController();
            settingsController.readVibranceSettings(GraphicsAdapter.NVIDIA, out vibranceIngameLevel,
                out vibranceWindowsLevel, out keepActive, out refreshRate, out affectPrimaryMonitorOnly);

            if (IsHandleCreated)
            {
                //no null check needed, SettingsController will always return matching values.
                labelWindowsLevel.Text = NvidiaSettingsWrapper.find(vibranceWindowsLevel).getPercentage;
                labelIngameLevel.Text = NvidiaSettingsWrapper.find(vibranceIngameLevel).getPercentage;

                trackBarWindowsLevel.Value = vibranceWindowsLevel;
                trackBarIngameLevel.Value = vibranceIngameLevel;
                checkBoxKeepActive.Checked = keepActive;
                checkBoxPrimaryMonitorOnly.Checked = affectPrimaryMonitorOnly;
            }
        }

        private void saveVibranceSettings(int ingameLevel, int windowsLevel, bool keepActive, int refreshRate,
            bool affectPrimaryMonitorOnly)
        {
            var settingsController = new SettingsController();

            settingsController.setVibranceSettings(
                ingameLevel.ToString(),
                windowsLevel.ToString(),
                keepActive.ToString(),
                refreshRate.ToString(),
                affectPrimaryMonitorOnly.ToString()
                );
        }

        private void buttonPaypal_Click(object sender, EventArgs e)
        {
            Process.Start(paypalDonationLink);
        }

        private void ShowWindow()
        {
            allowVisible = true;
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Show();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowWindow();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }
    }
}