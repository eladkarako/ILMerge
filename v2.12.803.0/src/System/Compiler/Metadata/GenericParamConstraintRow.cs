namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct GenericParamConstraintRow
    {
        internal int Param;
        internal int Constraint;
    }
}

