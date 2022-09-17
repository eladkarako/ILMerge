namespace System.Compiler
{
    using System;

    internal class EnsuresNormal : Ensures
    {
        public EnsuresNormal() : base(NodeType.EnsuresNormal)
        {
        }

        public EnsuresNormal(Expression expression) : base(NodeType.EnsuresNormal, expression)
        {
        }
    }
}

