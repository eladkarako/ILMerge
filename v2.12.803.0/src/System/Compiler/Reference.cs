namespace System.Compiler
{
    using System;
    using System.Compiler.Metadata;
    using System.Text;

    internal class Reference : TypeNode
    {
        private TypeNode elementType;
        protected TypeNodeList structuralElementTypes;

        internal Reference(TypeNode elementType) : base(NodeType.Reference)
        {
            this.elementType = elementType;
            base.typeCode = System.Compiler.Metadata.ElementType.Reference;
            base.Name = Identifier.For(elementType.Name + "@");
            base.Namespace = elementType.Namespace;
        }

        internal override void AppendDocumentIdMangledName(StringBuilder sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if (this.elementType != null)
            {
                this.elementType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
                sb.Append('@');
            }
        }

        public override Type GetRuntimeType()
        {
            if (base.runtimeType == null)
            {
                if (this.ElementType == null)
                {
                    return null;
                }
                Type runtimeType = this.ElementType.GetRuntimeType();
                if (runtimeType == null)
                {
                    return null;
                }
                base.runtimeType = runtimeType.MakeByRefType();
            }
            return base.runtimeType;
        }

        public override bool IsAssignableTo(TypeNode targetType)
        {
            if (targetType != this)
            {
                if (!(targetType is Pointer))
                {
                    return false;
                }
                if (!(((Pointer) targetType).ElementType == this.ElementType))
                {
                    return (((Pointer) targetType).ElementType == CoreSystemTypes.Void);
                }
            }
            return true;
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if (type == null)
            {
                return false;
            }
            if (this != type)
            {
                Reference reference = type as Reference;
                if (reference == null)
                {
                    return false;
                }
                if ((this.ElementType == null) || (reference.ElementType == null))
                {
                    return false;
                }
                if (!(this.ElementType == reference.ElementType))
                {
                    return this.ElementType.IsStructurallyEquivalentTo(reference.ElementType);
                }
            }
            return true;
        }

        public TypeNode ElementType
        {
            get => 
                this.elementType;
            set
            {
                this.elementType = value;
            }
        }

        public override string FullName
        {
            get
            {
                if ((this.ElementType != null) && (this.ElementType.DeclaringType != null))
                {
                    return (this.ElementType.DeclaringType.FullName + "+" + ((base.Name == null) ? "" : base.Name.ToString()));
                }
                if ((base.Namespace != null) && (base.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey))
                {
                    return (base.Namespace.ToString() + "." + ((base.Name == null) ? "" : base.Name.ToString()));
                }
                if (base.Name != null)
                {
                    return base.Name.ToString();
                }
                return "";
            }
        }

        public override bool IsStructural =>
            true;

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList structuralElementTypes = this.structuralElementTypes;
                if (structuralElementTypes == null)
                {
                    this.structuralElementTypes = structuralElementTypes = new TypeNodeList(1);
                    structuralElementTypes.Add(this.ElementType);
                }
                return structuralElementTypes;
            }
        }
    }
}

