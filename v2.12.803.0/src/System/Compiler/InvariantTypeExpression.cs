namespace System.Compiler
{
    using System;

    internal class InvariantTypeExpression : TypeNode, ISymbolicTypeReference
    {
        public TypeNode ElementType;

        public InvariantTypeExpression(TypeNode elementType) : base(NodeType.InvariantTypeExpression)
        {
            this.ElementType = elementType;
        }

        public InvariantTypeExpression(TypeNode elementType, SourceContext sctx) : base(NodeType.InvariantTypeExpression)
        {
            this.ElementType = elementType;
            base.SourceContext = sctx;
        }
    }
}

