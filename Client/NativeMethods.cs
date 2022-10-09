using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Web.Management.PHP
{
    static class ShowSelectedInExplorer
    {
        [Flags]
        enum SHCONT : ushort
        {
            SHCONTF_CHECKING_FOR_CHILDREN = 0x0010,
            SHCONTF_FOLDERS = 0x0020,
            SHCONTF_NONFOLDERS = 0x0040,
            SHCONTF_INCLUDEHIDDEN = 0x0080,
            SHCONTF_INIT_ON_FIRST_NEXT = 0x0100,
            SHCONTF_NETPRINTERSRCH = 0x0200,
            SHCONTF_SHAREABLE = 0x0400,
            SHCONTF_STORAGE = 0x0800,
            SHCONTF_NAVIGATION_ENUM = 0x1000,
            SHCONTF_FASTITEMS = 0x2000,
            SHCONTF_FLATLIST = 0x4000,
            SHCONTF_ENABLE_ASYNC = 0x8000
        }

        [ComImport,
        Guid("000214E6-0000-0000-C000-000000000046"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        ComConversionLoss]
        interface IShellFolder
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void ParseDisplayName(IntPtr hwnd, [In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, [Out] out uint pchEaten, [Out] out IntPtr ppidl, [In, Out] ref uint pdwAttributes);
            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int EnumObjects([In] IntPtr hwnd, [In] SHCONT grfFlags, [MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenumIDList);

            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int BindToObject([In] IntPtr pidl, [In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out IShellFolder ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void BindToStorage([In] ref IntPtr pidl, [In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In] ref Guid riid, out IntPtr ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void CompareIDs([In] IntPtr lParam, [In] ref IntPtr pidl1, [In] ref IntPtr pidl2);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void CreateViewObject([In] IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetAttributesOf([In] uint cidl, [In] IntPtr apidl, [In, Out] ref uint rgfInOut);


            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetUIObjectOf([In] IntPtr hwndOwner, [In] uint cidl, [In] IntPtr apidl, [In] ref Guid riid, [In, Out] ref uint rgfReserved, out IntPtr ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetDisplayNameOf([In] ref IntPtr pidl, [In] uint uFlags, out IntPtr pName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetNameOf([In] IntPtr hwnd, [In] ref IntPtr pidl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName, [In] uint uFlags, [Out] IntPtr ppidlOut);
        }

        [ComImport,
        Guid("000214F2-0000-0000-C000-000000000046"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IEnumIDList
        {
            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int Next(uint celt, IntPtr rgelt, out uint pceltFetched);

            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int Skip([In] uint celt);

            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int Reset();

            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            int Clone([MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenum);
        }

        static class NativeMethods
        {
            [DllImport("shell32.dll", EntryPoint = "SHGetDesktopFolder", CharSet = CharSet.Unicode,
                SetLastError = true)]
            static extern int SHGetDesktopFolder_([MarshalAs(UnmanagedType.Interface)] out IShellFolder ppshf);

            public static IShellFolder SHGetDesktopFolder()
            {
                IShellFolder result;
                Marshal.ThrowExceptionForHR(SHGetDesktopFolder_(out result));
                return result;
            }

            [DllImport("shell32.dll", EntryPoint = "SHOpenFolderAndSelectItems")]
            static extern int SHOpenFolderAndSelectItems_(
                [In] IntPtr pidlFolder, uint cidl, [In, Optional, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
                int dwFlags);

            public static void SHOpenFolderAndSelectItems(IntPtr pidlFolder, IntPtr[] apidl, int dwFlags)
            {
                var cidl = (apidl != null) ? (uint)apidl.Length : 0U;
                var result = SHOpenFolderAndSelectItems_(pidlFolder, cidl, apidl, dwFlags);
                Marshal.ThrowExceptionForHR(result);
            }

            [DllImport("shell32.dll")]
            public static extern void ILFree([In] IntPtr pidl);
        }

        static IntPtr GetShellFolderChildrenRelativePIDL(IShellFolder parentFolder, string displayName)
        {
            uint pchEaten;
            uint pdwAttributes = 0;
            IntPtr ppidl;
            parentFolder.ParseDisplayName(IntPtr.Zero, null, displayName, out pchEaten, out ppidl, ref pdwAttributes);

            return ppidl;
        }

        static IntPtr PathToAbsolutePIDL(string path)
        {
            var desktopFolder = NativeMethods.SHGetDesktopFolder();
            return GetShellFolderChildrenRelativePIDL(desktopFolder, path);
        }

        static Guid IID_IShellFolder = typeof(IShellFolder).GUID;

        static IShellFolder PIDLToShellFolder(IShellFolder parent, IntPtr pidl)
        {
            IShellFolder folder;
            var result = parent.BindToObject(pidl, null, ref IID_IShellFolder, out folder);
            Marshal.ThrowExceptionForHR((int)result);
            return folder;
        }

        static IShellFolder PIDLToShellFolder(IntPtr pidl)
        {
            return PIDLToShellFolder(NativeMethods.SHGetDesktopFolder(), pidl);
        }

        static void SHOpenFolderAndSelectItems(IntPtr pidlFolder, IntPtr[] apidl, bool edit)
        {
            NativeMethods.SHOpenFolderAndSelectItems(pidlFolder, apidl, edit ? 1 : 0);
        }

        public static void FileOrFolder(string path, bool edit = false)
        {
            if (path == null) throw new ArgumentNullException("path");

            var pidl = PathToAbsolutePIDL(path);
            try
            {
                SHOpenFolderAndSelectItems(pidl, null, edit);
            }
            finally
            {
                NativeMethods.ILFree(pidl);
            }
        }
    }
}
