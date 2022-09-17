namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ConstantRow
    {
        internal int Type;
        internal int Parent;
        internal int Value;
    }
}

