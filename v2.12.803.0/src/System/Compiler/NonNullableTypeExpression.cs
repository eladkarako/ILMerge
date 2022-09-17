namespace System.Compiler
{
    using System;

    internal class NonNullableTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ElementType;

        public NonNullableTypeExpression(TypeNode elementType) : base(NodeType.NonNullableTypeExpression)
        {
            this.ElementType = elementType;
        }

        public NonNullableTypeExpression(TypeNode elementType, SourceContext sctx) : base(NodeType.NonNullableTypeExpression)
        {
            this.ElementType = elementType;
            base.SourceContext = sctx;
        }
    }
}

