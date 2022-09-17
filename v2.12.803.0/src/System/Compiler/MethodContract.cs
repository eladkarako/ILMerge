namespace System.Compiler
{
    using System;

    internal class MethodContract : Node
    {
        protected internal Block contractInitializer;
        public Method DeclaringMethod;
        protected internal EnsuresList ensures;
        protected internal bool? isPure;
        private int legacyValidations;
        protected internal EnsuresList modelEnsures;
        protected internal ExpressionList modifies;
        public Method OriginalDeclaringMethod;
        protected internal Block postPreamble;
        protected internal RequiresList requires;
        protected internal RequiresList validations;

        public MethodContract(Method declaringMethod) : base(NodeType.MethodContract)
        {
            this.legacyValidations = -1;
            this.DeclaringMethod = this.OriginalDeclaringMethod = declaringMethod;
        }

        public Block ContractInitializer
        {
            get => 
                this.contractInitializer;
            set
            {
                this.contractInitializer = value;
            }
        }

        public EnsuresList Ensures
        {
            get => 
                this.ensures;
            set
            {
                this.ensures = value;
            }
        }

        public int EnsuresCount
        {
            get
            {
                EnsuresList ensures = this.Ensures;
                return ensures?.Count;
            }
        }

        public bool HasLegacyValidations
        {
            get
            {
                if (this.legacyValidations < 0)
                {
                    this.legacyValidations = 0;
                    if (this.validations != null)
                    {
                        for (int i = 0; i < this.validations.Count; i++)
                        {
                            if (this.validations[i] is RequiresOtherwise)
                            {
                                this.legacyValidations++;
                            }
                        }
                    }
                }
                return (this.legacyValidations > 0);
            }
        }

        public bool IsPure
        {
            get
            {
                if (this.isPure.HasValue)
                {
                    return this.isPure.Value;
                }
                if (this.DeclaringMethod != null)
                {
                    AttributeList attributes = this.DeclaringMethod.Attributes;
                    for (int i = 0; (attributes != null) && (i < attributes.Count); i++)
                    {
                        AttributeNode node = attributes[i];
                        if (((node != null) && (node.Type != null)) && ((node.Type.Name != null) && (node.Type.Name.Name == "PureAttribute")))
                        {
                            this.isPure = true;
                            return true;
                        }
                    }
                }
                this.isPure = false;
                return false;
            }
            set
            {
                this.isPure = new bool?(value);
            }
        }

        public EnsuresList ModelEnsures
        {
            get => 
                this.modelEnsures;
            set
            {
                this.modelEnsures = value;
            }
        }

        public int ModelEnsuresCount
        {
            get
            {
                EnsuresList modelEnsures = this.ModelEnsures;
                return modelEnsures?.Count;
            }
        }

        public ExpressionList Modifies
        {
            get => 
                null;
            set
            {
                this.modifies = value;
            }
        }

        public int ModifiesCount
        {
            get
            {
                ExpressionList modifies = this.Modifies;
                return modifies?.Count;
            }
        }

        public Block PostPreamble
        {
            get => 
                this.postPreamble;
            set
            {
                this.postPreamble = value;
            }
        }

        public RequiresList Requires
        {
            get => 
                this.requires;
            set
            {
                this.requires = value;
            }
        }

        public int RequiresCount
        {
            get
            {
                RequiresList requires = this.Requires;
                return requires?.Count;
            }
        }

        public RequiresList Validations
        {
            get => 
                this.validations;
            set
            {
                this.validations = value;
            }
        }

        public int ValidationsCount
        {
            get
            {
                RequiresList validations = this.Validations;
                return validations?.Count;
            }
        }
    }
}

