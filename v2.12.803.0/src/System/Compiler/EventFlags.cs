namespace System.Compiler
{
    using System;

    [Flags]
    internal enum EventFlags
    {
        Extend = 0x1000000,
        None = 0,
        ReservedMask = 0x400,
        RTSpecialName = 0x400,
        SpecialName = 0x200
    }
}

