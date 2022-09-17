namespace System.Compiler
{
    using System;

    internal class Class : TypeNode
    {
        internal Class baseClass;
        public Class BaseClassExpression;
        internal static readonly Class DoesNotExist = new Class();
        internal static readonly Class Dummy = new Class();
        public bool IsAbstractSealedContainerForStatics;

        public Class() : base(NodeType.Class)
        {
        }

        public Class(TypeNode.NestedTypeProvider provideNestedTypes, TypeNode.TypeAttributeProvider provideAttributes, TypeNode.TypeMemberProvider provideMembers, object handle) : base(NodeType.Class, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
        }

        public Class(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags, Identifier Namespace, Identifier name, Class baseClass, InterfaceList interfaces, MemberList members) : base(declaringModule, declaringType, attributes, flags, Namespace, name, interfaces, members, NodeType.Class)
        {
            this.baseClass = baseClass;
        }

        protected static bool AlreadyInList(MethodList list, Method method)
        {
            if (list != null)
            {
                int num = 0;
                int count = list.Count;
                while (num < count)
                {
                    if (list[num] == method)
                    {
                        return true;
                    }
                    num++;
                }
            }
            return false;
        }

        public override void GetAbstractMethods(MethodList result)
        {
            if (this.IsAbstract)
            {
                MethodList list = new MethodList();
                if (this.BaseClass != null)
                {
                    this.BaseClass.GetAbstractMethods(list);
                    int num = 0;
                    int num2 = list.Count;
                    while (num < num2)
                    {
                        Method meth = list[num];
                        if (!base.ImplementsMethod(meth, false))
                        {
                            result.Add(meth);
                        }
                        num++;
                    }
                }
                MemberList members = this.Members;
                int num3 = 0;
                int count = members.Count;
                while (num3 < count)
                {
                    Method element = members[num3] as Method;
                    if ((element != null) && element.IsAbstract)
                    {
                        result.Add(element);
                    }
                    num3++;
                }
                InterfaceList interfaces = this.Interfaces;
                if (interfaces != null)
                {
                    int num5 = 0;
                    int num6 = interfaces.Count;
                    while (num5 < num6)
                    {
                        Interface interface2 = interfaces[num5];
                        if (interface2 != null)
                        {
                            MemberList list4 = interface2.Members;
                            if (list4 != null)
                            {
                                int num7 = 0;
                                int num8 = list4.Count;
                                while (num7 < num8)
                                {
                                    Method method = list4[num7] as Method;
                                    if (((method != null) && !base.ImplementsExplicitly(method)) && (!base.ImplementsMethod(method, true) && !AlreadyInList(result, method)))
                                    {
                                        result.Add(method);
                                    }
                                    num7++;
                                }
                            }
                        }
                        num5++;
                    }
                }
            }
        }

        public virtual Class BaseClass
        {
            get
            {
                InterfaceList interfaces = this.Interfaces;
                return this.baseClass;
            }
            set
            {
                this.baseClass = value;
            }
        }
    }
}

