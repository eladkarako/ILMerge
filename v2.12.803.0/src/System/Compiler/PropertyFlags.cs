namespace System.Compiler
{
    using System;

    [Flags]
    internal enum PropertyFlags
    {
        Extend = 0x1000000,
        None = 0,
        ReservedMask = 0xf400,
        RTSpecialName = 0x400,
        SpecialName = 0x200
    }
}

