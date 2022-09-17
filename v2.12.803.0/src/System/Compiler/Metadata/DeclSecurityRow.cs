namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DeclSecurityRow
    {
        internal int Action;
        internal int Parent;
        internal int PermissionSet;
    }
}

