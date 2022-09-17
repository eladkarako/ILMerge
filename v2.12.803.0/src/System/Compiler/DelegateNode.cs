namespace System.Compiler
{
    using System;

    internal class DelegateNode : TypeNode
    {
        internal static readonly DelegateNode Dummy = new DelegateNode();
        private bool membersAlreadyProvided;
        protected ParameterList parameters;
        protected TypeNode returnType;
        public TypeNode ReturnTypeExpression;

        public DelegateNode() : base(NodeType.DelegateNode)
        {
        }

        public DelegateNode(TypeNode.NestedTypeProvider provideNestedTypes, TypeNode.TypeAttributeProvider provideAttributes, TypeNode.TypeMemberProvider provideMembers, object handle) : base(NodeType.DelegateNode, provideNestedTypes, provideAttributes, provideMembers, handle)
        {
        }

        public DelegateNode(Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags, Identifier Namespace, Identifier name, TypeNode returnType, ParameterList parameters) : base(declaringModule, declaringType, attributes, flags, Namespace, name, null, null, NodeType.DelegateNode)
        {
            this.parameters = parameters;
            this.returnType = returnType;
        }

        public virtual void ProvideMembers()
        {
            if (!this.membersAlreadyProvided)
            {
                this.membersAlreadyProvided = true;
                base.memberCount = 0;
                MemberList list2 = base.members = new MemberList();
                ParameterList parameters = new ParameterList(2);
                parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.Object, CoreSystemTypes.Object, null, null));
                parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.Method, CoreSystemTypes.IntPtr, null, null));
                InstanceInitializer element = new InstanceInitializer(this, null, parameters, null);
                element.Flags |= MethodFlags.HideBySig | MethodFlags.Public;
                element.CallingConvention = CallingConventionFlags.HasThis;
                element.ImplFlags = MethodImplFlags.CodeTypeMask;
                list2.Add(element);
                Method method = new Method(this, null, StandardIds.Invoke, this.Parameters, this.ReturnType, null) {
                    Flags = MethodFlags.NewSlot | MethodFlags.HideBySig | MethodFlags.Virtual | MethodFlags.Public,
                    CallingConvention = CallingConventionFlags.HasThis,
                    ImplFlags = MethodImplFlags.CodeTypeMask
                };
                list2.Add(method);
                if (SystemTypes.AsyncCallback.ReturnType != null)
                {
                    ParameterList list4 = this.parameters;
                    int num = (list4 == null) ? 0 : list4.Count;
                    parameters = new ParameterList(num + 2);
                    for (int i = 0; i < num; i++)
                    {
                        Parameter parameter = list4[i];
                        if (parameter != null)
                        {
                            parameters.Add((Parameter) parameter.Clone());
                        }
                    }
                    parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.callback, SystemTypes.AsyncCallback, null, null));
                    parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.Object, CoreSystemTypes.Object, null, null));
                    Method method2 = new Method(this, null, StandardIds.BeginInvoke, parameters, SystemTypes.IASyncResult, null) {
                        Flags = MethodFlags.NewSlot | MethodFlags.HideBySig | MethodFlags.Virtual | MethodFlags.Public,
                        CallingConvention = CallingConventionFlags.HasThis,
                        ImplFlags = MethodImplFlags.CodeTypeMask
                    };
                    list2.Add(method2);
                    parameters = new ParameterList(1);
                    for (int j = 0; j < num; j++)
                    {
                        Parameter parameter2 = list4[j];
                        if (((parameter2 != null) && (parameter2.Type != null)) && (parameter2.Type is Reference))
                        {
                            parameters.Add((Parameter) parameter2.Clone());
                        }
                    }
                    parameters.Add(new Parameter(null, ParameterFlags.None, StandardIds.result, SystemTypes.IASyncResult, null, null));
                    Method method3 = new Method(this, null, StandardIds.EndInvoke, parameters, this.ReturnType, null) {
                        Flags = MethodFlags.NewSlot | MethodFlags.HideBySig | MethodFlags.Virtual | MethodFlags.Public,
                        CallingConvention = CallingConventionFlags.HasThis,
                        ImplFlags = MethodImplFlags.CodeTypeMask
                    };
                    list2.Add(method3);
                }
                if (!this.IsGeneric)
                {
                    TypeNodeList templateParameters = this.TemplateParameters;
                    int num4 = 0;
                    int num5 = (templateParameters == null) ? 0 : templateParameters.Count;
                    while (num4 < num5)
                    {
                        TypeNode node = templateParameters[num4];
                        if (node != null)
                        {
                            list2.Add(node);
                        }
                        num4++;
                    }
                }
            }
        }

        public virtual ParameterList Parameters
        {
            get
            {
                ParameterList parameters = this.parameters;
                if (parameters == null)
                {
                    if (this.Members != null)
                    {
                    }
                    lock (this)
                    {
                        if (this.parameters != null)
                        {
                            return this.parameters;
                        }
                        MemberList membersNamed = this.GetMembersNamed(StandardIds.Invoke);
                        int num = 0;
                        int count = membersNamed.Count;
                        while (num < count)
                        {
                            Method method = membersNamed[num] as Method;
                            if (method != null)
                            {
                                this.parameters = parameters = method.Parameters;
                                this.returnType = method.ReturnType;
                                return parameters;
                            }
                            num++;
                        }
                    }
                }
                return parameters;
            }
            set
            {
                this.parameters = value;
            }
        }

        public virtual TypeNode ReturnType
        {
            get
            {
                TypeNode returnType = this.returnType;
                if (returnType != null)
                {
                    return returnType;
                }
                if (this.Parameters != null)
                {
                }
                return this.returnType;
            }
            set
            {
                this.returnType = value;
            }
        }
    }
}

