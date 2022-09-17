namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Text;

    internal class Property : Member
    {
        private PropertyFlags flags;
        protected string fullName;
        private Method getter;
        protected Property hiddenProperty;
        public TypeNodeList ImplementedTypeExpressions;
        public TypeNodeList ImplementedTypes;
        public bool IsModelfield;
        public static readonly Property NotSpecified = new Property();
        private MethodList otherMethods;
        protected Property overriddenProperty;
        private ParameterList parameters;
        protected PropertyInfo propertyInfo;
        private Method setter;
        protected TypeNode type;
        public TypeNode TypeExpression;

        public Property() : base(NodeType.Property)
        {
        }

        public Property(TypeNode declaringType, AttributeList attributes, PropertyFlags flags, Identifier name, Method getter, Method setter) : base(declaringType, attributes, name, NodeType.Property)
        {
            this.flags = flags;
            this.getter = getter;
            this.setter = setter;
            if (getter != null)
            {
                getter.DeclaringMember = this;
            }
            if (setter != null)
            {
                setter.DeclaringMember = this;
            }
        }

        public virtual Method GetBaseGetter()
        {
            if (!base.HidesBaseClassMember)
            {
                TypeNode declaringType = this.DeclaringType;
                if (declaringType != null)
                {
                    while (declaringType.BaseType != null)
                    {
                        MemberList membersNamed = declaringType.BaseType.GetMembersNamed(base.Name);
                        int num = 0;
                        int num2 = (membersNamed == null) ? 0 : membersNamed.Count;
                        while (num < num2)
                        {
                            Property property = membersNamed[num] as Property;
                            if (((property != null) && property.ParametersMatch(this.Parameters)) && (property.Getter != null))
                            {
                                return property.Getter;
                            }
                            num++;
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        public virtual Method GetBaseSetter()
        {
            if (!base.HidesBaseClassMember)
            {
                TypeNode declaringType = this.DeclaringType;
                if (declaringType != null)
                {
                    while (declaringType.BaseType != null)
                    {
                        MemberList membersNamed = declaringType.BaseType.GetMembersNamed(base.Name);
                        int num = 0;
                        int num2 = (membersNamed == null) ? 0 : membersNamed.Count;
                        while (num < num2)
                        {
                            Property property = membersNamed[num] as Property;
                            if (((property != null) && property.ParametersMatch(this.Parameters)) && (property.Setter != null))
                            {
                                return property.Setter;
                            }
                            num++;
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        protected override Identifier GetDocumentationId()
        {
            StringBuilder sb = new StringBuilder(this.DeclaringType.DocumentationId.ToString()) {
                [0] = 'P'
            };
            sb.Append('.');
            if (base.Name != null)
            {
                sb.Append(base.Name.ToString());
            }
            ParameterList parameters = this.Parameters;
            int num = 0;
            int num2 = (parameters == null) ? 0 : parameters.Count;
            while (num < num2)
            {
                Parameter parameter = parameters[num];
                if ((parameter != null) && (parameter.Type != null))
                {
                    if (num == 0)
                    {
                        sb.Append('(');
                    }
                    else
                    {
                        sb.Append(',');
                    }
                    parameter.Type.AppendDocumentIdMangledName(sb, null, this.DeclaringType.TemplateParameters);
                    if (num == (num2 - 1))
                    {
                        sb.Append(')');
                    }
                }
                num++;
            }
            return Identifier.For(sb.ToString());
        }

        public static Property GetProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return null;
            }
            TypeNode typeNode = TypeNode.GetTypeNode(propertyInfo.DeclaringType);
            if (typeNode == null)
            {
                return null;
            }
            ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
            int num = (indexParameters == null) ? 0 : indexParameters.Length;
            TypeNode[] types = new TypeNode[num];
            if (indexParameters != null)
            {
                for (int i = 0; i < num; i++)
                {
                    ParameterInfo info = indexParameters[i];
                    if (info == null)
                    {
                        return null;
                    }
                    types[i] = TypeNode.GetTypeNode(info.ParameterType);
                }
            }
            return typeNode.GetProperty(Identifier.For(propertyInfo.Name), types);
        }

        public virtual PropertyInfo GetPropertyInfo()
        {
            if (this.propertyInfo == null)
            {
                if (this.DeclaringType == null)
                {
                    return null;
                }
                System.Type runtimeType = this.DeclaringType.GetRuntimeType();
                if (runtimeType == null)
                {
                    return null;
                }
                if (this.Type == null)
                {
                    return null;
                }
                System.Type type2 = this.Type.GetRuntimeType();
                if (type2 == null)
                {
                    return null;
                }
                ParameterList parameters = this.Parameters;
                int num = (parameters == null) ? 0 : parameters.Count;
                System.Type[] typeArray = new System.Type[num];
                for (int i = 0; i < num; i++)
                {
                    Parameter parameter = parameters[i];
                    if ((parameter == null) || (parameter.Type == null))
                    {
                        return null;
                    }
                    if ((typeArray[i] = parameter.Type.GetRuntimeType()) == null)
                    {
                        return null;
                    }
                }
                foreach (PropertyInfo info in runtimeType.GetMember(base.Name.ToString(), MemberTypes.Property, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if ((info != null) && (info.PropertyType == type2))
                    {
                        ParameterInfo[] indexParameters = info.GetIndexParameters();
                        if ((indexParameters != null) && (indexParameters.Length == num))
                        {
                            for (int j = 0; j < num; j++)
                            {
                                ParameterInfo info2 = indexParameters[j];
                                if ((info2 != null) && (info2.ParameterType == typeArray[j]))
                                {
                                }
                            }
                            return (this.propertyInfo = info);
                        }
                    }
                }
            }
            return this.propertyInfo;
        }

        public virtual Literal GetValue(Literal targetObject, params Literal[] indices)
        {
            int num = (indices == null) ? 0 : indices.Length;
            object[] objArray = (num == 0) ? null : new object[num];
            if ((objArray != null) && (indices != null))
            {
                for (int i = 0; i < num; i++)
                {
                    Literal literal = indices[i];
                    objArray[i] = literal?.Value;
                }
            }
            return new Literal(this.GetValue(targetObject.Value, objArray));
        }

        public virtual object GetValue(object targetObject, params object[] indices) => 
            this.GetPropertyInfo()?.GetValue(targetObject, indices);

        public virtual bool ParametersMatch(ParameterList parameters)
        {
            ParameterList list = this.Parameters;
            int num = (list == null) ? 0 : list.Count;
            int num2 = (parameters == null) ? 0 : parameters.Count;
            if (num != num2)
            {
                return false;
            }
            if (parameters != null)
            {
                for (int i = 0; i < num; i++)
                {
                    Parameter parameter = list[i];
                    Parameter parameter2 = parameters[i];
                    if (parameter.Type != parameter2.Type)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public virtual bool ParametersMatchStructurally(ParameterList parameters)
        {
            ParameterList list = this.Parameters;
            int num = (list == null) ? 0 : list.Count;
            int num2 = (parameters == null) ? 0 : parameters.Count;
            if (num != num2)
            {
                return false;
            }
            if (parameters != null)
            {
                for (int i = 0; i < num; i++)
                {
                    Parameter parameter = list[i];
                    Parameter parameter2 = parameters[i];
                    if ((parameter == null) || (parameter2 == null))
                    {
                        return false;
                    }
                    if ((parameter.Type == null) || (parameter2.Type == null))
                    {
                        return false;
                    }
                    if ((parameter.Type != parameter2.Type) && !parameter.Type.IsStructurallyEquivalentTo(parameter2.Type))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public virtual bool ParameterTypesMatch(TypeNodeList argumentTypes)
        {
            ParameterList parameters = this.Parameters;
            int num = (parameters == null) ? 0 : parameters.Count;
            int num2 = (argumentTypes == null) ? 0 : argumentTypes.Count;
            if (num != num2)
            {
                return false;
            }
            if (argumentTypes != null)
            {
                for (int i = 0; i < num; i++)
                {
                    Parameter parameter = this.Parameters[i];
                    if (parameter == null)
                    {
                        return false;
                    }
                    TypeNode node = argumentTypes[i];
                    if (parameter.Type != node)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public virtual void SetValue(Literal targetObject, Literal value, params Literal[] indices)
        {
            int num = (indices == null) ? 0 : indices.Length;
            object[] index = (num == 0) ? null : new object[num];
            if ((index != null) && (indices != null))
            {
                for (int i = 0; i < num; i++)
                {
                    Literal literal = indices[i];
                    index[i] = literal?.Value;
                }
            }
            PropertyInfo propertyInfo = this.GetPropertyInfo();
            if (propertyInfo == null)
            {
                throw new InvalidOperationException();
            }
            propertyInfo.SetValue(targetObject.Value, value.Value, index);
        }

        public virtual void SetValue(object targetObject, object value, params object[] indices)
        {
            PropertyInfo propertyInfo = this.GetPropertyInfo();
            if (propertyInfo == null)
            {
                throw new InvalidOperationException();
            }
            propertyInfo.SetValue(targetObject, value, indices);
        }

        public override string ToString() => 
            (this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "." + base.Name);

        public PropertyFlags Flags
        {
            get => 
                this.flags;
            set
            {
                this.flags = value;
            }
        }

        public override string FullName
        {
            get
            {
                if (this.fullName != null)
                {
                    return this.fullName;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append(this.DeclaringType.FullName);
                builder.Append('.');
                if (base.Name != null)
                {
                    builder.Append(base.Name.ToString());
                }
                ParameterList parameters = this.Parameters;
                int num = 0;
                int num2 = (parameters == null) ? 0 : parameters.Count;
                while (num < num2)
                {
                    Parameter parameter = parameters[num];
                    if ((parameter != null) && (parameter.Type != null))
                    {
                        if (num == 0)
                        {
                            builder.Append('(');
                        }
                        else
                        {
                            builder.Append(',');
                        }
                        builder.Append(parameter.Type.FullName);
                        if (num == (num2 - 1))
                        {
                            builder.Append(')');
                        }
                    }
                    num++;
                }
                return (this.fullName = builder.ToString());
            }
        }

        public Method Getter
        {
            get => 
                this.getter;
            set
            {
                this.getter = value;
            }
        }

        public override string HelpText
        {
            get
            {
                if (base.helpText != null)
                {
                    return base.helpText;
                }
                StringBuilder builder = new StringBuilder(base.HelpText);
                bool flag = builder.Length != 0;
                if (((this.Getter != null) && (this.Getter.HelpText != null)) && (this.Getter.HelpText.Length > 0))
                {
                    if (flag)
                    {
                        builder.Append("\n");
                        flag = false;
                    }
                    builder.Append("get\n");
                    int length = builder.Length;
                    builder.Append(this.Getter.HelpText);
                    if (builder.Length > length)
                    {
                        flag = true;
                    }
                }
                if (((this.Setter != null) && (this.Setter.HelpText != null)) && (this.Setter.HelpText.Length > 0))
                {
                    if (flag)
                    {
                        builder.Append("\n");
                        flag = false;
                    }
                    builder.Append("set\n");
                    builder.Append(this.Setter.HelpText);
                }
                return (base.helpText = builder.ToString());
            }
            set
            {
                base.HelpText = value;
            }
        }

        public override Member HiddenMember
        {
            get => 
                this.HiddenProperty;
            set
            {
                this.HiddenProperty = (Property) value;
            }
        }

        public virtual Property HiddenProperty
        {
            get
            {
                if (base.hiddenMember == NotSpecified)
                {
                    return null;
                }
                Property hiddenMember = base.hiddenMember as Property;
                if (hiddenMember == null)
                {
                    Method hiddenMethod = this.Getter?.HiddenMethod;
                    Method method2 = this.Setter?.HiddenMethod;
                    Property property2 = (hiddenMethod == null) ? null : (hiddenMethod.DeclaringMember as Property);
                    Property property3 = (method2 == null) ? null : (method2.DeclaringMember as Property);
                    hiddenMember = property2;
                    if ((property3 != null) && ((hiddenMember == null) || ((property3.DeclaringType != null) && property3.DeclaringType.IsDerivedFrom(hiddenMember.DeclaringType))))
                    {
                        hiddenMember = property3;
                    }
                    base.hiddenMember = hiddenMember;
                }
                return hiddenMember;
            }
            set
            {
                base.hiddenMember = value;
            }
        }

        public override bool IsAssembly =>
            (Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.Assembly);

        public override bool IsCompilerControlled =>
            (Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.CompilerControlled);

        public override bool IsFamily =>
            (Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.Family);

        public override bool IsFamilyAndAssembly =>
            (Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.FamANDAssem);

        public override bool IsFamilyOrAssembly =>
            (Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.FamORAssem);

        public bool IsFinal
        {
            get
            {
                if ((this.Getter != null) && !this.Getter.IsFinal)
                {
                    return false;
                }
                if (this.Setter != null)
                {
                    return this.Setter.IsFinal;
                }
                return true;
            }
        }

        public override bool IsPrivate =>
            (Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.Private);

        public override bool IsPublic =>
            (Method.GetVisibilityUnion(this.Getter, this.Setter) == MethodFlags.Public);

        public override bool IsSpecialName =>
            ((this.Flags & PropertyFlags.SpecialName) != PropertyFlags.None);

        public override bool IsStatic
        {
            get
            {
                if ((this.Getter != null) && !this.Getter.IsStatic)
                {
                    return false;
                }
                if (this.Setter != null)
                {
                    return this.Setter.IsStatic;
                }
                return true;
            }
        }

        public bool IsVirtual
        {
            get
            {
                if ((this.Getter != null) && !this.Getter.IsVirtual)
                {
                    return false;
                }
                if (this.Setter != null)
                {
                    return this.Setter.IsVirtual;
                }
                return true;
            }
        }

        public override bool IsVisibleOutsideAssembly =>
            (((this.Getter != null) && this.Getter.IsVisibleOutsideAssembly) || ((this.Setter != null) && this.Setter.IsVisibleOutsideAssembly));

        public MethodList OtherMethods
        {
            get => 
                this.otherMethods;
            set
            {
                this.otherMethods = value;
            }
        }

        public override Member OverriddenMember
        {
            get => 
                this.OverriddenProperty;
            set
            {
                this.OverriddenProperty = (Property) value;
            }
        }

        public virtual Property OverriddenProperty
        {
            get
            {
                if (base.overriddenMember == NotSpecified)
                {
                    return null;
                }
                Property overriddenMember = base.overriddenMember as Property;
                if (overriddenMember == null)
                {
                    Method overriddenMethod = this.Getter?.OverriddenMethod;
                    Method method2 = this.Setter?.OverriddenMethod;
                    Property property2 = (overriddenMethod == null) ? null : (overriddenMethod.DeclaringMember as Property);
                    Property property3 = (method2 == null) ? null : (method2.DeclaringMember as Property);
                    overriddenMember = property2;
                    if ((property3 != null) && ((overriddenMember == null) || ((property3.DeclaringType != null) && property3.DeclaringType.IsDerivedFrom(overriddenMember.DeclaringType))))
                    {
                        overriddenMember = property3;
                    }
                    base.overriddenMember = overriddenMember;
                }
                return overriddenMember;
            }
            set
            {
                base.overriddenMember = value;
            }
        }

        public ParameterList Parameters
        {
            get
            {
                if (this.parameters != null)
                {
                    return this.parameters;
                }
                if (this.Getter != null)
                {
                    return (this.parameters = this.Getter.Parameters);
                }
                ParameterList parameters = this.Setter?.Parameters;
                int capacity = (parameters == null) ? 0 : (parameters.Count - 1);
                ParameterList list4 = this.parameters = new ParameterList(capacity);
                if (parameters != null)
                {
                    for (int i = 0; i < capacity; i++)
                    {
                        list4.Add(parameters[i]);
                    }
                }
                return list4;
            }
            set
            {
                this.parameters = value;
            }
        }

        public Method Setter
        {
            get => 
                this.setter;
            set
            {
                this.setter = value;
            }
        }

        public virtual TypeNode Type
        {
            get
            {
                if (this.type != null)
                {
                    return this.type;
                }
                if (this.Getter != null)
                {
                    return (this.type = this.Getter.ReturnType);
                }
                if ((this.Setter != null) && (this.Setter.Parameters != null))
                {
                    return (this.type = this.Setter.Parameters[this.Setter.Parameters.Count - 1].Type);
                }
                return CoreSystemTypes.Object;
            }
            set
            {
                this.type = value;
            }
        }
    }
}

