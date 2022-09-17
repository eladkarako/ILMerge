namespace System.Compiler
{
    using System;

    internal class Interface : TypeNode
    {
        internal static readonly Interface Dummy = new Interface();
        protected MemberList jointDefaultMembers;
        protected TrivialHashtable jointMemberTable;

        public Interface() : base(NodeType.Interface)
        {
            base.Flags = TypeFlags.Abstract | TypeFlags.ClassSemanticsMask;
        }

        public Interface(InterfaceList baseInterfaces) : base(NodeType.Interface)
        {
            this.Interfaces = baseInterfaces;
            base.Flags = TypeFlags.Abstract | TypeFlags.ClassSemanticsMask;
        }

        public Interface(InterfaceList baseInterfaces, TypeNode.NestedTypeProvider provideNestedTypes, TypeNode.TypeAttributeProvider provideAttributes, TypeNode.TypeMemberProvider provideMembers, object handle) : base(NodeType.Interface, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            this.Interfaces = baseInterfaces;
        }

        public Interface(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags, Identifier Namespace, Identifier name, InterfaceList baseInterfaces, MemberList members) : base(declaringModule, declaringType, attributes, flags, Namespace, name, baseInterfaces, members, NodeType.Interface)
        {
            base.Flags |= TypeFlags.Abstract | TypeFlags.ClassSemanticsMask;
        }

        public override void GetAbstractMethods(MethodList result)
        {
            MemberList members = this.Members;
            if (members != null)
            {
                int num = 0;
                int count = members.Count;
                while (num < count)
                {
                    Method element = members[num] as Method;
                    if (element != null)
                    {
                        result.Add(element);
                    }
                    num++;
                }
            }
        }

        public virtual MemberList GetAllDefaultMembers()
        {
            if (this.jointDefaultMembers == null)
            {
                this.jointDefaultMembers = new MemberList();
                MemberList defaultMembers = this.DefaultMembers;
                int num = 0;
                int num2 = (defaultMembers == null) ? 0 : defaultMembers.Count;
                while (num < num2)
                {
                    this.jointDefaultMembers.Add(defaultMembers[num]);
                    num++;
                }
                InterfaceList interfaces = this.Interfaces;
                if (interfaces != null)
                {
                    int num3 = 0;
                    int count = interfaces.Count;
                    while (num3 < count)
                    {
                        Interface interface2 = interfaces[num3];
                        if (interface2 != null)
                        {
                            defaultMembers = interface2.GetAllDefaultMembers();
                            if (defaultMembers != null)
                            {
                                int num5 = 0;
                                int num6 = defaultMembers.Count;
                                while (num5 < num6)
                                {
                                    this.jointDefaultMembers.Add(defaultMembers[num5]);
                                    num5++;
                                }
                            }
                        }
                        num3++;
                    }
                }
            }
            return this.jointDefaultMembers;
        }

        public virtual MemberList GetAllMembersNamed(Identifier name)
        {
            lock (this)
            {
                TrivialHashtable jointMemberTable = this.jointMemberTable;
                if (jointMemberTable == null)
                {
                    this.jointMemberTable = jointMemberTable = new TrivialHashtable();
                }
                MemberList list = (MemberList) jointMemberTable[name.UniqueIdKey];
                if (list == null)
                {
                    jointMemberTable[name.UniqueIdKey] = list = new MemberList();
                    MemberList membersNamed = this.GetMembersNamed(name);
                    int num = 0;
                    int num2 = (membersNamed == null) ? 0 : membersNamed.Count;
                    while (num < num2)
                    {
                        list.Add(membersNamed[num]);
                        num++;
                    }
                    InterfaceList interfaces = this.Interfaces;
                    int num3 = 0;
                    int num4 = (interfaces == null) ? 0 : interfaces.Count;
                    while (num3 < num4)
                    {
                        Interface interface3 = interfaces[num3];
                        if (interface3 != null)
                        {
                            membersNamed = interface3.GetAllMembersNamed(name);
                            if (membersNamed != null)
                            {
                                int num5 = 0;
                                int count = membersNamed.Count;
                                while (num5 < count)
                                {
                                    list.Add(membersNamed[num5]);
                                    num5++;
                                }
                            }
                        }
                        num3++;
                    }
                    membersNamed = CoreSystemTypes.Object.GetMembersNamed(name);
                    int num7 = 0;
                    int num8 = (membersNamed == null) ? 0 : membersNamed.Count;
                    while (num7 < num8)
                    {
                        list.Add(membersNamed[num7]);
                        num7++;
                    }
                }
                return list;
            }
        }
    }
}

