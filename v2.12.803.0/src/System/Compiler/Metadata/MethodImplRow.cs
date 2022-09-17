namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MethodImplRow
    {
        internal int Class;
        internal int MethodBody;
        internal int MethodDeclaration;
    }
}

