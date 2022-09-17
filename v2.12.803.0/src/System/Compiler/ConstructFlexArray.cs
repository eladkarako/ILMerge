namespace System.Compiler
{
    using System;

    internal class ConstructFlexArray : NaryExpression
    {
        public TypeNode ElementType;
        public TypeNode ElementTypeExpression;
        public ExpressionList Initializers;

        public ConstructFlexArray()
        {
            base.NodeType = NodeType.ConstructFlexArray;
        }

        public ConstructFlexArray(TypeNode elementType, ExpressionList sizes, ExpressionList initializers) : base(sizes, NodeType.ConstructFlexArray)
        {
            this.ElementType = elementType;
            base.Operands = sizes;
            this.Initializers = initializers;
        }
    }
}

