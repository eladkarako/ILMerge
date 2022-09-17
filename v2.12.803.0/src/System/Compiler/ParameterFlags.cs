namespace System.Compiler
{
    using System;

    [Flags]
    internal enum ParameterFlags
    {
        HasDefault = 0x1000,
        HasFieldMarshal = 0x2000,
        In = 1,
        None = 0,
        Optional = 0x10,
        Out = 2,
        ParameterNameMissing = 0x4000,
        ReservedMask = 0xf000
    }
}

