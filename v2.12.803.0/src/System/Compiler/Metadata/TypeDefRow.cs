namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TypeDefRow
    {
        internal int Flags;
        internal int Name;
        internal int Namespace;
        internal int Extends;
        internal int FieldList;
        internal int MethodList;
        internal TypeNode Type;
        internal Identifier NamespaceId;
        internal int NamespaceKey;
        internal int NameKey;
    }
}

