namespace System.Compiler
{
    using System;

    internal class ApplyToAll : BinaryExpression
    {
        public Local ElementLocal;
        public Method ResultIterator;

        public ApplyToAll() : base(null, null, NodeType.ApplyToAll)
        {
        }

        public ApplyToAll(Expression operand1, Expression operand2) : base(operand1, operand2, NodeType.ApplyToAll)
        {
        }

        public ApplyToAll(Expression operand1, Expression operand2, SourceContext ctx) : base(operand1, operand2, NodeType.ApplyToAll)
        {
            base.SourceContext = ctx;
        }
    }
}

