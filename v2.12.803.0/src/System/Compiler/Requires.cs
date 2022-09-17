namespace System.Compiler
{
    using System;

    internal abstract class Requires : MethodContractElement
    {
        public Expression Condition;

        protected Requires() : base(NodeType.Requires)
        {
        }

        protected Requires(NodeType nodeType) : base(nodeType)
        {
        }

        protected Requires(NodeType nodeType, Expression expression) : base(nodeType)
        {
            this.Condition = expression;
        }

        public override Expression Assertion =>
            this.Condition;
    }
}

