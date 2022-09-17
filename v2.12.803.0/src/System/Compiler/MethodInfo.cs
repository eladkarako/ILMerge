namespace System.Compiler
{
    using System;

    internal class MethodInfo
    {
        internal LocalList debugLocals;
        internal TrivialHashtable fixupIndex = new TrivialHashtable(0x10);
        internal TrivialHashtable<int> localVarIndex;
        internal BinaryWriter localVarSignature;
        internal int localVarSigTok;
        internal Int32List signatureLengths;
        internal Int32List signatureOffsets;
        internal NodeList statementNodes;
        internal Int32List statementOffsets;
    }
}

