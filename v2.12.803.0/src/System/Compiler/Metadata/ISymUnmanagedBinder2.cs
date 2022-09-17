namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(false), Guid("ACCEE350-89AF-4ccb-8B40-1C2C4C6F9434"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISymUnmanagedBinder2 : ISymUnmanagedBinder
    {
        void GetReaderForFile(IntPtr importer, [MarshalAs(UnmanagedType.LPWStr)] string filename, [MarshalAs(UnmanagedType.LPWStr)] string SearchPath, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader retVal);
        void GetReaderFromStream(IntPtr importer, IntPtr stream, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader retVal);
        [PreserveSig]
        int GetReaderForFile2([MarshalAs(UnmanagedType.IUnknown)] object importer, [MarshalAs(UnmanagedType.LPWStr)] string fileName, [MarshalAs(UnmanagedType.LPWStr)] string searchPath, int searchPolicy, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader pRetVal);
    }
}

