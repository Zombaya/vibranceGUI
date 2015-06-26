#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace vibrance.GUI
{
    internal static class Program
    {
        private const string errorGraphicsAdapterUnknown =
            "Failed to determine your Graphic Adapter type (NVIDIA/AMD). Please contact @juvlarN at twitter. Press Yes to open twitter in your browser now. Error: ";

        private const string messageBoxCaption = "vibranceGUI Error";

        [STAThread]
        private static void Main(string[] args)
        {
            var result = false;
            var mutex = new Mutex(true, "vibranceGUI~Mutex", out result);
            if (!result)
            {
                MessageBox.Show("You can run vibranceGUI only once at a time!", messageBoxCaption, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var adapter = GraphicsAdapterHelper.getAdapter();
            Form vibranceGUI = null;

            if (adapter == GraphicsAdapter.AMD)
            {
                vibranceGUI = new AmdVibranceGUI();
            }
            else if (adapter == GraphicsAdapter.NVIDIA)
            {
                vibranceGUI = new NvidiaVibranceGUI();
            }
            else if (adapter == GraphicsAdapter.UNKNOWN)
            {
                var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                if (MessageBox.Show(errorGraphicsAdapterUnknown + errorMessage,
                    messageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    Process.Start("https://twitter.com/juvlarN");
                }
                return;
            }
            vibranceGUI.WindowState = FormWindowState.Minimized;
            vibranceGUI.ShowInTaskbar = false;
            if (args.Contains("-minimized"))
            {
                vibranceGUI.WindowState = FormWindowState.Minimized;
                vibranceGUI.ShowInTaskbar = false;
                if (vibranceGUI is AmdVibranceGUI)
                {
                    ((AmdVibranceGUI) vibranceGUI).SetAllowVisible(false);
                }
                else
                {
                    ((NvidiaVibranceGUI) vibranceGUI).SetAllowVisible(false);
                }
            }
            Application.Run(vibranceGUI);

            GC.KeepAlive(mutex);
        }
    }
}