namespace System.Compiler
{
    using System;

    internal class FlexArrayTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ElementType;

        public FlexArrayTypeExpression(TypeNode elementType) : base(NodeType.FlexArrayTypeExpression)
        {
            this.ElementType = elementType;
        }

        public FlexArrayTypeExpression(TypeNode elementType, SourceContext sctx) : base(NodeType.FlexArrayTypeExpression)
        {
            this.ElementType = elementType;
            base.SourceContext = sctx;
        }
    }
}

