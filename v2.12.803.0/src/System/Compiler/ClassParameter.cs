namespace System.Compiler
{
    using System;
    using System.Text;
    using System.Xml;

    internal class ClassParameter : Class, ITypeParameter
    {
        private Member declaringMember;
        protected TrivialHashtable jointMemberTable;
        private int parameterListIndex;
        protected internal TypeNodeList structuralElementTypes;
        private System.Compiler.TypeParameterFlags typeParameterFlags;

        public ClassParameter()
        {
            base.NodeType = NodeType.ClassParameter;
            base.baseClass = CoreSystemTypes.Object;
            base.Flags = TypeFlags.Abstract | TypeFlags.NestedPublic;
            base.Namespace = StandardIds.TypeParameter;
        }

        public ClassParameter(TypeNode.NestedTypeProvider provideNestedTypes, TypeNode.TypeAttributeProvider provideAttributes, TypeNode.TypeMemberProvider provideMembers, object handle) : base(provideNestedTypes, provideAttributes, provideMembers, handle)
        {
            base.NodeType = NodeType.ClassParameter;
            base.baseClass = CoreSystemTypes.Object;
            base.Flags = TypeFlags.SpecialName | TypeFlags.Abstract | TypeFlags.NestedPrivate;
            base.Namespace = StandardIds.TypeParameter;
        }

        internal override void AppendDocumentIdMangledName(StringBuilder sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if (TargetPlatform.GenericTypeNamesMangleChar != '\0')
            {
                int num = (methodTypeParameters == null) ? 0 : methodTypeParameters.Count;
                for (int i = 0; i < num; i++)
                {
                    TypeNode node = methodTypeParameters[i];
                    if (node == this)
                    {
                        sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                        sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                        sb.Append(i);
                        return;
                    }
                }
                num = (typeParameters == null) ? 0 : typeParameters.Count;
                for (int j = 0; j < num; j++)
                {
                    TypeNode node2 = typeParameters[j];
                    if (node2 == this)
                    {
                        sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                        sb.Append(j);
                        return;
                    }
                }
                sb.Append("not found:");
            }
            sb.Append(this.FullName);
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
                    for (TypeNode node = this; node != null; node = node.BaseType)
                    {
                        MemberList membersNamed = node.GetMembersNamed(name);
                        if (membersNamed != null)
                        {
                            int num = 0;
                            int count = membersNamed.Count;
                            while (num < count)
                            {
                                list.Add(membersNamed[num]);
                                num++;
                            }
                        }
                    }
                    InterfaceList interfaces = this.Interfaces;
                    if (interfaces != null)
                    {
                        int num3 = 0;
                        int num4 = interfaces.Count;
                        while (num3 < num4)
                        {
                            Interface interface2 = interfaces[num3];
                            if (interface2 != null)
                            {
                                base.members = interface2.GetAllMembersNamed(name);
                                if (base.members != null)
                                {
                                    int num5 = 0;
                                    int num6 = (base.members == null) ? 0 : base.members.Count;
                                    while (num5 < num6)
                                    {
                                        list.Add(base.members[num5]);
                                        num5++;
                                    }
                                }
                            }
                            num3++;
                        }
                    }
                    base.members = CoreSystemTypes.Object.GetMembersNamed(name);
                    if (base.members != null)
                    {
                        int num7 = 0;
                        int num8 = base.members.Count;
                        while (num7 < num8)
                        {
                            list.Add(base.members[num7]);
                            num7++;
                        }
                    }
                }
                return list;
            }
        }

        public override string GetFullUnmangledNameWithoutTypeParameters() => 
            this.GetUnmangledNameWithoutTypeParameters();

        public override string GetFullUnmangledNameWithTypeParameters() => 
            this.GetUnmangledNameWithTypeParameters();

        public override Type GetRuntimeType()
        {
            TypeNode declaringMember = this.DeclaringMember as TypeNode;
            if (declaringMember == null)
            {
                return null;
            }
            Type runtimeType = declaringMember.GetRuntimeType();
            if (runtimeType == null)
            {
                return null;
            }
            Type[] genericArguments = runtimeType.GetGenericArguments();
            if (this.ParameterListIndex >= genericArguments.Length)
            {
                return null;
            }
            return genericArguments[this.ParameterListIndex];
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if (type == null)
            {
                return false;
            }
            if (this != type)
            {
                ITypeParameter parameter = type as ITypeParameter;
                if (parameter == null)
                {
                    return false;
                }
                if (((base.Name != null) && (type.Name != null)) && ((base.Name.UniqueIdKey != type.Name.UniqueIdKey) && (this.DeclaringMember == parameter.DeclaringMember)))
                {
                    return false;
                }
                TypeNode baseType = this.BaseType;
                TypeNode node2 = type.BaseType;
                if (baseType == null)
                {
                    baseType = CoreSystemTypes.Object;
                }
                if (node2 == null)
                {
                    node2 = CoreSystemTypes.Object;
                }
                if (baseType != node2)
                {
                    return false;
                }
                if (this.Interfaces == null)
                {
                    if (type.Interfaces != null)
                    {
                        return (type.Interfaces.Count == 0);
                    }
                    return true;
                }
                if (type.Interfaces == null)
                {
                    return (this.Interfaces.Count == 0);
                }
                int count = this.Interfaces.Count;
                if (count != type.Interfaces.Count)
                {
                    return false;
                }
                for (int i = 0; i < count; i++)
                {
                    Interface interface2 = this.Interfaces[i];
                    Interface interface3 = type.Interfaces[i];
                    if ((interface2 == null) || (interface3 == null))
                    {
                        return false;
                    }
                    if (interface2 != interface3)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Member DeclaringMember
        {
            get => 
                this.declaringMember;
            set
            {
                this.declaringMember = value;
            }
        }

        public override XmlNode Documentation
        {
            get
            {
                if (((base.documentation == null) && (this.declaringMember != null)) && (base.Name != null))
                {
                    XmlNode documentation = this.declaringMember.Documentation;
                    if ((documentation != null) && documentation.HasChildNodes)
                    {
                        string name = base.Name.Name;
                        foreach (XmlNode node2 in documentation.ChildNodes)
                        {
                            if ((node2.Name == "typeparam") && (node2.Attributes != null))
                            {
                                foreach (XmlAttribute attribute in node2.Attributes)
                                {
                                    if (((attribute != null) && (attribute.Name == "name")) && (attribute.Value == name))
                                    {
                                        return (base.documentation = node2);
                                    }
                                }
                            }
                        }
                    }
                }
                return base.documentation;
            }
            set
            {
                base.documentation = value;
            }
        }

        public override string HelpText
        {
            get
            {
                if (base.helpText == null)
                {
                    XmlNode documentation = this.Documentation;
                    if (documentation != null)
                    {
                        base.helpText = documentation.InnerText;
                    }
                }
                return base.helpText;
            }
            set
            {
                base.helpText = value;
            }
        }

        public override bool IsReferenceType =>
            (((this.TypeParameterFlags & System.Compiler.TypeParameterFlags.ReferenceTypeConstraint) == System.Compiler.TypeParameterFlags.ReferenceTypeConstraint) || (((base.baseClass != null) && (base.baseClass != SystemTypes.Object)) && base.baseClass.IsReferenceType));

        public override bool IsStructural =>
            true;

        public override bool IsTemplateParameter =>
            true;

        public override bool IsValueType =>
            ((this.typeParameterFlags & System.Compiler.TypeParameterFlags.ValueTypeConstraint) == System.Compiler.TypeParameterFlags.ValueTypeConstraint);

        public int ParameterListIndex
        {
            get => 
                this.parameterListIndex;
            set
            {
                this.parameterListIndex = value;
            }
        }

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList structuralElementTypes = this.structuralElementTypes;
                if (structuralElementTypes == null)
                {
                    this.structuralElementTypes = structuralElementTypes = new TypeNodeList();
                    if (this.BaseType != null)
                    {
                        structuralElementTypes.Add(this.BaseType);
                    }
                    InterfaceList interfaces = this.Interfaces;
                    int num = 0;
                    int num2 = (interfaces == null) ? 0 : interfaces.Count;
                    while (num < num2)
                    {
                        Interface element = interfaces[num];
                        if (element != null)
                        {
                            structuralElementTypes.Add(element);
                        }
                        num++;
                    }
                }
                return structuralElementTypes;
            }
        }

        Module ITypeParameter.DeclaringModule =>
            base.DeclaringModule;

        TypeFlags ITypeParameter.Flags =>
            base.Flags;

        SourceContext ITypeParameter.SourceContext =>
            base.SourceContext;

        public System.Compiler.TypeParameterFlags TypeParameterFlags
        {
            get => 
                this.typeParameterFlags;
            set
            {
                this.typeParameterFlags = value;
            }
        }
    }
}

