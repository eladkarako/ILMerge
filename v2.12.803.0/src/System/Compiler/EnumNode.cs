namespace System.Compiler
{
    using System;
    using System.Compiler.Metadata;

    internal class EnumNode : TypeNode
    {
        internal static readonly EnumNode Dummy = new EnumNode();
        protected internal TypeNode underlyingType;
        public TypeNode UnderlyingTypeExpression;

        public EnumNode() : base(NodeType.EnumNode)
        {
            base.typeCode = ElementType.ValueType;
            base.Flags |= TypeFlags.Sealed;
        }

        public EnumNode(TypeNode.NestedTypeProvider provideNestedTypes, TypeNode.TypeAttributeProvider provideAttributes, TypeNode.TypeMemberProvider provideMembers, object handle) : base(NodeType.EnumNode, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            base.typeCode = ElementType.ValueType;
            base.Flags |= TypeFlags.Sealed;
        }

        public EnumNode(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags typeAttributes, Identifier Namespace, Identifier name, InterfaceList interfaces, MemberList members) : base(declaringModule, declaringType, attributes, typeAttributes, Namespace, name, interfaces, members, NodeType.EnumNode)
        {
            base.typeCode = ElementType.ValueType;
            base.Flags |= TypeFlags.Sealed;
        }

        public override bool IsUnmanaged =>
            true;

        public virtual TypeNode UnderlyingType
        {
            get
            {
                if (this.underlyingType == null)
                {
                    if (base.template is EnumNode)
                    {
                        return (this.underlyingType = ((EnumNode) base.template).UnderlyingType);
                    }
                    this.underlyingType = CoreSystemTypes.Int32;
                    MemberList members = this.Members;
                    int num = 0;
                    int count = members.Count;
                    while (num < count)
                    {
                        Member member = members[num];
                        Field field = member as Field;
                        if ((field != null) && ((field.Flags & FieldFlags.Static) == FieldFlags.CompilerControlled))
                        {
                            return (this.underlyingType = field.Type);
                        }
                        num++;
                    }
                }
                return this.underlyingType;
            }
            set
            {
                this.underlyingType = value;
                MemberList members = this.Members;
                int num = 0;
                int count = members.Count;
                while (num < count)
                {
                    Member member = members[num];
                    Field field = member as Field;
                    if ((field != null) && ((field.Flags & FieldFlags.Static) == FieldFlags.CompilerControlled))
                    {
                        field.Type = value;
                        return;
                    }
                    num++;
                }
                this.Members.Add(new Field(this, null, FieldFlags.RTSpecialName | FieldFlags.SpecialName | FieldFlags.Public, StandardIds.Value__, value, null));
            }
        }
    }
}

