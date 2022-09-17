namespace System.Compiler
{
    using System;

    internal class ConstructTuple : Expression
    {
        public FieldList Fields;

        public ConstructTuple() : base(NodeType.ConstructTuple)
        {
        }
    }
}

