namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("AA544D42-28CB-11d3-BD22-0000F80849BD")]
    internal interface ISymUnmanagedBinder
    {
        [PreserveSig]
        int GetReaderForFile([MarshalAs(UnmanagedType.IUnknown)] object importer, string filename, string searchPath, out ISymUnmanagedReader reader);
        ISymUnmanagedReader GetReaderForStream([MarshalAs(UnmanagedType.IUnknown)] object importer, [MarshalAs(UnmanagedType.IUnknown)] object stream);
    }
}

