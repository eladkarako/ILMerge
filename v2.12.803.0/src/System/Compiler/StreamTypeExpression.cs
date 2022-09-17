namespace System.Compiler
{
    using System;

    internal class StreamTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ElementType;

        public StreamTypeExpression(TypeNode elementType) : base(NodeType.StreamTypeExpression)
        {
            this.ElementType = elementType;
        }

        public StreamTypeExpression(TypeNode elementType, SourceContext sctx) : base(NodeType.StreamTypeExpression)
        {
            this.ElementType = elementType;
            base.SourceContext = sctx;
        }
    }
}

