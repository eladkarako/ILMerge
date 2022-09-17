namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B62B923C-B500-3158-A543-24F307A8B7E1")]
    internal interface ISymUnmanagedMethod
    {
        uint GetToken();
        uint GetSequencePointCount();
        ISymUnmanagedScope GetRootScope();
        ISymUnmanagedScope GetScopeFromOffset(uint offset);
        uint Getoffset(ISymUnmanagedDocument document, uint line, uint column);
        void GetRanges(ISymUnmanagedDocument document, uint line, uint column, uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] uint[] ranges);
        void GetParameters(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ISymUnmanagedVariable[] parms);
        IntPtr GetNamespace();
        bool GetSourceStartEnd([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0, SizeConst=2)] ISymUnmanagedDocument[] docs, [MarshalAs(UnmanagedType.LPArray)] uint[] lines, [MarshalAs(UnmanagedType.LPArray)] uint[] columns);
        void GetSequencePoints(uint size, out uint length, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] uint[] offsets, [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.IUnknown, SizeParamIndex=0)] IntPtr[] documents, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] uint[] lines, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] uint[] columns, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] uint[] endLines, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] uint[] endColumns);
    }
}

