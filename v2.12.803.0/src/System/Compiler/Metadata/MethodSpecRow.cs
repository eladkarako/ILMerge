namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MethodSpecRow
    {
        internal int Method;
        internal int Instantiation;
        internal System.Compiler.Method InstantiatedMethod;
    }
}

