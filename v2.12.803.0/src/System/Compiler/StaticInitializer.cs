namespace System.Compiler
{
    using System;
    using System.Reflection;

    internal class StaticInitializer : Method
    {
        protected ConstructorInfo constructorInfo;

        public StaticInitializer()
        {
            base.NodeType = NodeType.StaticInitializer;
            base.Flags = MethodFlags.RTSpecialName | MethodFlags.SpecialName | MethodFlags.HideBySig | MethodFlags.Static | MethodFlags.Private;
            base.Name = StandardIds.CCtor;
            base.ReturnType = CoreSystemTypes.Void;
        }

        public StaticInitializer(Method.MethodBodyProvider provider, object handle) : base(provider, handle)
        {
            base.NodeType = NodeType.StaticInitializer;
        }

        public StaticInitializer(Method.MethodBodyProvider provider, object handle, int methodToken) : base(provider, handle, methodToken)
        {
            base.NodeType = NodeType.StaticInitializer;
        }

        public StaticInitializer(TypeNode declaringType, AttributeList attributes, Block body) : base(declaringType, attributes, StandardIds.CCtor, null, null, body)
        {
            base.NodeType = NodeType.StaticInitializer;
            base.Flags = MethodFlags.RTSpecialName | MethodFlags.SpecialName | MethodFlags.HideBySig | MethodFlags.Static | MethodFlags.Private;
            base.Name = StandardIds.CCtor;
            base.ReturnType = CoreSystemTypes.Void;
        }

        public StaticInitializer(TypeNode declaringType, AttributeList attributes, Block body, TypeNode voidTypeExpression) : base(declaringType, attributes, StandardIds.CCtor, null, null, body)
        {
            base.NodeType = NodeType.StaticInitializer;
            base.Flags = MethodFlags.RTSpecialName | MethodFlags.SpecialName | MethodFlags.HideBySig | MethodFlags.Static | MethodFlags.Private;
            base.Name = StandardIds.CCtor;
            base.ReturnType = voidTypeExpression;
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
                foreach (ConstructorInfo info in runtimeType.GetMember(base.Name.ToString(), MemberTypes.Constructor, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (info != null)
                    {
                        ParameterInfo[] infoArray3 = info.GetParameters();
                        int num4 = (infoArray3 == null) ? 0 : infoArray3.Length;
                        if (num4 == num)
                        {
                            if (infoArray3 != null)
                            {
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
            }
            return this.constructorInfo;
        }

        public override System.Reflection.MethodInfo GetMethodInfo() => 
            null;

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

