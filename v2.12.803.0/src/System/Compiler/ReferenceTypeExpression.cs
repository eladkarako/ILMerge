namespace System.Compiler
{
    using System;

    internal class ReferenceTypeExpression : Reference, ISymbolicTypeReference
    {
        public ReferenceTypeExpression(TypeNode elementType) : base(elementType)
        {
            base.NodeType = NodeType.ReferenceTypeExpression;
        }

        public ReferenceTypeExpression(TypeNode elementType, SourceContext sctx) : base(elementType)
        {
            base.NodeType = NodeType.ReferenceTypeExpression;
            base.SourceContext = sctx;
        }
    }
}

