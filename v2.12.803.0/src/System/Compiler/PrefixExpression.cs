namespace System.Compiler
{
    using System;

    internal class PrefixExpression : System.Compiler.Expression
    {
        public System.Compiler.Expression Expression;
        public NodeType Operator;
        public Method OperatorOverload;

        public PrefixExpression() : base(NodeType.PrefixExpression)
        {
        }

        public PrefixExpression(System.Compiler.Expression expression, NodeType Operator, SourceContext sourceContext) : base(NodeType.PrefixExpression)
        {
            this.Expression = expression;
            this.Operator = Operator;
            base.SourceContext = sourceContext;
        }
    }
}

