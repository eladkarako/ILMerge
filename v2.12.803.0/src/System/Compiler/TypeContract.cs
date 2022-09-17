namespace System.Compiler
{
    using System;

    internal class TypeContract : Node
    {
        public TypeNode DeclaringType;
        public InvariantList InheritedInvariants;
        protected internal InvariantList invariants;

        public TypeContract(TypeNode containingType) : this(containingType, false)
        {
        }

        public TypeContract(TypeNode containingType, bool initInvariantList) : base(NodeType.TypeContract)
        {
            this.DeclaringType = containingType;
            if (initInvariantList)
            {
                this.invariants = new InvariantList();
            }
        }

        public int InvariantCount
        {
            get
            {
                if (this.Invariants != null)
                {
                    return this.Invariants.Count;
                }
                return 0;
            }
        }

        public InvariantList Invariants
        {
            get => 
                this.invariants;
            set
            {
                this.invariants = value;
            }
        }
    }
}

