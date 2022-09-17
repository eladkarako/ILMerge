namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, SuppressUnmanagedCodeSecurity, Guid("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISymUnmanagedDocumentWriter
    {
        void SetSource(uint sourceSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] byte[] source);
        void SetCheckSum(ref Guid algorithmId, uint checkSumSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] checkSum);
    }
}

