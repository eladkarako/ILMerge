namespace System.Compiler
{
    using System;

    [Flags]
    internal enum CallingConventionFlags
    {
        ArgumentConvention = 7,
        C = 1,
        Default = 0,
        ExplicitThis = 0x40,
        FastCall = 4,
        Generic = 0x10,
        HasThis = 0x20,
        StandardCall = 2,
        ThisCall = 3,
        VarArg = 5
    }
}

