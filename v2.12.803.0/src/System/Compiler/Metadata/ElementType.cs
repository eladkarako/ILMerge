namespace System.Compiler.Metadata
{
    using System;

    internal enum ElementType
    {
        Array = 20,
        Boolean = 2,
        BoxedEnum = 0x51,
        Char = 3,
        Class = 0x12,
        Double = 13,
        DynamicallyTypedReference = 0x16,
        End = 0,
        Enum = 0x55,
        FunctionPointer = 0x1b,
        GenericTypeInstance = 0x15,
        Int16 = 6,
        Int32 = 8,
        Int64 = 10,
        Int8 = 4,
        Internal = 0x21,
        IntPtr = 0x18,
        MethodParameter = 30,
        Modifier = 0x40,
        Object = 0x1c,
        OptionalModifier = 0x20,
        Pinned = 0x45,
        Pointer = 15,
        Reference = 0x10,
        RequiredModifier = 0x1f,
        Sentinel = 0x41,
        Single = 12,
        String = 14,
        SzArray = 0x1d,
        Type = 80,
        TypeParameter = 0x13,
        UInt16 = 7,
        UInt32 = 9,
        UInt64 = 11,
        UInt8 = 5,
        UIntPtr = 0x19,
        ValueType = 0x11,
        Void = 1
    }
}

