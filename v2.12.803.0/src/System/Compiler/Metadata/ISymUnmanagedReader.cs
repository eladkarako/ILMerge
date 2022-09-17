namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISymUnmanagedReader
    {
        ISymUnmanagedDocument GetDocument(string url, ref Guid language, ref Guid languageVendor, ref Guid documentType);
        void GetDocuments(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ISymUnmanagedDocument[] docs);
        uint GetUserEntryPoint();
        [PreserveSig]
        int GetMethod(uint token, ref ISymUnmanagedMethod method);
        ISymUnmanagedMethod GetMethodByVersion(uint token, int version);
        void GetVariables(uint parent, uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] ISymUnmanagedVariable[] vars);
        void GetGlobalVariables(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ISymUnmanagedVariable[] vars);
        ISymUnmanagedMethod GetMethodFromDocumentPosition(ISymUnmanagedDocument document, uint line, uint column);
        void GetSymAttribute(uint parent, string name, ulong size, ref uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] buffer);
        void GetNamespaces(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] IntPtr[] namespaces);
        void Initialize([MarshalAs(UnmanagedType.IUnknown)] object importer, string filename, string searchPath, [MarshalAs(UnmanagedType.IUnknown)] object stream);
        void UpdateSymbolStore(string filename, [MarshalAs(UnmanagedType.IUnknown)] object stream);
        void ReplaceSymbolStore(string filename, [MarshalAs(UnmanagedType.IUnknown)] object stream);
        void GetSymbolStoreFileName(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] char[] name);
        void GetMethodsFromDocumentPosition(ISymUnmanagedDocument document, uint line, uint column, uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] ISymUnmanagedMethod[] retval);
        void GetDocumentVersion(ISymUnmanagedDocument doc, out int version, out bool isLatest);
        void GetMethodVersion(ISymUnmanagedMethod method, out int version);
    }
}

