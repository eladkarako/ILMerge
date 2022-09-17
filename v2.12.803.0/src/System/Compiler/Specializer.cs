namespace System.Compiler
{
    using System;

    internal class Specializer : StandardVisitor
    {
        public TypeNodeList args;
        public Method CurrentMethod;
        public TypeNode CurrentType;
        public readonly Block DummyBody;
        public TypeNodeList pars;
        public Module TargetModule;

        public Specializer(Visitor callingVisitor) : base(callingVisitor)
        {
            this.DummyBody = new Block();
        }

        public Specializer(Module targetModule, TypeNodeList pars, TypeNodeList args)
        {
            this.DummyBody = new Block();
            this.pars = pars;
            this.args = args;
            this.TargetModule = targetModule;
        }

        private TypeNode ConvertToClassParameter(TypeNode baseType, TypeNode typeParameter)
        {
            ClassParameter parameter;
            if (typeParameter is MethodTypeParameter)
            {
                parameter = new MethodClassParameter();
            }
            else if (typeParameter is TypeParameter)
            {
                parameter = new ClassParameter {
                    DeclaringType = typeParameter.DeclaringType
                };
            }
            else
            {
                return typeParameter;
            }
            parameter.SourceContext = typeParameter.SourceContext;
            parameter.TypeParameterFlags = ((ITypeParameter) typeParameter).TypeParameterFlags;
            parameter.ParameterListIndex = ((ITypeParameter) typeParameter).ParameterListIndex;
            parameter.Name = typeParameter.Name;
            parameter.Namespace = StandardIds.ClassParameter;
            parameter.BaseClass = (baseType is Class) ? ((Class) baseType) : CoreSystemTypes.Object;
            parameter.DeclaringMember = ((ITypeParameter) typeParameter).DeclaringMember;
            parameter.DeclaringModule = typeParameter.DeclaringModule;
            parameter.Flags = typeParameter.Flags & ~TypeFlags.ClassSemanticsMask;
            InterfaceList interfaces = typeParameter.Interfaces;
            int num = 1;
            int num2 = (interfaces == null) ? 0 : interfaces.Count;
            while (num < num2)
            {
                interfaces.Add(this.VisitInterfaceReference(interfaces[num]));
                num++;
            }
            return parameter;
        }

        public static Member GetCorrespondingMember(Member member, TypeNode specializedType)
        {
            if (member.DeclaringType != null)
            {
                MemberList members = member.DeclaringType.Members;
                MemberList list2 = specializedType.Members;
                if ((members == null) || (list2 == null))
                {
                    return null;
                }
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = (list2 == null) ? 0 : list2.Count;
                while (num3 < num4)
                {
                    Member member2 = members[num3 - num];
                    Member member3 = list2[num3 - num2];
                    if (member2 == member)
                    {
                        return member3;
                    }
                    num3++;
                }
            }
            return null;
        }

        private void ProvideNestedTypes(TypeNode typeNode, object handle)
        {
            SpecializerHandle handle2 = (SpecializerHandle) handle;
            if (handle2.NestedTypeProvider != null)
            {
                TypeNode currentType = this.CurrentType;
                this.CurrentType = typeNode;
                handle2.NestedTypeProvider(typeNode, handle2.Handle);
                TypeNodeList nestedTypes = typeNode.nestedTypes;
                int num = 0;
                int num2 = (nestedTypes == null) ? 0 : nestedTypes.Count;
                while (num < num2)
                {
                    TypeNode node2 = nestedTypes[num];
                    if (node2 != null)
                    {
                        this.VisitTypeNode(node2);
                    }
                    num++;
                }
                this.CurrentType = currentType;
            }
        }

        private void ProvideTypeAttributes(TypeNode typeNode, object handle)
        {
            SpecializerHandle handle2 = (SpecializerHandle) handle;
            TypeNode currentType = this.CurrentType;
            this.CurrentType = typeNode;
            if (handle2.TypeAttributeProvider != null)
            {
                handle2.TypeAttributeProvider(typeNode, handle2.Handle);
            }
            typeNode.Attributes = this.VisitAttributeList(typeNode.Attributes);
            typeNode.SecurityAttributes = this.VisitSecurityAttributeList(typeNode.SecurityAttributes);
            this.CurrentType = currentType;
        }

        private void ProvideTypeMembers(TypeNode typeNode, object handle)
        {
            SpecializerHandle handle2 = (SpecializerHandle) handle;
            TypeNode currentType = this.CurrentType;
            this.CurrentType = typeNode;
            handle2.TypeMemberProvider(typeNode, handle2.Handle);
            typeNode.Members = this.VisitMemberList(typeNode.Members);
            DelegateNode node2 = typeNode as DelegateNode;
            if ((node2 != null) && node2.IsNormalized)
            {
                node2.Parameters = this.VisitParameterList(node2.Parameters);
                node2.ReturnType = this.VisitTypeReference(node2.ReturnType);
            }
            this.CurrentType = currentType;
        }

        private void ProvideTypeSignature(TypeNode typeNode, object handle)
        {
            SpecializerHandle handle2 = (SpecializerHandle) handle;
            TypeNode currentType = this.CurrentType;
            this.CurrentType = typeNode;
            if (handle2.TypeSignatureProvider != null)
            {
                handle2.TypeSignatureProvider(typeNode, handle2.Handle);
            }
            Class class2 = typeNode as Class;
            if (class2 != null)
            {
                class2.BaseClass = (Class) this.VisitTypeReference(class2.BaseClass);
            }
            typeNode.Interfaces = this.VisitInterfaceReferenceList(typeNode.Interfaces);
            this.CurrentType = currentType;
        }

        public override void TransferStateTo(Visitor targetVisitor)
        {
            base.TransferStateTo(targetVisitor);
            Specializer specializer = targetVisitor as Specializer;
            if (specializer != null)
            {
                specializer.args = this.args;
                specializer.pars = this.pars;
                specializer.CurrentMethod = this.CurrentMethod;
                specializer.CurrentType = this.CurrentType;
            }
        }

        public virtual object VisitContractPart(Method method, object part)
        {
            if (method != null)
            {
                this.CurrentMethod = method;
                this.CurrentType = method.DeclaringType;
                EnsuresList ensures = part as EnsuresList;
                if (ensures != null)
                {
                    return this.VisitEnsuresList(ensures);
                }
                RequiresList requires = part as RequiresList;
                if (requires != null)
                {
                    return this.VisitRequiresList(requires);
                }
                Block block = part as Block;
                if (block != null)
                {
                    return this.VisitBlock(block);
                }
            }
            return part;
        }

        public override DelegateNode VisitDelegateNode(DelegateNode delegateNode) => 
            (this.VisitTypeNode(delegateNode) as DelegateNode);

        public override Interface VisitInterfaceReference(Interface Interface) => 
            (this.VisitTypeReference(Interface) as Interface);

        public virtual Member VisitMemberReference(Member member)
        {
            if (member == null)
            {
                return null;
            }
            TypeNode type = member as TypeNode;
            if (type != null)
            {
                return this.VisitTypeReference(type);
            }
            Method method = member as Method;
            if (((method != null) && (method.Template != null)) && ((method.TemplateArguments != null) && (method.TemplateArguments.Count > 0)))
            {
                Method method2 = this.VisitMemberReference(method.Template) as Method;
                bool flag = (method2 != null) && (method2 != method.Template);
                TypeNodeList typeArguments = method.TemplateArguments.Clone();
                int num = 0;
                int count = typeArguments.Count;
                while (num < count)
                {
                    TypeNode node2 = this.VisitTypeReference(typeArguments[num]);
                    if ((node2 != null) && (node2 != typeArguments[num]))
                    {
                        typeArguments[num] = node2;
                        flag = true;
                    }
                    num++;
                }
                if (flag)
                {
                    return method2.GetTemplateInstance(this.CurrentType, typeArguments);
                }
                return method;
            }
            TypeNode specializedType = this.VisitTypeReference(member.DeclaringType);
            if (!(specializedType == member.DeclaringType) && (specializedType != null))
            {
                return GetCorrespondingMember(member, specializedType);
            }
            return member;
        }

        public override Method VisitMethod(Method method)
        {
            if (method == null)
            {
                return null;
            }
            Method method2 = this.CurrentMethod;
            TypeNode node = this.CurrentType;
            this.CurrentMethod = method;
            this.CurrentType = method.DeclaringType;
            method.ThisParameter = (This) this.VisitThis(method.ThisParameter);
            method.Attributes = this.VisitAttributeList(method.Attributes);
            method.ReturnAttributes = this.VisitAttributeList(method.ReturnAttributes);
            method.SecurityAttributes = this.VisitSecurityAttributeList(method.SecurityAttributes);
            method.ReturnType = this.VisitTypeReference(method.ReturnType);
            method.Parameters = this.VisitParameterList(method.Parameters);
            if (TargetPlatform.UseGenerics && (this.args != method.TemplateArguments))
            {
                method.TemplateArguments = this.VisitTypeReferenceList(method.TemplateArguments);
                method.TemplateParameters = this.VisitTypeParameterList(method.TemplateParameters);
            }
            if (method.contract != null)
            {
                method.contract = this.VisitMethodContract(method.contract);
            }
            else if ((method.ProvideContract != null) && (method.ProviderHandle != null))
            {
                Method.MethodContractProvider origContractProvider = method.ProvideContract;
                method.ProvideContract = delegate (Method mHandle, object oHandle) {
                    origContractProvider(mHandle, oHandle);
                    Method currentMethod = this.CurrentMethod;
                    TypeNode currentType = this.CurrentType;
                    this.CurrentMethod = mHandle;
                    this.CurrentType = mHandle.DeclaringType;
                    this.VisitMethodContract(mHandle.contract);
                    this.CurrentType = currentType;
                    this.CurrentMethod = currentMethod;
                };
            }
            method.ImplementedInterfaceMethods = this.VisitMethodList(method.ImplementedInterfaceMethods);
            this.CurrentMethod = method2;
            this.CurrentType = node;
            return method;
        }

        public override MethodContract VisitMethodContract(MethodContract contract)
        {
            if (contract == null)
            {
                return null;
            }
            MethodBodySpecializer methodBodySpecializer = this as MethodBodySpecializer;
            if (methodBodySpecializer == null)
            {
                methodBodySpecializer = contract.DeclaringMethod.DeclaringType.DeclaringModule.GetMethodBodySpecializer(this.pars, this.args);
                methodBodySpecializer.CurrentMethod = this.CurrentMethod;
                methodBodySpecializer.CurrentType = this.CurrentType;
            }
            contract.contractInitializer = methodBodySpecializer.VisitBlock(contract.contractInitializer);
            contract.postPreamble = methodBodySpecializer.VisitBlock(contract.postPreamble);
            contract.ensures = methodBodySpecializer.VisitEnsuresList(contract.ensures);
            contract.modelEnsures = methodBodySpecializer.VisitEnsuresList(contract.modelEnsures);
            contract.modifies = methodBodySpecializer.VisitExpressionList(contract.modifies);
            contract.requires = methodBodySpecializer.VisitRequiresList(contract.requires);
            return contract;
        }

        public virtual MethodList VisitMethodList(MethodList methods)
        {
            if (methods == null)
            {
                return null;
            }
            int count = methods.Count;
            for (int i = 0; i < count; i++)
            {
                methods[i] = (Method) this.VisitMemberReference(methods[i]);
            }
            return methods;
        }

        public virtual Expression VisitTypeExpression(Expression expr)
        {
            TypeNodeList pars = this.pars;
            TypeNodeList args = this.args;
            Identifier identifier = expr as Identifier;
            if (identifier == null)
            {
                return expr;
            }
            int uniqueIdKey = identifier.UniqueIdKey;
            int num2 = 0;
            int num3 = (pars == null) ? 0 : pars.Count;
            int num4 = (args == null) ? 0 : args.Count;
            while ((num2 < num3) && (num2 < num4))
            {
                TypeNode node = pars[num2];
                if (((node != null) && (node.Name != null)) && (node.Name.UniqueIdKey == uniqueIdKey))
                {
                    return new Literal(args[num2], CoreSystemTypes.Type);
                }
                num2++;
            }
            return identifier;
        }

        public override TypeNode VisitTypeNode(TypeNode typeNode)
        {
            if (typeNode == null)
            {
                return null;
            }
            TypeNode currentType = this.CurrentType;
            if ((((currentType != null) && (currentType.TemplateArguments != null)) && ((currentType.TemplateArguments.Count > 0) && (typeNode.Template != null))) && ((typeNode.Template.TemplateParameters == null) || (typeNode.Template.TemplateParameters.Count == 0)))
            {
                typeNode.TemplateArguments = new TypeNodeList(0);
            }
            this.CurrentType = typeNode;
            if ((typeNode.ProvideTypeMembers != null) && (typeNode.ProviderHandle != null))
            {
                typeNode.members = null;
                typeNode.ProviderHandle = new SpecializerHandle(typeNode.ProvideNestedTypes, typeNode.ProvideTypeMembers, typeNode.ProvideTypeSignature, typeNode.ProvideTypeAttributes, typeNode.ProviderHandle);
                typeNode.ProvideNestedTypes = new TypeNode.NestedTypeProvider(this.ProvideNestedTypes);
                typeNode.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);
                typeNode.ProvideTypeAttributes = new TypeNode.TypeAttributeProvider(this.ProvideTypeAttributes);
                typeNode.ProvideTypeSignature = new TypeNode.TypeSignatureProvider(this.ProvideTypeSignature);
                DelegateNode node2 = typeNode as DelegateNode;
                if ((node2 != null) && !node2.IsNormalized)
                {
                    node2.Parameters = this.VisitParameterList(node2.Parameters);
                    node2.ReturnType = this.VisitTypeReference(node2.ReturnType);
                }
            }
            else
            {
                typeNode.Attributes = this.VisitAttributeList(typeNode.Attributes);
                typeNode.SecurityAttributes = this.VisitSecurityAttributeList(typeNode.SecurityAttributes);
                Class class2 = typeNode as Class;
                if (class2 != null)
                {
                    class2.BaseClass = (Class) this.VisitTypeReference(class2.BaseClass);
                }
                typeNode.Interfaces = this.VisitInterfaceReferenceList(typeNode.Interfaces);
                typeNode.Members = this.VisitMemberList(typeNode.Members);
                DelegateNode node3 = typeNode as DelegateNode;
                if (node3 != null)
                {
                    node3.Parameters = this.VisitParameterList(node3.Parameters);
                    node3.ReturnType = this.VisitTypeReference(node3.ReturnType);
                }
            }
            this.CurrentType = currentType;
            return typeNode;
        }

        public override TypeNode VisitTypeParameter(TypeNode typeParameter)
        {
            if (typeParameter != null)
            {
                if (TargetPlatform.UseGenerics)
                {
                    InterfaceList interfaces = typeParameter.Interfaces;
                    if ((interfaces != null) && (interfaces.Count != 0))
                    {
                        TypeNode baseType = this.VisitTypeReference(interfaces[0]);
                        if (baseType is Interface)
                        {
                            typeParameter.Interfaces = this.VisitInterfaceReferenceList(typeParameter.Interfaces);
                            return typeParameter;
                        }
                        typeParameter = this.ConvertToClassParameter(baseType, typeParameter);
                    }
                    return typeParameter;
                }
                typeParameter.Interfaces = this.VisitInterfaceReferenceList(typeParameter.Interfaces);
            }
            return null;
        }

        public override TypeNode VisitTypeReference(TypeNode type)
        {
            TypeNode node;
            if (type == null)
            {
                return null;
            }
            TypeNodeList pars = this.pars;
            TypeNodeList args = this.args;
            switch (type.NodeType)
            {
                case NodeType.InterfaceExpression:
                {
                    InterfaceExpression expression7 = (InterfaceExpression) type;
                    if (expression7.Expression != null)
                    {
                        expression7.Expression = this.VisitTypeExpression(expression7.Expression);
                        expression7.TemplateArguments = this.VisitTypeReferenceList(expression7.TemplateArguments);
                        return expression7;
                    }
                    break;
                }
                case NodeType.ArrayType:
                {
                    ArrayType type3 = (ArrayType) type;
                    node = this.VisitTypeReference(type3.ElementType);
                    if ((node == type3.ElementType) || (node == null))
                    {
                        return type3;
                    }
                    if (type3.IsSzArray())
                    {
                        return node.GetArrayType(1);
                    }
                    return node.GetArrayType(type3.Rank, type3.Sizes, type3.LowerBounds);
                }
                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                {
                    int uniqueKey = type.UniqueKey;
                    int num2 = 0;
                    int num3 = (pars == null) ? 0 : pars.Count;
                    int num4 = (args == null) ? 0 : args.Count;
                    while ((num2 < num3) && (num2 < num4))
                    {
                        TypeNode node3 = pars[num2];
                        if ((node3 != null) && (node3.UniqueKey == uniqueKey))
                        {
                            return args[num2];
                        }
                        num2++;
                    }
                    return type;
                }
                case NodeType.DelegateNode:
                {
                    FunctionType type4 = type as FunctionType;
                    if (type4 == null)
                    {
                        break;
                    }
                    TypeNode referringType = (type4.DeclaringType == null) ? this.CurrentType : this.VisitTypeReference(type4.DeclaringType);
                    return FunctionType.For(this.VisitTypeReference(type4.ReturnType), this.VisitParameterList(type4.Parameters), referringType);
                }
                case NodeType.OptionalModifier:
                {
                    TypeModifier modifier = (TypeModifier) type;
                    TypeNode modified = this.VisitTypeReference(modifier.ModifiedType);
                    TypeNode node5 = this.VisitTypeReference(modifier.Modifier);
                    if ((modified != null) && (node5 != null))
                    {
                        return OptionalModifier.For(node5, modified);
                    }
                    return type;
                }
                case NodeType.Pointer:
                {
                    Pointer pointer = (Pointer) type;
                    node = this.VisitTypeReference(pointer.ElementType);
                    if (!(node == pointer.ElementType) && (node != null))
                    {
                        return node.GetPointerType();
                    }
                    return pointer;
                }
                case NodeType.Reference:
                {
                    Reference reference = (Reference) type;
                    node = this.VisitTypeReference(reference.ElementType);
                    if (!(node == reference.ElementType) && (node != null))
                    {
                        return node.GetReferenceType();
                    }
                    return reference;
                }
                case NodeType.RequiredModifier:
                {
                    TypeModifier modifier2 = (TypeModifier) type;
                    TypeNode node6 = this.VisitTypeReference(modifier2.ModifiedType);
                    TypeNode node7 = this.VisitTypeReference(modifier2.Modifier);
                    if ((node6 != null) && (node7 != null))
                    {
                        return RequiredModifier.For(node7, node6);
                    }
                    return type;
                }
                case NodeType.BoxedTypeExpression:
                {
                    BoxedTypeExpression expression2 = (BoxedTypeExpression) type;
                    expression2.ElementType = this.VisitTypeReference(expression2.ElementType);
                    return expression2;
                }
                case NodeType.ClassExpression:
                {
                    ClassExpression expression3 = (ClassExpression) type;
                    expression3.Expression = this.VisitTypeExpression(expression3.Expression);
                    Literal literal = expression3.Expression as Literal;
                    if (literal == null)
                    {
                        expression3.TemplateArguments = this.VisitTypeReferenceList(expression3.TemplateArguments);
                        return expression3;
                    }
                    return (literal.Value as TypeNode);
                }
                case NodeType.FlexArrayTypeExpression:
                {
                    FlexArrayTypeExpression expression4 = (FlexArrayTypeExpression) type;
                    expression4.ElementType = this.VisitTypeReference(expression4.ElementType);
                    return expression4;
                }
                case NodeType.ArrayTypeExpression:
                {
                    ArrayTypeExpression expression = (ArrayTypeExpression) type;
                    expression.ElementType = this.VisitTypeReference(expression.ElementType);
                    return expression;
                }
                case NodeType.NonEmptyStreamTypeExpression:
                {
                    NonEmptyStreamTypeExpression expression8 = (NonEmptyStreamTypeExpression) type;
                    expression8.ElementType = this.VisitTypeReference(expression8.ElementType);
                    return expression8;
                }
                case NodeType.NonNullableTypeExpression:
                {
                    NonNullableTypeExpression expression10 = (NonNullableTypeExpression) type;
                    expression10.ElementType = this.VisitTypeReference(expression10.ElementType);
                    return expression10;
                }
                case NodeType.NonNullTypeExpression:
                {
                    NonNullTypeExpression expression9 = (NonNullTypeExpression) type;
                    expression9.ElementType = this.VisitTypeReference(expression9.ElementType);
                    return expression9;
                }
                case NodeType.NullableTypeExpression:
                {
                    NullableTypeExpression expression11 = (NullableTypeExpression) type;
                    expression11.ElementType = this.VisitTypeReference(expression11.ElementType);
                    return expression11;
                }
                case NodeType.InvariantTypeExpression:
                {
                    InvariantTypeExpression expression6 = (InvariantTypeExpression) type;
                    expression6.ElementType = this.VisitTypeReference(expression6.ElementType);
                    return expression6;
                }
                case NodeType.FunctionTypeExpression:
                {
                    FunctionTypeExpression expression5 = (FunctionTypeExpression) type;
                    expression5.Parameters = this.VisitParameterList(expression5.Parameters);
                    expression5.ReturnType = this.VisitTypeReference(expression5.ReturnType);
                    return expression5;
                }
            }
            if (type.Template != null)
            {
                bool flag = false;
                TypeNodeList consolidatedTemplateArguments = type.ConsolidatedTemplateArguments;
                int num5 = (consolidatedTemplateArguments == null) ? 0 : consolidatedTemplateArguments.Count;
                if (consolidatedTemplateArguments != null)
                {
                    consolidatedTemplateArguments = consolidatedTemplateArguments.Clone();
                    for (int i = 0; i < num5; i++)
                    {
                        TypeNode node8 = consolidatedTemplateArguments[i];
                        consolidatedTemplateArguments[i] = this.VisitTypeReference(node8);
                        if (node8 != consolidatedTemplateArguments[i])
                        {
                            flag = true;
                        }
                    }
                }
                if ((consolidatedTemplateArguments != null) && flag)
                {
                    return type.Template.GetGenericTemplateInstance(this.TargetModule, consolidatedTemplateArguments);
                }
            }
            return type;
        }

        internal class SpecializerHandle
        {
            internal object Handle;
            internal System.Compiler.TypeNode.NestedTypeProvider NestedTypeProvider;
            internal System.Compiler.TypeNode.TypeAttributeProvider TypeAttributeProvider;
            internal System.Compiler.TypeNode.TypeMemberProvider TypeMemberProvider;
            internal System.Compiler.TypeNode.TypeSignatureProvider TypeSignatureProvider;

            internal SpecializerHandle(System.Compiler.TypeNode.NestedTypeProvider nestedTypeProvider, System.Compiler.TypeNode.TypeMemberProvider typeMemberProvider, System.Compiler.TypeNode.TypeSignatureProvider typeSignatureProvider, System.Compiler.TypeNode.TypeAttributeProvider typeAttributeProvider, object handle)
            {
                this.NestedTypeProvider = nestedTypeProvider;
                this.TypeMemberProvider = typeMemberProvider;
                this.TypeSignatureProvider = typeSignatureProvider;
                this.TypeAttributeProvider = typeAttributeProvider;
                this.Handle = handle;
            }
        }
    }
}

