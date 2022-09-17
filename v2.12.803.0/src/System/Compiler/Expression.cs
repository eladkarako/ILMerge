namespace System.Compiler
{
    using System;

    internal class Expression : Node
    {
        public int ILOffset;
        private TypeNode type;

        public Expression(NodeType nodeType) : base(nodeType)
        {
        }

        public Expression(NodeType nodeType, TypeNode type) : base(nodeType)
        {
            this.type = type;
        }

        public virtual TypeNode Type
        {
            get => 
                this.type;
            set
            {
                this.type = value;
            }
        }
    }
}

