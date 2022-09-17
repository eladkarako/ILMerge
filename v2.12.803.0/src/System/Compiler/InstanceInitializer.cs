namespace System.Compiler
{
    using System;
    using System.Reflection;

    internal class InstanceInitializer : Method
    {
        public Block BaseOrDefferingCallBlock;
        protected ConstructorInfo constructorInfo;
        public bool ContainsBaseMarkerBecauseOfNonNullFields;
        public bool IsCompilerGenerated;
        public bool IsDeferringConstructor;

        public InstanceInitializer()
        {
            base.NodeType = NodeType.InstanceInitializer;
            base.CallingConvention = CallingConventionFlags.HasThis;
            base.Flags = MethodFlags.RTSpecialName | MethodFlags.SpecialName;
            base.Name = StandardIds.Ctor;
            base.ReturnType = CoreSystemTypes.Void;
        }

        public InstanceInitializer(Method.MethodBodyProvider provider, object handle) : base(provider, handle)
        {
            base.NodeType = NodeType.InstanceInitializer;
        }

        public InstanceInitializer(Method.MethodBodyProvider provider, object handle, int methodToken) : base(provider, handle, methodToken)
        {
            base.NodeType = NodeType.InstanceInitializer;
        }

        public InstanceInitializer(TypeNode declaringType, AttributeList attributes, ParameterList parameters, Block body) : this(declaringType, attributes, parameters, body, CoreSystemTypes.Void)
        {
        }

        public InstanceInitializer(TypeNode declaringType, AttributeList attributes, ParameterList parameters, Block body, TypeNode returnType) : base(declaringType, attributes, StandardIds.Ctor, parameters, null, body)
        {
            base.NodeType = NodeType.InstanceInitializer;
            base.CallingConvention = CallingConventionFlags.HasThis;
            base.Flags = MethodFlags.RTSpecialName | MethodFlags.SpecialName;
            base.Name = StandardIds.Ctor;
            base.ReturnType = returnType;
        }

        public virtual MemberList GetAttributeConstructorNamedParameters()
        {
            TypeNode declaringType = this.DeclaringType;
            if (((declaringType == null) || !declaringType.IsAssignableTo(SystemTypes.Attribute)) || (declaringType.Members == null))
            {
                return null;
            }
            MemberList members = declaringType.Members;
            int count = members.Count;
            MemberList list2 = new MemberList(members.Count);
            for (int i = 0; i < count; i++)
            {
                Property element = members[i] as Property;
                if ((element != null) && element.IsPublic)
                {
                    if ((element.Setter != null) && (element.Getter != null))
                    {
                        list2.Add(element);
                    }
                }
                else
                {
                    Field field = members[i] as Field;
                    if (((field != null) && !field.IsInitOnly) && field.IsPublic)
                    {
                        list2.Add(field);
                    }
                }
            }
            return list2;
        }

        public virtual ConstructorInfo GetConstructorInfo()
        {
            if (this.constructorInfo == null)
            {
                if (this.DeclaringType == null)
                {
                    return null;
                }
                Type runtimeType = this.DeclaringType.GetRuntimeType();
                if (runtimeType == null)
                {
                    return null;
                }
                ParameterList parameters = base.Parameters;
                int num = (parameters == null) ? 0 : parameters.Count;
                Type[] typeArray = new Type[num];
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
                foreach (ConstructorInfo info in runtimeType.GetMember(base.Name.ToString(), MemberTypes.Constructor, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (info != null)
                    {
                        ParameterInfo[] infoArray3 = info.GetParameters();
                        if (infoArray3 != null)
                        {
                            if (infoArray3.Length != num)
                            {
                                continue;
                            }
                            for (int j = 0; j < num; j++)
                            {
                                ParameterInfo info2 = infoArray3[j];
                                if ((info2 != null) && (info2.ParameterType == typeArray[j]))
                                {
                                }
                            }
                        }
                        return (this.constructorInfo = info);
                    }
                }
            }
            return this.constructorInfo;
        }

        public override System.Reflection.MethodInfo GetMethodInfo() => 
            null;

        public virtual Literal Invoke(params Literal[] arguments)
        {
            int num = (arguments == null) ? 0 : arguments.Length;
            object[] objArray = (num == 0) ? null : new object[num];
            if ((objArray != null) && (arguments != null))
            {
                for (int i = 0; i < num; i++)
                {
                    Literal literal = arguments[i];
                    objArray[i] = literal?.Value;
                }
            }
            return new Literal(this.Invoke(objArray));
        }

        public virtual object Invoke(params object[] arguments) => 
            this.GetConstructorInfo()?.Invoke(arguments);

        public override string ToString() => 
            string.Concat(new object[] { this.DeclaringType.GetFullUnmangledNameWithTypeParameters(), "(", base.Parameters, ")" });

        public override Member OverriddenMember
        {
            get => 
                null;
            set
            {
            }
        }

        public override Method OverriddenMethod
        {
            get => 
                null;
            set
            {
            }
        }

        public override bool OverridesBaseClassMember
        {
            get => 
                false;
            set
            {
            }
        }
    }
}

