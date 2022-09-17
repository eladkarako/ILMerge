namespace System.Compiler
{
    using System;

    internal class Variable : Expression
    {
        private Identifier name;
        public TypeNode TypeExpression;

        public Variable(NodeType type) : base(type)
        {
        }

        public Identifier Name
        {
            get => 
                this.name;
            set
            {
                this.name = value;
            }
        }
    }
}

