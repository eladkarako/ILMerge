namespace System.Compiler
{
    using System;

    internal class NonNullTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ElementType;

        public NonNullTypeExpression(TypeNode elementType) : base(NodeType.NonNullTypeExpression)
        {
            this.ElementType = elementType;
        }

        public NonNullTypeExpression(TypeNode elementType, SourceContext sctx) : base(NodeType.NonNullTypeExpression)
        {
            this.ElementType = elementType;
            base.SourceContext = sctx;
        }
    }
}

