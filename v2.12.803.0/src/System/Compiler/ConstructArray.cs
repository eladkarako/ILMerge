namespace System.Compiler
{
    using System;

    internal class ConstructArray : NaryExpression
    {
        private TypeNode elementType;
        public TypeNode ElementTypeExpression;
        public ExpressionList Initializers;
        public Expression Owner;
        private int rank;

        public ConstructArray()
        {
            base.NodeType = NodeType.ConstructArray;
            this.rank = 1;
        }

        public ConstructArray(TypeNode elementType, ExpressionList initializers) : base(null, NodeType.ConstructArray)
        {
            this.elementType = elementType;
            this.Initializers = initializers;
            this.rank = 1;
            if (elementType != null)
            {
                this.Type = elementType.GetArrayType(1);
            }
        }

        public ConstructArray(TypeNode elementType, ExpressionList sizes, ExpressionList initializers) : base(sizes, NodeType.ConstructArray)
        {
            this.elementType = elementType;
            base.Operands = sizes;
            this.rank = (sizes == null) ? 1 : sizes.Count;
            this.Initializers = initializers;
        }

        public ConstructArray(TypeNode elementType, int rank, ExpressionList initializers) : base(null, NodeType.ConstructArray)
        {
            this.elementType = elementType;
            this.Initializers = initializers;
            this.rank = rank;
            if (elementType != null)
            {
                this.Type = elementType.GetArrayType(1);
            }
        }

        public TypeNode ElementType
        {
            get => 
                this.elementType;
            set
            {
                this.elementType = value;
            }
        }

        public int Rank
        {
            get => 
                this.rank;
            set
            {
                this.rank = value;
            }
        }
    }
}

