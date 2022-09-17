namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemberRefRow
    {
        internal int Class;
        internal int Name;
        internal int Signature;
        internal System.Compiler.Member Member;
        internal TypeNodeList VarargTypes;
    }
}

