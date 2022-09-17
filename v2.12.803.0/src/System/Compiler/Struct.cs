namespace System.Compiler
{
    using System;
    using System.Compiler.Metadata;

    internal class Struct : TypeNode
    {
        protected bool cachedUnmanaged;
        protected bool cachedUnmanagedIsValid;
        internal static readonly Struct Dummy = new Struct();

        public Struct() : base(NodeType.Struct)
        {
            base.typeCode = ElementType.ValueType;
            base.Flags = TypeFlags.Sealed;
        }

        public Struct(TypeNode.NestedTypeProvider provideNestedTypes, TypeNode.TypeAttributeProvider provideAttributes, TypeNode.TypeMemberProvider provideMembers, object handle) : base(NodeType.Struct, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            base.typeCode = ElementType.ValueType;
        }

        public Struct(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags, Identifier Namespace, Identifier name, InterfaceList interfaces, MemberList members) : base(declaringModule, declaringType, attributes, flags, Namespace, name, interfaces, members, NodeType.Struct)
        {
            this.Interfaces = interfaces;
            base.typeCode = ElementType.ValueType;
            base.Flags |= TypeFlags.Sealed;
        }

        public override bool IsUnmanaged
        {
            get
            {
                if (!this.cachedUnmanagedIsValid)
                {
                    this.cachedUnmanagedIsValid = true;
                    this.cachedUnmanaged = true;
                    if (this.IsPrimitive)
                    {
                        return (this.cachedUnmanaged = true);
                    }
                    MemberList members = this.Members;
                    bool flag2 = true;
                    int num = 0;
                    int num2 = (members == null) ? 0 : members.Count;
                    while (num < num2)
                    {
                        Field field = members[num] as Field;
                        if (((field != null) && (field.Type != null)) && (!field.IsStatic && !field.Type.IsUnmanaged))
                        {
                            flag2 = false;
                            break;
                        }
                        num++;
                    }
                    this.cachedUnmanaged = flag2;
                }
                return this.cachedUnmanaged;
            }
        }
    }
}

