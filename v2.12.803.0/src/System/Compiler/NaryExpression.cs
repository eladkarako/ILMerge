namespace System.Compiler
{
    using System;

    internal abstract class NaryExpression : Expression
    {
        public ExpressionList Operands;

        protected NaryExpression() : base(NodeType.Nop)
        {
        }

        protected NaryExpression(ExpressionList operands, NodeType nodeType) : base(nodeType)
        {
            this.Operands = operands;
        }
    }
}

