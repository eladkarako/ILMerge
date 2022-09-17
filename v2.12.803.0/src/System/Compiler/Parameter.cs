namespace System.Compiler
{
    using System;

    internal class Parameter : Variable
    {
        private int argumentListIndex;
        private AttributeList attributes;
        private Method declaringMethod;
        private Expression defaultValue;
        private ParameterFlags flags;
        private System.Compiler.MarshallingInformation marshallingInformation;
        protected AttributeNode paramArrayAttribute;
        protected internal TypeNode paramArrayElementType;
        private int parameterListIndex;

        public Parameter() : base(NodeType.Parameter)
        {
        }

        public Parameter(Identifier name, TypeNode type) : base(NodeType.Parameter)
        {
            base.Name = name;
            this.Type = type;
        }

        public Parameter(AttributeList attributes, ParameterFlags flags, Identifier name, TypeNode type, Literal defaultValue, System.Compiler.MarshallingInformation marshallingInformation) : base(NodeType.Parameter)
        {
            this.attributes = attributes;
            this.defaultValue = defaultValue;
            this.flags = flags;
            this.marshallingInformation = marshallingInformation;
            base.Name = name;
            this.Type = type;
        }

        public override bool Equals(object obj)
        {
            ParameterBinding binding = obj as ParameterBinding;
            return ((obj == this) || ((binding != null) && (binding.BoundParameter == this)));
        }

        public virtual AttributeNode GetAttribute(TypeNode attributeType)
        {
            if (attributeType != null)
            {
                AttributeList attributes = this.Attributes;
                int num = 0;
                int num2 = (attributes == null) ? 0 : attributes.Count;
                while (num < num2)
                {
                    AttributeNode node = attributes[num];
                    if (node != null)
                    {
                        MemberBinding constructor = node.Constructor as MemberBinding;
                        if (constructor != null)
                        {
                            if ((constructor.BoundMember != null) && !(constructor.BoundMember.DeclaringType != attributeType))
                            {
                                return node;
                            }
                        }
                        else
                        {
                            Literal literal = node.Constructor as Literal;
                            if ((literal != null) && !((literal.Value as TypeNode) != attributeType))
                            {
                                return node;
                            }
                        }
                    }
                    num++;
                }
            }
            return null;
        }

        public override int GetHashCode() => 
            base.GetHashCode();

        public virtual AttributeNode GetParamArrayAttribute()
        {
            AttributeNode paramArrayAttribute = this.paramArrayAttribute;
            if (paramArrayAttribute == null)
            {
                AttributeList attributes = this.Attributes;
                int num = 0;
                int num2 = (attributes == null) ? 0 : attributes.Count;
                while (num < num2)
                {
                    AttributeNode node2 = attributes[num];
                    if (node2 != null)
                    {
                        TypeNode declaringType = null;
                        MemberBinding constructor = node2.Constructor as MemberBinding;
                        if (constructor != null)
                        {
                            declaringType = constructor.BoundMember.DeclaringType;
                        }
                        else
                        {
                            Literal literal = node2.Constructor as Literal;
                            if (literal == null)
                            {
                                goto Label_00AB;
                            }
                            declaringType = literal.Value as TypeNode;
                        }
                        if (declaringType == SystemTypes.ParamArrayAttribute)
                        {
                            return (this.paramArrayAttribute = node2);
                        }
                    }
                Label_00AB:
                    num++;
                }
                paramArrayAttribute = this.paramArrayAttribute = AttributeNode.DoesNotExist;
            }
            if (paramArrayAttribute == AttributeNode.DoesNotExist)
            {
                return null;
            }
            return paramArrayAttribute;
        }

        public virtual TypeNode GetParamArrayElementType()
        {
            TypeNode paramArrayElementType = this.paramArrayElementType;
            if (paramArrayElementType == null)
            {
                if (this.GetParamArrayAttribute() != null)
                {
                    TypeNode elementType = TypeNode.StripModifiers(this.Type);
                    Reference reference = elementType as Reference;
                    if (reference != null)
                    {
                        elementType = reference.ElementType;
                    }
                    ArrayType type = elementType as ArrayType;
                    if ((type != null) && (type.Rank == 1))
                    {
                        return (this.paramArrayElementType = type.ElementType);
                    }
                }
                this.paramArrayElementType = paramArrayElementType = Class.DoesNotExist;
            }
            if (paramArrayElementType == Class.DoesNotExist)
            {
                return null;
            }
            return paramArrayElementType;
        }

        public override string ToString()
        {
            if (base.Name == null)
            {
                return "";
            }
            if (this.Type == null)
            {
                return base.Name.ToString();
            }
            return (this.Type.ToString() + " " + base.Name.ToString());
        }

        public int ArgumentListIndex
        {
            get => 
                this.argumentListIndex;
            set
            {
                this.argumentListIndex = value;
            }
        }

        public AttributeList Attributes
        {
            get => 
                this.attributes;
            set
            {
                this.attributes = value;
            }
        }

        public Method DeclaringMethod
        {
            get => 
                this.declaringMethod;
            set
            {
                this.declaringMethod = value;
            }
        }

        public Expression DefaultValue
        {
            get => 
                this.defaultValue;
            set
            {
                this.defaultValue = value;
            }
        }

        public ParameterFlags Flags
        {
            get => 
                this.flags;
            set
            {
                this.flags = value;
            }
        }

        public virtual bool IsIn
        {
            get => 
                ((this.Flags & ParameterFlags.In) != ParameterFlags.None);
            set
            {
                if (value)
                {
                    this.Flags |= ParameterFlags.In;
                }
                else
                {
                    this.Flags &= ~ParameterFlags.In;
                }
            }
        }

        public virtual bool IsOptional
        {
            get => 
                ((this.Flags & ParameterFlags.Optional) != ParameterFlags.None);
            set
            {
                if (value)
                {
                    this.Flags |= ParameterFlags.Optional;
                }
                else
                {
                    this.Flags &= ~ParameterFlags.Optional;
                }
            }
        }

        public virtual bool IsOut
        {
            get => 
                ((this.Flags & ParameterFlags.Out) != ParameterFlags.None);
            set
            {
                if (value)
                {
                    this.Flags |= ParameterFlags.Out;
                }
                else
                {
                    this.Flags &= ~ParameterFlags.Out;
                }
            }
        }

        public System.Compiler.MarshallingInformation MarshallingInformation
        {
            get => 
                this.marshallingInformation;
            set
            {
                this.marshallingInformation = value;
            }
        }

        public int ParameterListIndex
        {
            get => 
                this.parameterListIndex;
            set
            {
                this.parameterListIndex = value;
            }
        }
    }
}

