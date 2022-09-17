namespace System.Compiler
{
    using System;

    internal class RequiresOtherwise : Requires
    {
        public Expression ThrowException;

        public RequiresOtherwise() : base(NodeType.RequiresOtherwise)
        {
        }

        public RequiresOtherwise(Expression cond, Expression exc) : base(NodeType.RequiresOtherwise, cond)
        {
            this.ThrowException = exc;
        }
    }
}

