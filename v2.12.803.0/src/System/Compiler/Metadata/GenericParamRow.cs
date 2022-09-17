namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct GenericParamRow
    {
        internal int Number;
        internal int Flags;
        internal int Owner;
        internal int Name;
        internal Member GenericParameter;
    }
}

