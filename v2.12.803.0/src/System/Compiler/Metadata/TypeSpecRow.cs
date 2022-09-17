namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TypeSpecRow
    {
        internal int Signature;
        internal TypeNode Type;
    }
}

