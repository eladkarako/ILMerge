namespace System.Compiler
{
    using System;

    internal class PointerTypeExpression : Pointer, ISymbolicTypeReference
    {
        public PointerTypeExpression(TypeNode elementType) : base(elementType)
        {
            base.NodeType = NodeType.PointerTypeExpression;
        }

        public PointerTypeExpression(TypeNode elementType, SourceContext sctx) : base(elementType)
        {
            base.NodeType = NodeType.PointerTypeExpression;
            base.SourceContext = sctx;
        }

        public override bool IsUnmanaged =>
            true;
    }
}

