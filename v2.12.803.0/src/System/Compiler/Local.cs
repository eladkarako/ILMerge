namespace System.Compiler
{
    using System;

    internal class Local : Variable
    {
        public uint Attributes;
        public Block DeclaringBlock;
        public bool HasNoPDBInfo;
        public int Index;
        public bool InitOnly;
        private bool pinned;

        public Local() : base(NodeType.Local)
        {
        }

        public Local(TypeNode type) : base(NodeType.Local)
        {
            base.Name = Identifier.Empty;
            if (type == null)
            {
                type = CoreSystemTypes.Object;
            }
            this.Type = type;
        }

        public Local(Identifier name, TypeNode type) : this(type)
        {
            base.Name = name;
        }

        public Local(TypeNode type, SourceContext context) : this(Identifier.Empty, type, (Block) null)
        {
            base.SourceContext = context;
        }

        public Local(Identifier name, TypeNode type, Block declaringBlock) : base(NodeType.Local)
        {
            this.DeclaringBlock = declaringBlock;
            base.Name = name;
            if (type == null)
            {
                type = CoreSystemTypes.Object;
            }
            this.Type = type;
        }

        public Local(Identifier name, TypeNode type, SourceContext context) : this(name, type, (Block) null)
        {
            base.SourceContext = context;
        }

        public override bool Equals(object obj)
        {
            LocalBinding binding = obj as LocalBinding;
            return ((obj == this) || ((binding != null) && (binding.BoundLocal == this)));
        }

        public override int GetHashCode() => 
            base.GetHashCode();

        public override string ToString() => 
            base.Name?.ToString();

        public bool Pinned
        {
            get => 
                this.pinned;
            set
            {
                this.pinned = value;
            }
        }
    }
}

