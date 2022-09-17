namespace System.Compiler
{
    using System;

    internal class EnsuresExceptional : Ensures
    {
        public TypeNode Type;
        public TypeNode TypeExpression;
        public Expression Variable;

        public EnsuresExceptional() : base(NodeType.EnsuresExceptional)
        {
        }

        public EnsuresExceptional(Expression expression) : base(NodeType.EnsuresExceptional, expression)
        {
        }
    }
}

