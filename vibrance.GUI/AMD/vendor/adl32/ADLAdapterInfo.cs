﻿#region

using System.Runtime.InteropServices;

#endregion

namespace gui.app.gpucontroller.amd.adl64
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfo
    {
        private readonly int Size;
        internal int AdapterIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)] internal string UDID;
        internal int BusNumber;
        internal int DriverNumber;
        internal int FunctionNumber;
        internal int VendorID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)] internal string AdapterName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)] internal string DisplayName;
        internal int Present;
        internal int Exist;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)] internal string DriverPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)] internal string DriverPathExt;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)] internal string PNPString;
        internal int OSDisplayIndex;
    }
}