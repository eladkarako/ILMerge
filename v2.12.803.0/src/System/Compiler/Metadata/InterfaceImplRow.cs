namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct InterfaceImplRow
    {
        internal int Class;
        internal int Interface;
    }
}

