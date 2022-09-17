namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ModuleRefRow
    {
        internal int Name;
        internal System.Compiler.Module Module;
    }
}

