namespace System.Compiler
{
    using System;

    internal class PostfixExpression : System.Compiler.Expression
    {
        public System.Compiler.Expression Expression;
        public NodeType Operator;
        public Method OperatorOverload;

        public PostfixExpression() : base(NodeType.PostfixExpression)
        {
        }

        public PostfixExpression(System.Compiler.Expression expression, NodeType Operator, SourceContext sourceContext) : base(NodeType.PostfixExpression)
        {
            this.Expression = expression;
            this.Operator = Operator;
            base.SourceContext = sourceContext;
        }
    }
}

