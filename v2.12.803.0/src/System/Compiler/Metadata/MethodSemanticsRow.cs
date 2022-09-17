namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MethodSemanticsRow
    {
        internal int Semantics;
        internal int Method;
        internal int Association;
    }
}

