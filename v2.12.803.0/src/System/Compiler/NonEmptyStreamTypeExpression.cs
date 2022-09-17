namespace System.Compiler
{
    using System;

    internal class NonEmptyStreamTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ElementType;

        public NonEmptyStreamTypeExpression(TypeNode elementType) : base(NodeType.NonEmptyStreamTypeExpression)
        {
            this.ElementType = elementType;
        }

        public NonEmptyStreamTypeExpression(TypeNode elementType, SourceContext sctx) : base(NodeType.NonEmptyStreamTypeExpression)
        {
            this.ElementType = elementType;
            base.SourceContext = sctx;
        }
    }
}

