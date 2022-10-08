using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Shell.Common;

namespace Web.Management.PHP
{
    internal class NativeMethods
    {
        #region View Folder Information

        internal unsafe static bool OpenFile(string fileName)
        {
            ITEMIDLIST* nativeFolder;
            uint psfgaoOut;
            var folderPath = Path.GetDirectoryName(fileName);
            PInvoke.SHParseDisplayName(folderPath, null, &nativeFolder, 0, &psfgaoOut);

            if (nativeFolder == null)
            {
                // Log error, can't find folder
                return false;
            }

            ITEMIDLIST* nativeFile;
            PInvoke.SHParseDisplayName(fileName, null, &nativeFile, 0, &psfgaoOut);

            ITEMIDLIST** fileArray;
            if (nativeFile == null)
            {
                // Open the folder without the file selected if we can't find the file
                fileArray = &nativeFolder;
            }
            else
            {
                fileArray = &nativeFile;
            }

            PInvoke.SHOpenFolderAndSelectItems(nativeFolder, 1, fileArray, 0);

            Marshal.FreeCoTaskMem(new IntPtr(&nativeFolder));
            if (nativeFile != null)
            {
                Marshal.FreeCoTaskMem(new IntPtr(nativeFile));
            }

            return true;
        }

        #endregion
    }
}
