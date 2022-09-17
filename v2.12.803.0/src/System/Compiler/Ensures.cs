namespace System.Compiler
{
    using System;

    internal abstract class Ensures : MethodContractElement
    {
        public Expression PostCondition;

        protected Ensures() : base(NodeType.Ensures)
        {
        }

        protected Ensures(NodeType nodeType) : base(nodeType)
        {
        }

        protected Ensures(NodeType nodeType, Expression expression) : base(nodeType)
        {
            this.PostCondition = expression;
        }

        public override Expression Assertion =>
            this.PostCondition;
    }
}

