namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("2DE91396-3844-3B1D-8E91-41C24FD672EA"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISymUnmanagedWriter
    {
        ISymUnmanagedDocumentWriter DefineDocument(string url, ref Guid language, ref Guid languageVendor, ref Guid documentType);
        void SetUserEntryPoint(uint entryMethod);
        void OpenMethod(uint method);
        void CloseMethod();
        uint OpenScope(uint startOffset);
        void CloseScope(uint endOffset);
        void SetScopeRange(uint scopeID, uint startOffset, uint endOffset);
        void DefineLocalVariable(string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint startOffset, uint endOffset);
        void DefineParameter(string name, uint attributes, uint sequence, uint addrKind, uint addr1, uint addr2, uint addr3);
        void DefineField(uint parent, string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint addr3);
        void DefineGlobalVariable(string name, uint attributes, uint cSig, IntPtr signature, uint addrKind, uint addr1, uint addr2, uint addr3);
        void Close();
        void SetSymAttribute(uint parent, string name, uint cData, IntPtr signature);
        void OpenNamespace(string name);
        void CloseNamespace();
        void UsingNamespace(string fullName);
        void SetMethodSourceRange(ISymUnmanagedDocumentWriter startDoc, uint startLine, uint startColumn, object endDoc, uint endLine, uint endColumn);
        void Initialize([MarshalAs(UnmanagedType.IUnknown)] object emitter, string filename, [MarshalAs(UnmanagedType.IUnknown)] object pIStream, bool fFullBuild);
        void GetDebugInfo(ref ImageDebugDirectory pIDD, uint cData, out uint pcData, IntPtr data);
        void DefineSequencePoints(ISymUnmanagedDocumentWriter document, uint spCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] offsets, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] lines, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] columns, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] endLines, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] endColumns);
        void RemapToken(uint oldToken, uint newToken);
        void Initialize2([MarshalAs(UnmanagedType.IUnknown)] object emitter, string tempfilename, [MarshalAs(UnmanagedType.IUnknown)] object pIStream, bool fFullBuild, string finalfilename);
        void DefineConstant(string name, object value, uint cSig, IntPtr signature);
    }
}

