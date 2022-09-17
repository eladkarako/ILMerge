namespace System.Compiler
{
    using System;

    internal class RequiresPlain : Requires
    {
        public TypeNode ExceptionType;
        public bool IsFromValidation;

        public RequiresPlain() : base(NodeType.RequiresPlain)
        {
        }

        public RequiresPlain(Expression expression) : base(NodeType.RequiresPlain, expression)
        {
        }

        public RequiresPlain(Expression expression, TypeNode texception) : base(NodeType.RequiresPlain, expression)
        {
            this.ExceptionType = texception;
        }

        public virtual bool IsWithException =>
            (this.ExceptionType != null);
    }
}

