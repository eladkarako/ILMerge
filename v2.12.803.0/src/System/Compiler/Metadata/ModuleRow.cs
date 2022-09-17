namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ModuleRow
    {
        internal int Generation;
        internal int Name;
        internal int Mvid;
        internal int EncId;
        internal int EncBaseId;
    }
}

