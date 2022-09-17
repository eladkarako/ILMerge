namespace System.Compiler
{
    using System;

    internal class Assertion : Statement
    {
        public Expression Condition;
        public Literal userMessage;

        public Assertion() : base(NodeType.Assertion)
        {
        }

        public Assertion(Expression condition) : base(NodeType.Assertion)
        {
            this.Condition = condition;
        }
    }
}

