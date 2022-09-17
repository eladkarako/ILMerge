namespace System.Compiler
{
    using System;

    internal class AddressDereference : Expression
    {
        private Expression address;
        private int alignment;
        private ExplicitOp explicitOperation;
        private bool isVolatile;

        public AddressDereference() : base(NodeType.AddressDereference)
        {
        }

        public AddressDereference(Expression address, TypeNode type) : this(address, type, false, -1)
        {
        }

        public AddressDereference(Expression address, TypeNode type, SourceContext sctx) : this(address, type, false, -1, sctx)
        {
        }

        public AddressDereference(Expression address, TypeNode type, bool isVolatile, int alignment) : base(NodeType.AddressDereference)
        {
            this.address = address;
            this.alignment = alignment;
            this.Type = type;
            this.isVolatile = isVolatile;
        }

        public AddressDereference(Expression address, TypeNode type, bool Volatile, int alignment, SourceContext sctx) : base(NodeType.AddressDereference)
        {
            this.address = address;
            this.alignment = alignment;
            this.Type = type;
            this.isVolatile = Volatile;
            base.SourceContext = sctx;
        }

        public Expression Address
        {
            get => 
                this.address;
            set
            {
                this.address = value;
            }
        }

        public int Alignment
        {
            get => 
                this.alignment;
            set
            {
                this.alignment = value;
            }
        }

        public bool Explicit =>
            (this.explicitOperation != ExplicitOp.None);

        public ExplicitOp ExplicitOperator
        {
            get => 
                this.explicitOperation;
            set
            {
                this.explicitOperation = value;
            }
        }

        public bool Volatile
        {
            get => 
                this.isVolatile;
            set
            {
                this.isVolatile = value;
            }
        }

        public enum ExplicitOp
        {
            None,
            Star,
            Arrow
        }
    }
}

