namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TypeRefRow
    {
        internal int ResolutionScope;
        internal int Name;
        internal int Namespace;
        internal TypeNode Type;
    }
}

