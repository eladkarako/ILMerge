namespace System.Compiler
{
    using System;

    internal class Fixup
    {
        internal int addressOfNextInstruction;
        internal int fixupLocation;
        internal Fixup nextFixUp;
        internal bool shortOffset;
    }
}

