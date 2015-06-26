#region

using System;
using System.Runtime.InteropServices;
using gui.app.gpucontroller.amd.adl64;

#endregion

namespace vibrance.GUI
{
    public enum GraphicsAdapter
    {
        UNKNOWN = 0,
        NVIDIA = 1,
        AMD = 2
    }

    public class GraphicsAdapterHelper
    {
        private const string nvidiaDllName = "nvapi.dll";

        private static readonly string amdDllName = Environment.Is64BitOperatingSystem
            ? ADLImport.Atiadl_FileName
            : gui.app.gpucontroller.amd.adl32.ADLImport.Atiadl_FileName;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        public static GraphicsAdapter getAdapter()
        {
            if (isAdapterAvailable(nvidiaDllName))
                return GraphicsAdapter.NVIDIA;
            if (isAdapterAvailable(amdDllName))
                return GraphicsAdapter.AMD;
            return GraphicsAdapter.UNKNOWN;
        }

        private static bool isAdapterAvailable(string dllName)
        {
            try
            {
                var pDll = LoadLibrary(dllName);
                if (pDll != IntPtr.Zero)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}