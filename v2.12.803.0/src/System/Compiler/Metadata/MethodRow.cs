namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MethodRow
    {
        internal int RVA;
        internal int ImplFlags;
        internal int Flags;
        internal int Name;
        internal int Signature;
        internal int ParamList;
        internal System.Compiler.Method Method;
    }
}

