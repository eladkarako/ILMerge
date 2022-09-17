namespace System.Compiler
{
    using System;

    internal class BoxedTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ElementType;

        public BoxedTypeExpression(TypeNode elementType) : base(NodeType.BoxedTypeExpression)
        {
            this.ElementType = elementType;
        }

        public BoxedTypeExpression(TypeNode elementType, SourceContext sctx) : base(NodeType.BoxedTypeExpression)
        {
            this.ElementType = elementType;
            base.SourceContext = sctx;
        }
    }
}

