namespace System.Compiler
{
    using System;

    internal class ConstructIterator : Expression
    {
        public Block Body;
        public TypeNode ElementType;
        public Class State;

        public ConstructIterator() : base(NodeType.ConstructIterator)
        {
        }

        public ConstructIterator(Class state, Block body, TypeNode elementType, TypeNode type) : base(NodeType.ConstructIterator)
        {
            this.State = state;
            this.Body = body;
            this.ElementType = elementType;
            this.Type = type;
        }
    }
}

