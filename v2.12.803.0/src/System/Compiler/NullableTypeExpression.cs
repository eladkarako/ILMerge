namespace System.Compiler
{
    using System;

    internal class NullableTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ElementType;

        public NullableTypeExpression(TypeNode elementType) : base(NodeType.NullableTypeExpression)
        {
            this.ElementType = elementType;
        }

        public NullableTypeExpression(TypeNode elementType, SourceContext sctx) : base(NodeType.NullableTypeExpression)
        {
            this.ElementType = elementType;
            base.SourceContext = sctx;
        }
    }
}

