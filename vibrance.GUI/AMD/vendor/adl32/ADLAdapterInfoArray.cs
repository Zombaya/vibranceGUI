#region

using System.Runtime.InteropServices;

#endregion

namespace gui.app.gpucontroller.amd.adl64
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfoArray
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ADL.ADL_MAX_ADAPTERS)] internal ADLAdapterInfo[] ADLAdapterInfo;
    }
}