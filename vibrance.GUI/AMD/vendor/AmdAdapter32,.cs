#region

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using gui.app.gpucontroller.amd.adl32;
using vibrance.GUI.AMD.vendor;

#endregion

namespace gui.app.gpucontroller.amd32
{
    public class AmdAdapter32 : AmdAdapter
    {
        public bool IsAvailable()
        {
            if (ADL.ADL_Main_Control_Create != null)
            {
                if (ADL.ADL_SUCCESS == ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1))
                {
                    if (ADL.ADL_Main_Control_Destroy != null)
                    {
                        ADL.ADL_Main_Control_Destroy();
                    }

                    return true;
                }
            }

            return false;
        }

        public override void SetSaturationOnAllDisplays(int vibranceLevel)
        {
            SetSaturationOnDisplay(vibranceLevel, null);
        }

        public override void SetSaturationOnDisplay(int vibranceLevel, string displayName)
        {
            SetSaturation((adlDisplayInfo, adlAdapterInfo, adapterIndex) =>
            {
                var infoValue = adlDisplayInfo.DisplayID.DisplayLogicalIndex;

                var adapterIsAssociatedWithDisplay = adapterIndex == adlDisplayInfo.DisplayID.DisplayLogicalAdapterIndex;
                if (infoValue != -1 && adapterIsAssociatedWithDisplay && adlAdapterInfo.DisplayName == displayName)
                {
                    ADL.ADL_Display_Color_Set(
                        adapterIndex,
                        infoValue,
                        ADL.ADL_DISPLAY_COLOR_SATURATION,
                        vibranceLevel);
                }
            }, displayName);
        }

        private void SetSaturation(Action<ADLDisplayInfo, ADLAdapterInfo, int> handle, string displayName)
        {
            var numberOfAdapters = 0;

            ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);

            if (ADL.ADL_Adapter_NumberOfAdapters_Get != null)
            {
                ADL.ADL_Adapter_NumberOfAdapters_Get(ref numberOfAdapters);
            }

            ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);

            if (numberOfAdapters > 0)
            {
                var osAdapterInfoData = new ADLAdapterInfoArray();

                if (ADL.ADL_Adapter_AdapterInfo_Get != null)
                {
                    var size = Marshal.SizeOf(osAdapterInfoData);
                    var adapterBuffer = Marshal.AllocCoTaskMem(size);
                    Marshal.StructureToPtr(osAdapterInfoData, adapterBuffer, false);

                    var adlRet = ADL.ADL_Adapter_AdapterInfo_Get(adapterBuffer, size);
                    if (adlRet == ADL.ADL_SUCCESS)
                    {
                        osAdapterInfoData =
                            (ADLAdapterInfoArray) Marshal.PtrToStructure(adapterBuffer, osAdapterInfoData.GetType());
                        var isActive = 0;

                        for (var i = 0; i < numberOfAdapters; i++)
                        {
                            var adlAdapterInfo = osAdapterInfoData.ADLAdapterInfo[i];

                            if (adlAdapterInfo.DisplayName != displayName && displayName != null)
                            {
                                continue;
                            }

                            var adapterIndex = adlAdapterInfo.AdapterIndex;

                            if (ADL.ADL_Adapter_Active_Get != null)
                            {
                                adlRet = ADL.ADL_Adapter_Active_Get(adlAdapterInfo.AdapterIndex, ref isActive);
                            }

                            if (ADL.ADL_SUCCESS == adlRet)
                            {
                                var oneDisplayInfo = new ADLDisplayInfo();

                                if (ADL.ADL_Display_DisplayInfo_Get != null)
                                {
                                    var displayBuffer = IntPtr.Zero;

                                    var numberOfDisplays = 0;
                                    adlRet = ADL.ADL_Display_DisplayInfo_Get(adlAdapterInfo.AdapterIndex,
                                        ref numberOfDisplays, out displayBuffer, 1);
                                    if (ADL.ADL_SUCCESS == adlRet)
                                    {
                                        var displayInfoData = new List<ADLDisplayInfo>();
                                        for (var j = 0; j < numberOfDisplays; j++)
                                        {
                                            oneDisplayInfo =
                                                (ADLDisplayInfo)
                                                    Marshal.PtrToStructure(
                                                        new IntPtr(displayBuffer.ToInt64() +
                                                                   j*Marshal.SizeOf(oneDisplayInfo)),
                                                        oneDisplayInfo.GetType());
                                            displayInfoData.Add(oneDisplayInfo);
                                        }

                                        for (var j = 0; j < numberOfDisplays; j++)
                                        {
                                            var adlDisplayInfo = displayInfoData[j];

                                            handle(adlDisplayInfo, adlAdapterInfo, adapterIndex);
                                        }
                                    }

                                    if (displayBuffer != IntPtr.Zero)
                                    {
                                        Marshal.FreeCoTaskMem(displayBuffer);
                                    }
                                }
                            }
                        }
                    }

                    if (adapterBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(adapterBuffer);
                    }
                }
            }

            if (ADL.ADL_Main_Control_Destroy != null)
            {
                ADL.ADL_Main_Control_Destroy();
            }
        }
    }
}