#region

using System;

#endregion

namespace gui.app.gpucontroller.amd.adl64
{
    public class ADLCheckLibrary
    {
        private static readonly ADLCheckLibrary ADLCheckLibrary_ = new ADLCheckLibrary();
        private readonly IntPtr ADLLibrary = IntPtr.Zero;

        private ADLCheckLibrary()
        {
            try
            {
                if (1 == ADLImport.ADL_Main_Control_IsFunctionValid(IntPtr.Zero, "ADL_Main_Control_Create"))
                {
                    ADLLibrary = ADLImport.GetModuleHandle(ADLImport.Atiadl_FileName);
                }
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
            catch (Exception)
            {
            }
        }

        ~ADLCheckLibrary()
        {
            if (IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
            {
                ADLImport.ADL_Main_Control_Destroy();
            }
        }

        public static bool IsFunctionValid(string functionName)
        {
            var result = false;
            if (IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
            {
                if (1 == ADLImport.ADL_Main_Control_IsFunctionValid(ADLCheckLibrary_.ADLLibrary, functionName))
                {
                    result = true;
                }
            }
            return result;
        }

        public static IntPtr GetProcAddress(string functionName)
        {
            var result = IntPtr.Zero;
            if (IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
            {
                result = ADLImport.ADL_Main_Control_GetProcAddress(ADLCheckLibrary_.ADLLibrary, functionName);
            }
            return result;
        }
    }
}