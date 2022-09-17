namespace System.Compiler
{
    using System;

    internal class LRExpression : System.Compiler.Expression
    {
        public System.Compiler.Expression Expression;
        public ExpressionList SubexpressionsToEvaluateOnce;
        public LocalList Temporaries;

        public LRExpression(System.Compiler.Expression expression) : base(NodeType.LRExpression)
        {
            this.Expression = expression;
            this.Type = expression.Type;
        }
    }
}

