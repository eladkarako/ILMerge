namespace System.Compiler
{
    using System;

    [Flags]
    internal enum PInvokeFlags
    {
        BestFitDisabled = 0x20,
        BestFitEnabled = 0x10,
        BestFitMask = 0x30,
        BestFitUseAsm = 0,
        CallConvCdecl = 0x200,
        CallConvFastcall = 0x500,
        CallConvStdcall = 0x300,
        CallConvThiscall = 0x400,
        CallConvWinapi = 0x100,
        CallingConvMask = 0x700,
        CharSetAns = 2,
        CharSetAuto = 6,
        CharSetMask = 6,
        CharSetNotSpec = 0,
        CharSetUnicode = 4,
        NoMangle = 1,
        None = 0,
        SupportsLastError = 0x40,
        ThrowOnUnmappableCharDisabled = 0x2000,
        ThrowOnUnmappableCharEnabled = 0x1000,
        ThrowOnUnmappableCharMask = 0x3000,
        ThrowOnUnmappableCharUseAsm = 0
    }
}

