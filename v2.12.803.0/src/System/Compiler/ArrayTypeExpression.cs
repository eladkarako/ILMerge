namespace System.Compiler
{
    using System;

    internal class ArrayTypeExpression : ArrayType, ISymbolicTypeReference
    {
        public bool LowerBoundIsUnknown;

        public ArrayTypeExpression()
        {
            base.NodeType = NodeType.ArrayTypeExpression;
        }

        public ArrayTypeExpression(TypeNode elementType, int rank) : base(elementType, rank)
        {
            base.NodeType = NodeType.ArrayTypeExpression;
        }

        public ArrayTypeExpression(TypeNode elementType, int rank, int[] sizes) : base(elementType, rank, sizes)
        {
            base.NodeType = NodeType.ArrayTypeExpression;
        }

        public ArrayTypeExpression(TypeNode elementType, int rank, SourceContext sctx) : base(elementType, rank)
        {
            base.NodeType = NodeType.ArrayTypeExpression;
            base.SourceContext = sctx;
        }

        public ArrayTypeExpression(TypeNode elementType, int rank, int[] sizes, int[] lowerBounds) : base(elementType, rank, sizes, sizes)
        {
            base.NodeType = NodeType.ArrayTypeExpression;
        }

        public ArrayTypeExpression(TypeNode elementType, int rank, int[] sizes, SourceContext sctx) : base(elementType, rank, sizes)
        {
            base.NodeType = NodeType.ArrayTypeExpression;
            base.SourceContext = sctx;
        }

        public ArrayTypeExpression(TypeNode elementType, int rank, int[] sizes, int[] lowerBounds, SourceContext sctx) : base(elementType, rank, sizes, sizes)
        {
            base.NodeType = NodeType.ArrayTypeExpression;
            base.SourceContext = sctx;
        }
    }
}

