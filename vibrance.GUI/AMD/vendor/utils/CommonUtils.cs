#region

using System;
using System.IO;
using System.Reflection;

#endregion

namespace gui.app.utils
{
    public static class CommonUtils
    {
        public static string GetVibrance_GUI_AppDataPath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vibranceGUI");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static string LoadUnmanagedLibraryFromResource(Assembly assembly,
            string libraryResourceName,
            string libraryName)
        {
            string tempDllPath;
            using (var s = assembly.GetManifestResourceStream(libraryResourceName))
            {
                var data = new BinaryReader(s).ReadBytes((int) s.Length);

                tempDllPath = Path.Combine(GetVibrance_GUI_AppDataPath(), libraryName);
                File.WriteAllBytes(tempDllPath, data);
            }

            NativeMethods.LoadLibrary(libraryName);
            return tempDllPath;
        }
    }
}