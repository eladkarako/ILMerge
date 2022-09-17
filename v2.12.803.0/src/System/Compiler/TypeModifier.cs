namespace System.Compiler
{
    using System;
    using System.Compiler.Metadata;

    internal abstract class TypeModifier : TypeNode
    {
        private TypeNode modifiedType;
        public TypeNode ModifiedTypeExpression;
        private TypeNode modifier;
        public TypeNode ModifierExpression;
        protected TypeNodeList structuralElementTypes;

        internal TypeModifier(NodeType type, TypeNode modifier, TypeNode modified) : base(type)
        {
            this.modifier = modifier;
            this.modifiedType = modified;
            base.DeclaringModule = modified.DeclaringModule;
            base.Namespace = modified.Namespace;
            if (type == NodeType.OptionalModifier)
            {
                base.typeCode = ElementType.OptionalModifier;
                base.Name = Identifier.For(string.Concat(new object[] { "optional(", modifier.Name, ") ", modified.Name }));
                base.fullName = "optional(" + modifier.FullName + ") " + modified.FullName;
            }
            else
            {
                base.typeCode = ElementType.RequiredModifier;
                base.Name = Identifier.For(string.Concat(new object[] { "required(", modifier.Name, ") ", modified.Name }));
                base.fullName = "required(" + modifier.FullName + ") " + modified.FullName;
            }
            base.Flags = modified.Flags;
        }

        public override Node Clone() => 
            base.Clone();

        public override string GetFullUnmangledNameWithoutTypeParameters() => 
            this.ModifiedType.GetFullUnmangledNameWithoutTypeParameters();

        public override string GetFullUnmangledNameWithTypeParameters() => 
            this.ModifiedType.GetFullUnmangledNameWithTypeParameters();

        public override string GetUnmangledNameWithoutTypeParameters() => 
            this.ModifiedType.GetUnmangledNameWithoutTypeParameters();

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if (type == null)
            {
                return false;
            }
            if (this == type)
            {
                return true;
            }
            if (base.NodeType != type.NodeType)
            {
                return false;
            }
            TypeModifier modifier = type as TypeModifier;
            if (modifier == null)
            {
                return false;
            }
            if ((this.Modifier != modifier.Modifier) && ((this.Modifier == null) || !this.Modifier.IsStructurallyEquivalentTo(modifier.Modifier)))
            {
                return false;
            }
            return (!(this.ModifiedType != modifier.ModifiedType) || ((this.ModifiedType != null) && this.ModifiedType.IsStructurallyEquivalentTo(modifier.ModifiedType)));
        }

        public override bool IsPointerType =>
            this.ModifiedType.IsPointerType;

        public override bool IsReferenceType =>
            this.ModifiedType.IsReferenceType;

        public override bool IsStructural =>
            true;

        public override bool IsTemplateParameter =>
            this.ModifiedType.IsTemplateParameter;

        public override bool IsUnmanaged =>
            this.ModifiedType.IsUnmanaged;

        public override bool IsValueType =>
            this.ModifiedType.IsValueType;

        public TypeNode ModifiedType
        {
            get => 
                this.modifiedType;
            set
            {
                this.modifiedType = value;
            }
        }

        public TypeNode Modifier
        {
            get => 
                this.modifier;
            set
            {
                this.modifier = value;
            }
        }

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList structuralElementTypes = this.structuralElementTypes;
                if (structuralElementTypes == null)
                {
                    this.structuralElementTypes = structuralElementTypes = new TypeNodeList(2);
                    structuralElementTypes.Add(this.ModifiedType);
                    structuralElementTypes.Add(this.Modifier);
                }
                return structuralElementTypes;
            }
        }
    }
}

