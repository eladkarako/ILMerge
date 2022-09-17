namespace System.Compiler
{
    using System;

    internal class Assumption : Statement
    {
        public Expression Condition;
        public Literal userMessage;

        public Assumption() : base(NodeType.Assumption)
        {
        }

        public Assumption(Expression condition) : base(NodeType.Assumption)
        {
            this.Condition = condition;
        }
    }
}

