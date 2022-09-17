namespace System.Compiler
{
    using System;

    internal class NamedArgument : Expression
    {
        private bool isCustomAttributeProperty;
        private Identifier name;
        private Expression value;
        private bool valueIsBoxed;

        public NamedArgument() : base(NodeType.NamedArgument)
        {
        }

        public NamedArgument(Identifier name, Expression value) : base(NodeType.NamedArgument)
        {
            this.Name = name;
            this.Value = value;
        }

        public NamedArgument(Identifier name, Expression value, SourceContext ctx) : base(NodeType.NamedArgument)
        {
            this.Name = name;
            this.Value = value;
            base.SourceContext = ctx;
        }

        public bool IsCustomAttributeProperty
        {
            get => 
                this.isCustomAttributeProperty;
            set
            {
                this.isCustomAttributeProperty = value;
            }
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

        public Expression Value
        {
            get => 
                this.value;
            set
            {
                this.value = value;
            }
        }

        public bool ValueIsBoxed
        {
            get => 
                this.valueIsBoxed;
            set
            {
                this.valueIsBoxed = value;
            }
        }
    }
}

