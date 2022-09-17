namespace System.Compiler
{
    using System;

    internal class Invariant : Method
    {
        public Expression Condition;
        public int ILOffset;
        public Literal SourceConditionText;
        public Literal UserMessage;
        public bool UsesModels;

        public Invariant(TypeNode declaringType, Expression invariant, string name)
        {
            base.NodeType = NodeType.Invariant;
            this.DeclaringType = declaringType;
            this.Condition = invariant;
            if (name == null)
            {
                name = "ObjectInvariant";
            }
            base.Name = Identifier.For(name);
            base.ReturnType = new TypeExpression(new Literal(TypeCode.Boolean), 0);
        }
    }
}

