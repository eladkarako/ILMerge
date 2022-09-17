namespace System.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Compiler.Metadata;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class Method : Member, IEquatable<Method>
    {
        protected internal Block body;
        private CallingConventionFlags callingConvention;
        protected string conditionalSymbol;
        internal TrivialHashtable contextForOffset;
        protected internal MethodContract contract;
        private Member declaringMember;
        protected bool doesNotHaveAConditionalSymbol;
        internal static readonly MethodContract DummyContract = new MethodContract(null);
        protected ExceptionHandlerList exceptionHandlers;
        private MethodFlags flags;
        protected internal string fullName;
        private MethodList implementedInterfaceMethods;
        private MethodImplFlags implFlags;
        private MethodList implicitlyImplementedInterfaceMethods;
        private bool initLocals;
        protected InstructionList instructions;
        private bool isGeneric;
        protected bool isNormalized;
        public System.Compiler.LocalList LocalList;
        protected System.Reflection.MethodInfo methodInfo;
        public readonly int MethodToken;
        private static Method NotSpecified = new Method();
        private ParameterList parameters;
        protected TypeNode[] parameterTypes;
        private System.Compiler.PInvokeFlags pInvokeFlags;
        private string pInvokeImportName;
        private System.Compiler.Module pInvokeModule;
        public MethodBodyProvider ProvideBody;
        internal MethodContractProvider ProvideContract;
        public MethodAttributeProvider ProvideMethodAttributes;
        public object ProviderHandle;
        private AttributeList returnAttributes;
        private TypeNode returnType;
        private MarshallingInformation returnTypeMarshallingInformation;
        protected SecurityAttributeList securityAttributes;
        private MethodList shallowImplicitlyImplementedInterfaceMethods;
        private Method template;
        private TypeNodeList templateArguments;
        internal TypeNodeList templateParameters;
        private This thisParameter;

        public Method() : base(NodeType.Method)
        {
            this.initLocals = true;
        }

        public Method(MethodBodyProvider provider, object handle) : base(NodeType.Method)
        {
            this.initLocals = true;
            this.ProvideBody = provider;
            this.ProviderHandle = handle;
        }

        public Method(MethodBodyProvider provider, object handle, int methodToken) : this(provider, handle)
        {
            this.MethodToken = methodToken;
        }

        public Method(TypeNode declaringType, AttributeList attributes, Identifier name, ParameterList parameters, TypeNode returnType, Block body) : base(declaringType, attributes, name, NodeType.Method)
        {
            this.initLocals = true;
            this.body = body;
            this.Parameters = parameters;
            this.returnType = returnType;
        }

        public void ClearBody()
        {
            lock (System.Compiler.Module.GlobalLock)
            {
                this.Body = new Block();
                this.Instructions = new InstructionList();
                this.ExceptionHandlers = null;
                this.LocalList = null;
            }
        }

        public Method CreateExplicitImplementation(TypeNode implementingType, ParameterList parameters, StatementList body) => 
            new Method(implementingType, null, base.Name, parameters, this.ReturnType, new Block(body)) { 
                CallingConvention = CallingConventionFlags.HasThis,
                Flags = MethodFlags.NewSlot | MethodFlags.HideBySig | MethodFlags.Virtual | MethodFlags.Final | MethodFlags.Public,
                ImplementedInterfaceMethods = new MethodList(new Method[] { this })
            };

        public bool Equals(Method other) => 
            (this == other);

        protected override Identifier GetDocumentationId()
        {
            if (this.Template != null)
            {
                return this.Template.GetDocumentationId();
            }
            StringBuilder sb = new StringBuilder(this.DeclaringType.DocumentationId.ToString()) {
                [0] = 'M'
            };
            sb.Append('.');
            if (base.NodeType == NodeType.InstanceInitializer)
            {
                sb.Append("#ctor");
            }
            else if (base.Name != null)
            {
                sb.Append(base.Name.ToString());
                if (((TargetPlatform.GenericTypeNamesMangleChar != '\0') && (this.TemplateParameters != null)) && (this.TemplateParameters.Count > 0))
                {
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(TargetPlatform.GenericTypeNamesMangleChar);
                    sb.Append(this.TemplateParameters.Count);
                }
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
                    parameter.Type.AppendDocumentIdMangledName(sb, this.TemplateParameters, this.DeclaringType.TemplateParameters);
                    if (num == (num2 - 1))
                    {
                        sb.Append(')');
                    }
                }
                num++;
            }
            if (((this.IsSpecialName && (this.ReturnType != null)) && (base.Name != null)) && ((base.Name.UniqueIdKey == StandardIds.opExplicit.UniqueIdKey) || (base.Name.UniqueIdKey == StandardIds.opImplicit.UniqueIdKey)))
            {
                sb.Append('~');
                this.ReturnType.AppendDocumentIdMangledName(sb, this.TemplateParameters, this.DeclaringType.TemplateParameters);
            }
            return Identifier.For(sb.ToString());
        }

        public virtual string GetFullUnmangledNameWithTypeParameters() => 
            this.GetFullUnmangledNameWithTypeParameters(false);

        public virtual string GetFullUnmangledNameWithTypeParameters(bool omitParameterTypes)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.DeclaringType.GetFullUnmangledNameWithTypeParameters());
            builder.Append('.');
            builder.Append(this.GetUnmangledNameWithTypeParameters());
            return builder.ToString();
        }

        public static Method GetMethod(System.Reflection.MethodInfo methodInfo)
        {
            if (methodInfo != null)
            {
                if (methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
                {
                    try
                    {
                        Method method = GetMethod(methodInfo.GetGenericMethodDefinition());
                        if (method == null)
                        {
                            return null;
                        }
                        TypeNodeList typeArguments = new TypeNodeList();
                        foreach (Type type in methodInfo.GetGenericArguments())
                        {
                            typeArguments.Add(TypeNode.GetTypeNode(type));
                        }
                        return method.GetTemplateInstance(method.DeclaringType, typeArguments);
                    }
                    catch
                    {
                        return null;
                    }
                }
                TypeNode typeNode = TypeNode.GetTypeNode(methodInfo.DeclaringType);
                if (typeNode != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    int num2 = (parameters == null) ? 0 : parameters.Length;
                    TypeNode[] elements = new TypeNode[num2];
                    for (int i = 0; i < num2; i++)
                    {
                        ParameterInfo info = parameters[i];
                        if (info == null)
                        {
                            return null;
                        }
                        elements[i] = TypeNode.GetTypeNode(info.ParameterType);
                    }
                    TypeNodeList argumentTypes = new TypeNodeList(elements);
                    TypeNode node2 = TypeNode.GetTypeNode(methodInfo.ReturnType);
                    MemberList membersNamed = typeNode.GetMembersNamed(Identifier.For(methodInfo.Name));
                    int num4 = 0;
                    int num5 = (membersNamed == null) ? 0 : membersNamed.Count;
                    while (num4 < num5)
                    {
                        Method method3 = membersNamed[num4] as Method;
                        if (((method3 != null) && method3.ParameterTypesMatch(argumentTypes)) && !(method3.ReturnType != node2))
                        {
                            return method3;
                        }
                        num4++;
                    }
                }
            }
            return null;
        }

        public virtual System.Reflection.MethodInfo GetMethodInfo()
        {
            if (this.methodInfo == null)
            {
                if (this.DeclaringType == null)
                {
                    return null;
                }
                if (this.IsGeneric && (this.Template != null))
                {
                    try
                    {
                        System.Reflection.MethodInfo methodInfo = this.Template.GetMethodInfo();
                        if (methodInfo == null)
                        {
                            return null;
                        }
                        TypeNodeList templateArguments = this.TemplateArguments;
                        Type[] typeArguments = new Type[templateArguments.Count];
                        for (int j = 0; j < templateArguments.Count; j++)
                        {
                            typeArguments[j] = templateArguments[j].GetRuntimeType();
                        }
                        return methodInfo.MakeGenericMethod(typeArguments);
                    }
                    catch
                    {
                        return null;
                    }
                }
                Type runtimeType = this.DeclaringType.GetRuntimeType();
                if (runtimeType == null)
                {
                    return null;
                }
                Type type2 = typeof(object);
                if (!this.isGeneric)
                {
                    type2 = this.ReturnType.GetRuntimeType();
                    if (type2 == null)
                    {
                        return null;
                    }
                }
                ParameterList parameters = this.Parameters;
                int num2 = (parameters == null) ? 0 : parameters.Count;
                Type[] typeArray2 = new Type[num2];
                for (int i = 0; i < num2; i++)
                {
                    Type type4;
                    Parameter parameter = parameters[i];
                    if ((parameter == null) || (parameter.Type == null))
                    {
                        return null;
                    }
                    if (this.isGeneric)
                    {
                        type4 = typeArray2[i] = typeof(object);
                    }
                    else
                    {
                        type4 = typeArray2[i] = parameter.Type.GetRuntimeType();
                    }
                    if (type4 == null)
                    {
                        return null;
                    }
                }
                foreach (System.Reflection.MethodInfo info3 in runtimeType.GetMember(base.Name.ToString(), MemberTypes.Method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (((info3 != null) && (info3.IsStatic == this.IsStatic)) && (info3.ReturnType == type2))
                    {
                        if (info3.IsGenericMethodDefinition)
                        {
                            TypeNodeList templateParameters = this.TemplateParameters;
                            Type[] genericArguments = info3.GetGenericArguments();
                            if (((templateParameters == null) || (genericArguments == null)) || (this.templateParameters.Count != genericArguments.Length))
                            {
                                continue;
                            }
                            int index = 0;
                            int length = genericArguments.Length;
                            while (index < length)
                            {
                                TypeNode node = this.templateParameters[index];
                                Type type6 = genericArguments[index];
                                if (((node != null) && (type6 != null)) && ((node.Name != null) && (node.Name.Name == type6.Name)))
                                {
                                    index++;
                                }
                            }
                        }
                        ParameterInfo[] infoArray3 = info3.GetParameters();
                        int num7 = (infoArray3 == null) ? 0 : infoArray3.Length;
                        if (num7 == num2)
                        {
                            for (int k = 0; k < num2; k++)
                            {
                                ParameterInfo info4 = infoArray3[k];
                                if (info4 != null)
                                {
                                    if (this.isGeneric)
                                    {
                                        Parameter parameter2 = parameters[k];
                                        if (info4.Name == parameter2.Name.Name)
                                        {
                                            continue;
                                        }
                                        continue;
                                    }
                                    if (info4.ParameterType == typeArray2[k])
                                    {
                                    }
                                }
                            }
                            return (this.methodInfo = info3);
                        }
                    }
                }
            }
            return this.methodInfo;
        }

        public virtual TypeNode[] GetParameterTypes()
        {
            if (this.parameterTypes != null)
            {
                return this.parameterTypes;
            }
            ParameterList parameters = this.Parameters;
            int num = (parameters == null) ? 0 : parameters.Count;
            TypeNode[] nodeArray2 = this.parameterTypes = new TypeNode[num];
            for (int i = 0; i < num; i++)
            {
                Parameter parameter = parameters[i];
                if (parameter != null)
                {
                    nodeArray2[i] = parameter.Type;
                }
            }
            return nodeArray2;
        }

        public virtual Method GetTemplateInstance(TypeNode referringType, params TypeNode[] typeArguments) => 
            this.GetTemplateInstance(referringType, new TypeNodeList(typeArguments));

        public virtual Method GetTemplateInstance(TypeNode referringType, TypeNodeList typeArguments)
        {
            if (!this.IsGeneric && ((referringType == null) || (this.DeclaringType == null)))
            {
                return this;
            }
            if (this.IsGeneric)
            {
                referringType = this.DeclaringType;
            }
            if ((referringType != this.DeclaringType) && (referringType.DeclaringModule == this.DeclaringType.DeclaringModule))
            {
                return this.GetTemplateInstance(this.DeclaringType, typeArguments);
            }
            if (referringType.structurallyEquivalentMethod == null)
            {
                referringType.structurallyEquivalentMethod = new TrivialHashtableUsingWeakReferences();
            }
            System.Compiler.Module declaringModule = referringType.DeclaringModule;
            if (declaringModule == null)
            {
                return this;
            }
            int num = (typeArguments == null) ? 0 : typeArguments.Count;
            if ((num == 0) || (typeArguments == null))
            {
                return this;
            }
            Identifier uniqueMangledTemplateInstanceName = TypeNode.GetUniqueMangledTemplateInstanceName(this.UniqueKey, typeArguments);
            lock (this)
            {
                Method method2 = (Method) referringType.structurallyEquivalentMethod[uniqueMangledTemplateInstanceName.UniqueIdKey];
                if (method2 != null)
                {
                    return method2;
                }
                StringBuilder builder = new StringBuilder(base.Name.ToString());
                builder.Append('<');
                for (int i = 0; i < num; i++)
                {
                    TypeNode node = typeArguments[i];
                    if (node != null)
                    {
                        builder.Append(node.FullName);
                        if (i < (num - 1))
                        {
                            builder.Append(',');
                        }
                    }
                }
                builder.Append('>');
                Identifier identifier2 = Identifier.For(builder.ToString());
                Method method = new Duplicator(referringType.DeclaringModule, referringType) { 
                    RecordOriginalAsTemplate = true,
                    SkipBodies = true
                }.VisitMethodInternal(this);
                method.Attributes = this.Attributes;
                method.Name = identifier2;
                method.fullName = null;
                method.template = this;
                method.TemplateArguments = typeArguments;
                TypeNodeList templateParameters = method.TemplateParameters;
                method.TemplateParameters = null;
                method.IsNormalized = true;
                if (!this.IsGeneric)
                {
                    ParameterList parameters = this.Parameters;
                    ParameterList list3 = method.Parameters;
                    if (((parameters != null) && (list3 != null)) && (list3.Count >= parameters.Count))
                    {
                        int num3 = 0;
                        int count = parameters.Count;
                        while (num3 < count)
                        {
                            Parameter parameter = parameters[num3];
                            Parameter parameter2 = list3[num3];
                            if ((parameter != null) && (parameter2 != null))
                            {
                                parameter2.Attributes = parameter.Attributes;
                            }
                            num3++;
                        }
                    }
                }
                if ((!this.IsGeneric && !method.IsStatic) && (this.DeclaringType != referringType))
                {
                    method.Flags &= ~(MethodFlags.NewSlot | MethodFlags.Virtual);
                    method.Flags |= MethodFlags.Static;
                    method.CallingConvention &= ~CallingConventionFlags.HasThis;
                    method.CallingConvention |= CallingConventionFlags.ExplicitThis;
                    ParameterList list4 = method.Parameters;
                    if (list4 == null)
                    {
                        method.Parameters = list4 = new ParameterList(1);
                    }
                    Parameter element = new Parameter(StandardIds.This, this.DeclaringType);
                    list4.Add(element);
                    for (int j = list4.Count - 1; j > 0; j--)
                    {
                        list4[j] = list4[j - 1];
                    }
                    list4[0] = element;
                }
                referringType.structurallyEquivalentMethod[uniqueMangledTemplateInstanceName.UniqueIdKey] = method;
                new Specializer(declaringModule, templateParameters, typeArguments).VisitMethod(method);
                if (this.IsGeneric)
                {
                    method.DeclaringType = this.DeclaringType;
                    return method;
                }
                if (!this.IsAbstract)
                {
                    referringType.Members.Add(method);
                }
                return method;
            }
        }

        public virtual string GetUnmangledNameWithoutTypeParameters() => 
            this.GetUnmangledNameWithoutTypeParameters(false);

        public virtual string GetUnmangledNameWithoutTypeParameters(bool omitParameterTypes)
        {
            StringBuilder builder = new StringBuilder();
            if (base.NodeType == NodeType.InstanceInitializer)
            {
                builder.Append("#ctor");
            }
            else if (base.Name != null)
            {
                string str = base.Name.ToString();
                int num = str.LastIndexOf('.');
                int num2 = str.LastIndexOf('>');
                if (num2 < num)
                {
                    num2 = -1;
                }
                if (num2 > 0)
                {
                    builder.Append(str.Substring(0, num2 + 1));
                }
                else
                {
                    builder.Append(str);
                }
            }
            if (!omitParameterTypes)
            {
                ParameterList parameters = this.Parameters;
                int num3 = 0;
                int num4 = (parameters == null) ? 0 : parameters.Count;
                while (num3 < num4)
                {
                    Parameter parameter = parameters[num3];
                    if ((parameter != null) && (parameter.Type != null))
                    {
                        if (num3 == 0)
                        {
                            builder.Append('(');
                        }
                        else
                        {
                            builder.Append(',');
                        }
                        builder.Append(parameter.Type.GetFullUnmangledNameWithTypeParameters());
                        if (num3 == (num4 - 1))
                        {
                            if (this.IsVarArg)
                            {
                                builder.Append(", __arglist");
                            }
                            builder.Append(')');
                        }
                    }
                    num3++;
                }
            }
            return builder.ToString();
        }

        public virtual string GetUnmangledNameWithTypeParameters() => 
            this.GetUnmangledNameWithTypeParameters(false);

        public virtual string GetUnmangledNameWithTypeParameters(bool omitParameterTypes)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.GetUnmangledNameWithoutTypeParameters(true));
            TypeNodeList templateParameters = this.TemplateParameters;
            int num = 0;
            int num2 = (templateParameters == null) ? 0 : templateParameters.Count;
            while (num < num2)
            {
                TypeNode node = templateParameters[num];
                if (node != null)
                {
                    if (num == 0)
                    {
                        builder.Append('<');
                    }
                    else
                    {
                        builder.Append(',');
                    }
                    builder.Append(node.Name.ToString());
                    if (num == (num2 - 1))
                    {
                        builder.Append('>');
                    }
                }
                num++;
            }
            if (!omitParameterTypes)
            {
                ParameterList parameters = this.Parameters;
                int num3 = 0;
                int num4 = (parameters == null) ? 0 : parameters.Count;
                while (num3 < num4)
                {
                    Parameter parameter = parameters[num3];
                    if ((parameter != null) && (parameter.Type != null))
                    {
                        if (num3 == 0)
                        {
                            builder.Append('(');
                        }
                        else
                        {
                            builder.Append(',');
                        }
                        builder.Append(parameter.Type.GetFullUnmangledNameWithTypeParameters());
                        if (num3 == (num4 - 1))
                        {
                            builder.Append(')');
                        }
                    }
                    num3++;
                }
            }
            return builder.ToString();
        }

        public static MethodFlags GetVisibilityUnion(Method m1, Method m2)
        {
            if ((m1 == null) && (m2 != null))
            {
                return (m2.Flags & MethodFlags.MethodAccessMask);
            }
            if ((m2 == null) && (m1 != null))
            {
                return (m1.Flags & MethodFlags.MethodAccessMask);
            }
            if ((m1 != null) && (m2 != null))
            {
                return GetVisibilityUnion(m1.Flags, m2.Flags);
            }
            return MethodFlags.CompilerControlled;
        }

        public static MethodFlags GetVisibilityUnion(MethodFlags vis1, MethodFlags vis2)
        {
            vis1 &= MethodFlags.MethodAccessMask;
            vis2 &= MethodFlags.MethodAccessMask;
            switch (vis1)
            {
                case MethodFlags.FamANDAssem:
                    switch (vis2)
                    {
                        case MethodFlags.Assembly:
                            return MethodFlags.Assembly;

                        case MethodFlags.Family:
                            return MethodFlags.Family;

                        case MethodFlags.FamORAssem:
                            return MethodFlags.FamORAssem;

                        case MethodFlags.Public:
                            return MethodFlags.Public;
                    }
                    return vis1;

                case MethodFlags.Assembly:
                    switch (vis2)
                    {
                        case MethodFlags.Family:
                        case MethodFlags.FamORAssem:
                            return MethodFlags.FamORAssem;

                        case MethodFlags.Public:
                            return MethodFlags.Public;
                    }
                    return vis1;

                case MethodFlags.Family:
                    switch (vis2)
                    {
                        case MethodFlags.Assembly:
                        case MethodFlags.FamORAssem:
                            return MethodFlags.FamORAssem;

                        case MethodFlags.Family:
                            return vis1;

                        case MethodFlags.Public:
                            return MethodFlags.Public;
                    }
                    return vis1;

                case MethodFlags.FamORAssem:
                    if (vis2 != MethodFlags.Public)
                    {
                        return vis1;
                    }
                    return MethodFlags.Public;

                case MethodFlags.Public:
                    return MethodFlags.Public;
            }
            return vis2;
        }

        public virtual Literal Invoke(Literal targetObject, params Literal[] arguments)
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
            return new Literal(this.Invoke(targetObject.Value, objArray));
        }

        public virtual object Invoke(object targetObject, params object[] arguments) => 
            this.GetMethodInfo()?.Invoke(targetObject, arguments);

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
                    if ((parameter == null) || (parameter2 == null))
                    {
                        return false;
                    }
                    if (parameter.Type != parameter2.Type)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public virtual bool ParametersMatchExceptLast(ParameterList parameters)
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
                for (int i = 0; i < (num - 1); i++)
                {
                    Parameter parameter = list[i];
                    Parameter parameter2 = parameters[i];
                    if ((parameter == null) || (parameter2 == null))
                    {
                        return false;
                    }
                    if (parameter.Type != parameter2.Type)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public virtual bool ParametersMatchIncludingOutFlag(ParameterList parameters)
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
                    if ((parameter.Flags & ParameterFlags.Out) != (parameter2.Flags & ParameterFlags.Out))
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

        public virtual bool ParametersMatchStructurallyExceptLast(ParameterList parameters)
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
                for (int i = 0; i < (num - 1); i++)
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

        public virtual bool ParametersMatchStructurallyIncludingOutFlag(ParameterList parameters) => 
            this.ParametersMatchStructurallyIncludingOutFlag(parameters, false);

        public virtual bool ParametersMatchStructurallyIncludingOutFlag(ParameterList parameters, bool allowCoVariance)
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
                    if ((parameter.Flags & ParameterFlags.Out) != (parameter2.Flags & ParameterFlags.Out))
                    {
                        return false;
                    }
                    if ((parameter.Type != parameter2.Type) && !parameter.Type.IsStructurallyEquivalentTo(parameter2.Type))
                    {
                        return ((allowCoVariance && !parameter2.Type.IsValueType) && parameter2.Type.IsAssignableTo(parameter.Type));
                    }
                }
            }
            return true;
        }

        public virtual bool ParameterTypesMatch(TypeNodeList argumentTypes)
        {
            int num = (this.Parameters == null) ? 0 : this.Parameters.Count;
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
                    TypeNode type = argumentTypes[i];
                    if (parameter.Type != type)
                    {
                        TypeNode node2 = TypeNode.StripModifiers(parameter.Type);
                        type = TypeNode.StripModifiers(type);
                        if (node2 != type)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public virtual bool ParameterTypesMatchStructurally(TypeNodeList argumentTypes)
        {
            int num = (this.Parameters == null) ? 0 : this.Parameters.Count;
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
                    TypeNode type = argumentTypes[i];
                    if (parameter.Type != type)
                    {
                        TypeNode node2 = TypeNode.StripModifiers(parameter.Type);
                        type = TypeNode.StripModifiers(type);
                        if ((node2 == null) || !node2.IsStructurallyEquivalentTo(type))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        internal void RecordSequencePoints(ISymUnmanagedMethod methodInfo, Dictionary<IntPtr, UnmanagedDocument> documentCache)
        {
            if ((methodInfo != null) && (this.contextForOffset == null))
            {
                uint num2;
                uint sequencePointCount = methodInfo.GetSequencePointCount();
                this.contextForOffset = new TrivialHashtable((int) sequencePointCount);
                IntPtr[] documents = new IntPtr[sequencePointCount];
                uint[] lines = new uint[sequencePointCount];
                uint[] columns = new uint[sequencePointCount];
                uint[] endLines = new uint[sequencePointCount];
                uint[] endColumns = new uint[sequencePointCount];
                uint[] offsets = new uint[sequencePointCount];
                methodInfo.GetSequencePoints(sequencePointCount, out num2, offsets, documents, lines, columns, endLines, endColumns);
                for (int i = 0; i < sequencePointCount; i++)
                {
                    if ((lines[i] >= 0xfeefee) || (endLines[i] >= 0xfeefee))
                    {
                        this.contextForOffset[((int) offsets[i]) + 1] = new SourceContext(HiddenDocument.Document);
                    }
                    else
                    {
                        UnmanagedDocument document = UnmanagedDocument.For(documentCache, documents[i]);
                        this.contextForOffset[((int) offsets[i]) + 1] = new SourceContext(document, document.GetOffset(lines[i], columns[i]), document.GetOffset(endLines[i], endColumns[i]));
                    }
                }
                for (int j = 0; j < sequencePointCount; j++)
                {
                    Marshal.Release(documents[j]);
                }
            }
        }

        public void SetDelayedContract(MethodContractProvider provider)
        {
            if (this.ProvideContract != null)
            {
                this.ProvideContract = (MethodContractProvider) Delegate.Combine(this.ProvideContract, provider);
            }
            else
            {
                this.ProvideContract = provider;
            }
            this.contract = null;
        }

        public virtual bool TemplateParametersMatch(TypeNodeList templateParameters)
        {
            TypeNodeList list = this.TemplateParameters;
            if (list == null)
            {
                if (templateParameters != null)
                {
                    return (templateParameters.Count == 0);
                }
                return true;
            }
            if (templateParameters == null)
            {
                return false;
            }
            int count = list.Count;
            if (count != templateParameters.Count)
            {
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                TypeNode node = list[i];
                TypeNode type = templateParameters[i];
                if ((node == null) || (type == null))
                {
                    return false;
                }
                if ((node != type) && !node.IsStructurallyEquivalentTo(type))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString() => 
            (this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "." + base.Name);

        private static bool TypeListsAreEquivalent(TypeNodeList list1, TypeNodeList list2)
        {
            if ((list1 == null) || (list2 == null))
            {
                return (list1 == list2);
            }
            int count = list1.Count;
            if (count != list2.Count)
            {
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                if (list1[i] != list2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool TypeParameterCountsMatch(Method meth2)
        {
            if (meth2 == null)
            {
                return false;
            }
            int num = (this.TemplateParameters == null) ? 0 : this.TemplateParameters.Count;
            int num2 = (meth2.TemplateParameters == null) ? 0 : meth2.TemplateParameters.Count;
            return (num == num2);
        }

        public override AttributeList Attributes
        {
            get
            {
                if (base.attributes == null)
                {
                    if ((this.ProvideMethodAttributes != null) && (this.ProviderHandle != null))
                    {
                        lock (System.Compiler.Module.GlobalLock)
                        {
                            if (base.attributes == null)
                            {
                                this.ProvideMethodAttributes(this, this.ProviderHandle);
                            }
                            goto Label_0062;
                        }
                    }
                    base.attributes = new AttributeList(0);
                }
            Label_0062:
                return base.attributes;
            }
            set
            {
                base.attributes = value;
            }
        }

        public virtual Block Body
        {
            get
            {
                if ((this.body == null) && ((this.ProvideBody != null) && (this.ProviderHandle != null)))
                {
                    lock (System.Compiler.Module.GlobalLock)
                    {
                        if (this.body == null)
                        {
                            this.ProvideBody(this, this.ProviderHandle, false);
                        }
                    }
                }
                return this.body;
            }
            set
            {
                this.body = value;
            }
        }

        public CallingConventionFlags CallingConvention
        {
            get => 
                this.callingConvention;
            set
            {
                this.callingConvention = value;
            }
        }

        public string ConditionalSymbol
        {
            get
            {
                if (this.doesNotHaveAConditionalSymbol)
                {
                    return null;
                }
                if (this.conditionalSymbol == null)
                {
                    lock (this)
                    {
                        if (this.conditionalSymbol != null)
                        {
                            return this.conditionalSymbol;
                        }
                        AttributeNode attribute = this.GetAttribute(SystemTypes.ConditionalAttribute);
                        if (((attribute != null) && (attribute.Expressions != null)) && (attribute.Expressions.Count > 0))
                        {
                            Literal literal = attribute.Expressions[0] as Literal;
                            if (literal != null)
                            {
                                this.conditionalSymbol = literal.Value as string;
                                if (this.conditionalSymbol != null)
                                {
                                    return this.conditionalSymbol;
                                }
                            }
                        }
                        this.doesNotHaveAConditionalSymbol = true;
                    }
                }
                return this.conditionalSymbol;
            }
            set
            {
                this.conditionalSymbol = value;
            }
        }

        public virtual MethodContract Contract
        {
            get
            {
                if ((this.contract == null) && ((this.ProvideContract != null) && (this.ProviderHandle != null)))
                {
                    lock (System.Compiler.Module.GlobalLock)
                    {
                        if (this.contract == null)
                        {
                            MethodContractProvider provideContract = this.ProvideContract;
                            this.ProvideContract = null;
                            provideContract(this, this.ProviderHandle);
                        }
                    }
                }
                return this.contract;
            }
            set
            {
                this.contract = value;
                if (value != null)
                {
                    this.contract.DeclaringMethod = this;
                }
                this.ProvideContract = null;
            }
        }

        public Member DeclaringMember
        {
            get
            {
                if (((this.declaringMember == null) && (this.DeclaringType != null)) && !this.DeclaringType.membersBeingPopulated)
                {
                    MemberList members = this.DeclaringType.Members;
                }
                return this.declaringMember;
            }
            set
            {
                this.declaringMember = value;
            }
        }

        public virtual ExceptionHandlerList ExceptionHandlers
        {
            get
            {
                if (this.exceptionHandlers == null)
                {
                    Block body = this.Body;
                    if (this.exceptionHandlers == null)
                    {
                        this.exceptionHandlers = new ExceptionHandlerList(0);
                    }
                }
                return this.exceptionHandlers;
            }
            set
            {
                this.exceptionHandlers = value;
            }
        }

        public MethodFlags Flags
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
                if (this.DeclaringType != null)
                {
                    builder.Append(this.DeclaringType.FullName);
                    builder.Append('.');
                    if (base.NodeType == NodeType.InstanceInitializer)
                    {
                        builder.Append("#ctor");
                    }
                    else if (base.Name != null)
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
                }
                return (this.fullName = builder.ToString());
            }
        }

        public override Member HiddenMember
        {
            get => 
                this.HiddenMethod;
            set
            {
                this.HiddenMethod = (Method) value;
            }
        }

        public virtual Method HiddenMethod
        {
            get
            {
                if (base.hiddenMember == NotSpecified)
                {
                    return null;
                }
                Method hiddenMember = base.hiddenMember as Method;
                if (hiddenMember == null)
                {
                    if (this.ProvideBody == null)
                    {
                        return null;
                    }
                    if (this.IsVirtual && ((this.Flags & MethodFlags.NewSlot) != MethodFlags.NewSlot))
                    {
                        return null;
                    }
                    for (TypeNode node = this.DeclaringType.BaseType; node != null; node = node.BaseType)
                    {
                        MemberList membersNamed = node.GetMembersNamed(base.Name);
                        if (membersNamed != null)
                        {
                            int num = 0;
                            int count = membersNamed.Count;
                            while (num < count)
                            {
                                Method method2 = membersNamed[num] as Method;
                                if ((method2 != null) && (method2.ParametersMatch(this.Parameters) || (((this.TemplateParameters != null) && this.TemplateParametersMatch(method2.TemplateParameters)) && method2.ParametersMatchStructurally(this.Parameters))))
                                {
                                    hiddenMember = method2;
                                    break;
                                }
                                num++;
                            }
                        }
                    }
                    if (hiddenMember == null)
                    {
                        base.hiddenMember = NotSpecified;
                        return null;
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

        public MethodList ImplementedInterfaceMethods
        {
            get => 
                this.implementedInterfaceMethods;
            set
            {
                this.implementedInterfaceMethods = value;
            }
        }

        public MethodImplFlags ImplFlags
        {
            get => 
                this.implFlags;
            set
            {
                this.implFlags = value;
            }
        }

        public MethodList ImplicitlyImplementedInterfaceMethods
        {
            get
            {
                if (this.implicitlyImplementedInterfaceMethods == null)
                {
                    this.implicitlyImplementedInterfaceMethods = new MethodList();
                    if ((this.DeclaringType != null) && (this.DeclaringType.NodeType == NodeType.Interface))
                    {
                        return this.implicitlyImplementedInterfaceMethods;
                    }
                    if (((this.ImplementedInterfaceMethods == null) || (this.ImplementedInterfaceMethods.Count == 0)) && (this.IsPublic && !this.IsStatic))
                    {
                        if ((this.DeclaringType != null) && (this.DeclaringType.Interfaces != null))
                        {
                            InterfaceList.Enumerator enumerator = this.DeclaringType.Interfaces.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                Interface current = enumerator.Current;
                                if (current != null)
                                {
                                    Method exactMatchingMethod = current.GetExactMatchingMethod(this);
                                    if (((exactMatchingMethod != null) && exactMatchingMethod.ReturnType.IsStructurallyEquivalentTo(this.ReturnType)) && !this.DeclaringType.ImplementsExplicitly(exactMatchingMethod))
                                    {
                                        this.implicitlyImplementedInterfaceMethods.Add(exactMatchingMethod);
                                    }
                                }
                            }
                        }
                        if (this.OverriddenMethod != null)
                        {
                            MethodList.Enumerator enumerator2 = this.OverriddenMethod.ImplicitlyImplementedInterfaceMethods.GetEnumerator();
                            while (enumerator2.MoveNext())
                            {
                                Method method = enumerator2.Current;
                                if (!this.DeclaringType.ImplementsExplicitly(method))
                                {
                                    int num = 0;
                                    int count = this.implicitlyImplementedInterfaceMethods.Count;
                                    while (num < count)
                                    {
                                        Method method3 = this.implicitlyImplementedInterfaceMethods[num];
                                        if (method3 == method)
                                        {
                                            break;
                                        }
                                        num++;
                                    }
                                    if (num == count)
                                    {
                                        this.implicitlyImplementedInterfaceMethods.Add(method);
                                    }
                                }
                            }
                        }
                    }
                }
                return this.implicitlyImplementedInterfaceMethods;
            }
            set
            {
                this.implicitlyImplementedInterfaceMethods = value;
            }
        }

        public bool InitLocals
        {
            get => 
                this.initLocals;
            set
            {
                this.initLocals = value;
            }
        }

        public virtual InstructionList Instructions
        {
            get
            {
                if ((this.instructions == null) && ((this.ProvideBody != null) && (this.ProviderHandle != null)))
                {
                    lock (System.Compiler.Module.GlobalLock)
                    {
                        if (this.instructions == null)
                        {
                            this.ProvideBody(this, this.ProviderHandle, true);
                        }
                    }
                }
                return this.instructions;
            }
            set
            {
                this.instructions = value;
            }
        }

        public virtual bool IsAbstract =>
            ((this.Flags & MethodFlags.Abstract) != MethodFlags.CompilerControlled);

        public override bool IsAssembly =>
            ((this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.Assembly);

        public override bool IsCompilerControlled =>
            ((this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.CompilerControlled);

        public virtual bool IsExtern
        {
            get
            {
                if ((this.Flags & MethodFlags.PInvokeImpl) == MethodFlags.CompilerControlled)
                {
                    return ((this.ImplFlags & (MethodImplFlags.InternalCall | MethodImplFlags.CodeTypeMask)) != MethodImplFlags.IL);
                }
                return true;
            }
        }

        public override bool IsFamily =>
            ((this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.Family);

        public override bool IsFamilyAndAssembly =>
            ((this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.FamANDAssem);

        public override bool IsFamilyOrAssembly =>
            ((this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.FamORAssem);

        public virtual bool IsFieldInitializerMethod =>
            false;

        public virtual bool IsFinal =>
            ((this.Flags & MethodFlags.Final) != MethodFlags.CompilerControlled);

        public bool IsGeneric
        {
            get => 
                this.isGeneric;
            set
            {
                this.isGeneric = value;
            }
        }

        public virtual bool IsInternalCall =>
            ((this.ImplFlags & MethodImplFlags.InternalCall) != MethodImplFlags.IL);

        public virtual bool IsNonSealedVirtual
        {
            get
            {
                if (((this.Flags & MethodFlags.Virtual) == MethodFlags.CompilerControlled) || ((this.Flags & MethodFlags.Final) != MethodFlags.CompilerControlled))
                {
                    return false;
                }
                if (this.DeclaringType != null)
                {
                    return ((this.DeclaringType.Flags & TypeFlags.Sealed) == TypeFlags.AnsiClass);
                }
                return true;
            }
        }

        public virtual bool IsNormalized
        {
            get => 
                (this.isNormalized || (((this.DeclaringType != null) && (this.SourceContext.Document == null)) && (this.isNormalized = this.DeclaringType.IsNormalized)));
            set
            {
                this.isNormalized = value;
            }
        }

        public override bool IsPrivate =>
            ((this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.Private);

        public bool IsPropertyGetter
        {
            get
            {
                if (this.DeclaringMember != null)
                {
                    Property declaringMember = this.DeclaringMember as Property;
                    if (declaringMember == null)
                    {
                        return false;
                    }
                    if (declaringMember.Getter == this)
                    {
                        return true;
                    }
                    if (this.Template != null)
                    {
                        declaringMember = this.Template.DeclaringMember as Property;
                        if (declaringMember != null)
                        {
                            return (declaringMember.Getter == this.Template);
                        }
                    }
                }
                return false;
            }
        }

        public bool IsPropertySetter
        {
            get
            {
                if (this.DeclaringMember != null)
                {
                    Property declaringMember = this.DeclaringMember as Property;
                    if (declaringMember == null)
                    {
                        return false;
                    }
                    if (declaringMember.Setter == this)
                    {
                        return true;
                    }
                    if (this.Template != null)
                    {
                        declaringMember = this.Template.DeclaringMember as Property;
                        if (declaringMember != null)
                        {
                            return (declaringMember.Setter == this.Template);
                        }
                    }
                }
                return false;
            }
        }

        public override bool IsPublic =>
            ((this.Flags & MethodFlags.MethodAccessMask) == MethodFlags.Public);

        public override bool IsSpecialName =>
            ((this.Flags & MethodFlags.SpecialName) != MethodFlags.CompilerControlled);

        public override bool IsStatic =>
            ((this.Flags & MethodFlags.Static) != MethodFlags.CompilerControlled);

        public bool IsVarArg =>
            ((this.CallingConvention & CallingConventionFlags.VarArg) != CallingConventionFlags.Default);

        public virtual bool IsVirtual =>
            ((this.Flags & MethodFlags.Virtual) != MethodFlags.CompilerControlled);

        public virtual bool IsVirtualAndNotDeclaredInStruct
        {
            get
            {
                if ((this.Flags & MethodFlags.Virtual) == MethodFlags.CompilerControlled)
                {
                    return false;
                }
                if (this.DeclaringType != null)
                {
                    return !(this.DeclaringType is Struct);
                }
                return true;
            }
        }

        public override bool IsVisibleOutsideAssembly
        {
            get
            {
                if ((this.DeclaringType == null) || this.DeclaringType.IsVisibleOutsideAssembly)
                {
                    switch ((this.Flags & MethodFlags.MethodAccessMask))
                    {
                        case MethodFlags.Family:
                        case MethodFlags.FamORAssem:
                            if ((this.DeclaringType == null) || this.DeclaringType.IsSealed)
                            {
                                break;
                            }
                            return true;

                        case MethodFlags.Public:
                            return true;
                    }
                    int num = 0;
                    int num2 = (this.ImplementedInterfaceMethods == null) ? 0 : this.ImplementedInterfaceMethods.Count;
                    while (num < num2)
                    {
                        Method method = this.ImplementedInterfaceMethods[num];
                        if (((method != null) && ((method.DeclaringType == null) || method.DeclaringType.IsVisibleOutsideAssembly)) && method.IsVisibleOutsideAssembly)
                        {
                            return true;
                        }
                        num++;
                    }
                }
                return false;
            }
        }

        public override Member OverriddenMember
        {
            get => 
                this.OverriddenMethod;
            set
            {
                this.OverriddenMethod = (Method) value;
            }
        }

        public virtual Method OverriddenMethod
        {
            get
            {
                if ((this.Flags & MethodFlags.NewSlot) == MethodFlags.NewSlot)
                {
                    return null;
                }
                if (base.overriddenMember == NotSpecified)
                {
                    return null;
                }
                Method overriddenMember = base.overriddenMember as Method;
                if (overriddenMember == null)
                {
                    if (this.ProvideBody == null)
                    {
                        return null;
                    }
                    if (!this.IsVirtual)
                    {
                        return null;
                    }
                    for (TypeNode node = this.DeclaringType.BaseType; node != null; node = node.BaseType)
                    {
                        MemberList membersNamed = node.GetMembersNamed(base.Name);
                        if (membersNamed != null)
                        {
                            int num = 0;
                            int count = membersNamed.Count;
                            while (num < count)
                            {
                                Method method2 = membersNamed[num] as Method;
                                if ((method2 != null) && (method2.ParametersMatch(this.Parameters) || (((this.TemplateParameters != null) && this.TemplateParametersMatch(method2.TemplateParameters)) && method2.ParametersMatchStructurally(this.Parameters))))
                                {
                                    overriddenMember = method2;
                                    break;
                                }
                                num++;
                            }
                        }
                    }
                    if (overriddenMember == null)
                    {
                        base.overriddenMember = NotSpecified;
                        return null;
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

        public int ParameterCount =>
            this.parameters?.Count;

        public ParameterList Parameters
        {
            get => 
                this.parameters;
            set
            {
                this.parameters = value;
                if (value != null)
                {
                    int num = 0;
                    int count = value.Count;
                    while (num < count)
                    {
                        Parameter parameter = this.parameters[num];
                        if (parameter != null)
                        {
                            parameter.DeclaringMethod = this;
                        }
                        num++;
                    }
                }
            }
        }

        public System.Compiler.PInvokeFlags PInvokeFlags
        {
            get => 
                this.pInvokeFlags;
            set
            {
                this.pInvokeFlags = value;
            }
        }

        public string PInvokeImportName
        {
            get => 
                this.pInvokeImportName;
            set
            {
                this.pInvokeImportName = value;
            }
        }

        public System.Compiler.Module PInvokeModule
        {
            get => 
                this.pInvokeModule;
            set
            {
                this.pInvokeModule = value;
            }
        }

        public AttributeList ReturnAttributes
        {
            get => 
                this.returnAttributes;
            set
            {
                this.returnAttributes = value;
            }
        }

        public TypeNode ReturnType
        {
            get => 
                this.returnType;
            set
            {
                this.returnType = value;
            }
        }

        public MarshallingInformation ReturnTypeMarshallingInformation
        {
            get => 
                this.returnTypeMarshallingInformation;
            set
            {
                this.returnTypeMarshallingInformation = value;
            }
        }

        public SecurityAttributeList SecurityAttributes
        {
            get
            {
                if (this.securityAttributes != null)
                {
                    return this.securityAttributes;
                }
                if (base.attributes == null)
                {
                    if (this.Attributes != null)
                    {
                    }
                    if (this.securityAttributes != null)
                    {
                        return this.securityAttributes;
                    }
                }
                return (this.securityAttributes = new SecurityAttributeList(0));
            }
            set
            {
                this.securityAttributes = value;
            }
        }

        public MethodList ShallowImplicitlyImplementedInterfaceMethods
        {
            get
            {
                if (this.shallowImplicitlyImplementedInterfaceMethods == null)
                {
                    this.shallowImplicitlyImplementedInterfaceMethods = new MethodList();
                    if ((this.DeclaringType != null) && (this.DeclaringType.NodeType == NodeType.Interface))
                    {
                        return this.shallowImplicitlyImplementedInterfaceMethods;
                    }
                    if (((this.ImplementedInterfaceMethods == null) || (this.ImplementedInterfaceMethods.Count == 0)) && ((this.IsPublic && !this.IsStatic) && ((this.DeclaringType != null) && (this.DeclaringType.Interfaces != null))))
                    {
                        InterfaceList.Enumerator enumerator = this.DeclaringType.Interfaces.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Interface current = enumerator.Current;
                            if (current != null)
                            {
                                Method exactMatchingMethod = current.GetExactMatchingMethod(this);
                                if (((exactMatchingMethod != null) && exactMatchingMethod.ReturnType.IsStructurallyEquivalentTo(this.ReturnType)) && !this.DeclaringType.ImplementsExplicitly(exactMatchingMethod))
                                {
                                    this.shallowImplicitlyImplementedInterfaceMethods.Add(exactMatchingMethod);
                                }
                            }
                        }
                    }
                }
                return this.shallowImplicitlyImplementedInterfaceMethods;
            }
        }

        public Method Template
        {
            get
            {
                Method template = this.template;
                if (template == NotSpecified)
                {
                    return null;
                }
                return template;
            }
            set
            {
                this.template = value;
            }
        }

        public TypeNodeList TemplateArguments
        {
            get => 
                this.templateArguments;
            set
            {
                this.templateArguments = value;
            }
        }

        public virtual TypeNodeList TemplateParameters
        {
            get => 
                this.templateParameters;
            set
            {
                this.templateParameters = value;
            }
        }

        public This ThisParameter
        {
            get
            {
                if (((this.thisParameter == null) && !this.IsStatic) && (this.DeclaringType != null))
                {
                    if (this.DeclaringType.IsValueType)
                    {
                        this.ThisParameter = new This(this.DeclaringType.SelfInstantiation().GetReferenceType());
                    }
                    else
                    {
                        this.ThisParameter = new This(this.DeclaringType.SelfInstantiation());
                    }
                }
                return this.thisParameter;
            }
            set
            {
                if (value != null)
                {
                    value.DeclaringMethod = this;
                }
                this.thisParameter = value;
            }
        }

        public delegate void MethodAttributeProvider(Method method, object handle);

        public delegate void MethodBodyProvider(Method method, object handle, bool asInstructionList);

        public delegate void MethodContractProvider(Method method, object handle);
    }
}

