﻿#region

using System.Runtime.InteropServices;

#endregion

namespace gui.app.gpucontroller.amd.adl64
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayID
    {
        internal int DisplayLogicalIndex;
        internal int DisplayPhysicalIndex;
        internal int DisplayLogicalAdapterIndex;
        internal int DisplayPhysicalAdapterIndex;
    }
}