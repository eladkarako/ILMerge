namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct EventMapRow
    {
        internal int Parent;
        internal int EventList;
    }
}

