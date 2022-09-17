namespace System.Compiler
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class Duplicator : StandardVisitor
    {
        public bool CopyDocumentation;
        public readonly Block DummyBody;
        public TypeNode OriginalTargetType;
        public bool RecordOriginalAsTemplate;
        public bool SkipBodies;
        public Method TargetMethod;
        public Module TargetModule;
        public TypeNode TargetType;

        public Duplicator(Module module, TypeNode type) : this(module, type, 4)
        {
        }

        public Duplicator(Module module, TypeNode type, int initialCapacity)
        {
            this.DummyBody = new Block();
            this.TargetModule = module;
            this.TargetType = this.OriginalTargetType = type;
            this.DuplicateFor = new TrivialHashtable(initialCapacity);
            this.TypesToBeDuplicated = new TrivialHashtable();
        }

        public virtual void FindMembersToBeDuplicated(MemberList members)
        {
            if (members != null)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    this.RegisterMemberToBeDuplicated(members[i]);
                }
            }
        }

        public virtual void FindTypesToBeDuplicated(NamespaceList namespaces)
        {
            if (namespaces != null)
            {
                int num = 0;
                int count = namespaces.Count;
                while (num < count)
                {
                    Namespace namespace2 = namespaces[num];
                    if (namespace2 != null)
                    {
                        this.FindTypesToBeDuplicated(namespace2.Types);
                    }
                    num++;
                }
            }
        }

        public virtual void FindTypesToBeDuplicated(TypeNodeList types)
        {
            if (types != null)
            {
                int num = 0;
                int count = types.Count;
                while (num < count)
                {
                    TypeNode t = types[num];
                    this.RegisterTypeToBeDuplicated(t);
                    num++;
                }
            }
        }

        private void ObjectInvariant()
        {
        }

        protected virtual void ProvideMethodAttributes(Method dup, object handle)
        {
            Method method = (Method) handle;
            AttributeList attributes = method.Attributes;
            SecurityAttributeList securityAttributes = method.SecurityAttributes;
            if ((attributes == null) && (securityAttributes == null))
            {
                dup.ProvideMethodAttributes = null;
            }
            else
            {
                TypeNode targetType = this.TargetType;
                this.TargetType = dup.DeclaringType;
                dup.Attributes = this.VisitAttributeList(attributes);
                dup.SecurityAttributes = this.VisitSecurityAttributeList(securityAttributes);
                this.TargetType = targetType;
            }
        }

        protected virtual void ProvideMethodBody(Method dup, object handle, bool asInstructionList)
        {
            if (asInstructionList)
            {
                dup.Instructions = new InstructionList(0);
            }
            else
            {
                Method method = (Method) handle;
                Block body = method.Body;
                if (body == null)
                {
                    dup.ProvideBody = null;
                }
                else
                {
                    TypeNode targetType = this.TargetType;
                    this.TargetType = dup.DeclaringType;
                    dup.Body = this.VisitBlock(body);
                    dup.ExceptionHandlers = this.VisitExceptionHandlerList(method.ExceptionHandlers);
                    this.TargetType = targetType;
                }
            }
        }

        protected virtual void ProvideMethodContract(Method dup, object handle)
        {
            dup.ProvideContract = null;
            Method method = (Method) handle;
            MethodContract contract = method.Contract;
            if (contract != null)
            {
                TypeNode targetType = this.TargetType;
                Method targetMethod = this.TargetMethod;
                this.TargetType = dup.DeclaringType;
                this.TargetMethod = dup;
                dup.contract = this.VisitMethodContract(contract);
                this.TargetType = targetType;
                this.TargetMethod = targetMethod;
            }
        }

        private void ProvideNestedTypes(TypeNode dup, object handle)
        {
            TypeNode node = (TypeNode) handle;
            TypeNode targetType = this.TargetType;
            Module targetModule = this.TargetModule;
            this.TargetType = dup;
            this.TargetModule = dup.DeclaringModule;
            this.FindTypesToBeDuplicated(node.NestedTypes);
            dup.NestedTypes = this.VisitNestedTypes(dup, node.NestedTypes);
            this.TargetModule = targetModule;
            this.TargetType = targetType;
        }

        private void ProvideTypeAttributes(TypeNode dup, object handle)
        {
            TypeNode node = (TypeNode) handle;
            AttributeList attributes = node.Attributes;
            SecurityAttributeList securityAttributes = node.SecurityAttributes;
            if ((attributes != null) || (securityAttributes != null))
            {
                TypeNode targetType = this.TargetType;
                this.TargetType = dup;
                dup.Attributes = this.VisitAttributeList(attributes);
                dup.SecurityAttributes = this.VisitSecurityAttributeList(securityAttributes);
                this.TargetType = targetType;
            }
        }

        private void ProvideTypeMembers(TypeNode dup, object handle)
        {
            TypeNode node = (TypeNode) handle;
            TypeNode targetType = this.TargetType;
            Module targetModule = this.TargetModule;
            this.TargetType = dup;
            this.TargetModule = dup.DeclaringModule;
            dup.Members = this.VisitMemberList(node.Members);
            DelegateNode node3 = dup as DelegateNode;
            if ((node3 != null) && node3.IsNormalized)
            {
                DelegateNode node4 = node as DelegateNode;
                node3.Parameters = this.VisitParameterList(node4.Parameters);
                node3.ReturnType = this.VisitTypeReference(node4.ReturnType);
            }
            dup.Contract = this.VisitTypeContract(node.Contract);
            this.TargetModule = targetModule;
            this.TargetType = targetType;
        }

        private void ProvideTypeSignature(TypeNode dup, object handle)
        {
            TypeNode node = (TypeNode) handle;
            TypeNode targetType = this.TargetType;
            this.TargetType = dup;
            this.TargetModule = dup.DeclaringModule;
            if (!this.RecordOriginalAsTemplate && (dup.templateParameters == null))
            {
                dup.TemplateParameters = this.VisitTypeReferenceList(node.TemplateParameters);
            }
            if ((dup.DeclaringType == null) && !(dup is ITypeParameter))
            {
                TypeNode declaringType = node.DeclaringType;
                TypeNode node4 = this.VisitTypeReference(declaringType);
                if ((declaringType != null) && ((node4 == null) || (node4 == declaringType)))
                {
                    dup.DeclaringType = this.OriginalTargetType;
                }
                else
                {
                    dup.DeclaringType = node4;
                }
            }
            Class class2 = dup as Class;
            if ((class2 != null) && (class2.baseClass == null))
            {
                Class class3 = (Class) node;
                class2.BaseClass = (Class) this.VisitTypeReference(class3.BaseClass);
            }
            dup.Interfaces = this.VisitInterfaceReferenceList(node.Interfaces);
            this.TargetType = targetType;
        }

        private bool RegisterMemberToBeDuplicated(Member m)
        {
            if (m == null)
            {
                return false;
            }
            TypeNode t = m as TypeNode;
            return ((t != null) && this.RegisterTypeToBeDuplicated(t));
        }

        private bool RegisterTypeToBeDuplicated(TypeNode t)
        {
            if (t == null)
            {
                return false;
            }
            if (this.DuplicateFor[t.UniqueKey] != null)
            {
                return false;
            }
            if (this.TypesToBeDuplicated[t.UniqueKey] != null)
            {
                return false;
            }
            this.TypesToBeDuplicated[t.UniqueKey] = t;
            TypeNode node = this.VisitTypeNode(t, null, null, null, true);
            TypeNode targetType = this.TargetType;
            this.TargetType = node;
            this.FindTypesToBeDuplicated(t.TemplateParameters);
            this.FindMembersToBeDuplicated(t.Members);
            this.TargetType = targetType;
            return true;
        }

        public override Node Visit(Node node)
        {
            node = base.Visit(node);
            Expression expression = node as Expression;
            if (expression != null)
            {
                expression.Type = this.VisitTypeReference(expression.Type);
            }
            return node;
        }

        public override Expression VisitAddressDereference(AddressDereference addr)
        {
            if (addr == null)
            {
                return null;
            }
            return base.VisitAddressDereference((AddressDereference) addr.Clone());
        }

        public override AssemblyNode VisitAssembly(AssemblyNode assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            this.FindTypesToBeDuplicated(assembly.Types);
            return base.VisitAssembly((AssemblyNode) assembly.Clone());
        }

        public override AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference)
        {
            if (assemblyReference == null)
            {
                return null;
            }
            return base.VisitAssemblyReference((AssemblyReference) assemblyReference.Clone());
        }

        public override Statement VisitAssertion(Assertion assertion)
        {
            if (assertion == null)
            {
                return null;
            }
            return base.VisitAssertion((Assertion) assertion.Clone());
        }

        public override Expression VisitAssignmentExpression(AssignmentExpression assignment)
        {
            if (assignment == null)
            {
                return null;
            }
            return base.VisitAssignmentExpression((AssignmentExpression) assignment.Clone());
        }

        public override Statement VisitAssignmentStatement(AssignmentStatement assignment)
        {
            if (assignment == null)
            {
                return null;
            }
            return base.VisitAssignmentStatement((AssignmentStatement) assignment.Clone());
        }

        public override Statement VisitAssumption(Assumption Assumption)
        {
            if (Assumption == null)
            {
                return null;
            }
            return base.VisitAssumption((Assumption) Assumption.Clone());
        }

        public override Expression VisitAttributeConstructor(AttributeNode attribute)
        {
            if ((attribute != null) && (attribute.Constructor != null))
            {
                return this.VisitExpression((Expression) attribute.Constructor.Clone());
            }
            return null;
        }

        public override AttributeList VisitAttributeList(AttributeList attributes)
        {
            if (attributes == null)
            {
                return null;
            }
            return base.VisitAttributeList(attributes.Clone());
        }

        public override AttributeNode VisitAttributeNode(AttributeNode attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return base.VisitAttributeNode((AttributeNode) attribute.Clone());
        }

        public override Expression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression == null)
            {
                return null;
            }
            binaryExpression = (BinaryExpression) base.VisitBinaryExpression((BinaryExpression) binaryExpression.Clone());
            return binaryExpression;
        }

        public override Block VisitBlock(Block block)
        {
            if (block == null)
            {
                return null;
            }
            Block block2 = (Block) this.DuplicateFor[block.UniqueKey];
            if (block2 != null)
            {
                return block2;
            }
            this.DuplicateFor[block.UniqueKey] = block2 = (Block) block.Clone();
            return base.VisitBlock(block2);
        }

        public override Expression VisitBlockExpression(BlockExpression blockExpression)
        {
            if (blockExpression == null)
            {
                return null;
            }
            return base.VisitBlockExpression((BlockExpression) blockExpression.Clone());
        }

        public override BlockList VisitBlockList(BlockList blockList)
        {
            if (blockList == null)
            {
                return null;
            }
            return base.VisitBlockList(blockList.Clone());
        }

        public override Statement VisitBranch(Branch branch)
        {
            if (branch == null)
            {
                return null;
            }
            branch = (Branch) base.VisitBranch((Branch) branch.Clone());
            if (branch == null)
            {
                return null;
            }
            branch.Target = this.VisitBlock(branch.Target);
            return branch;
        }

        public override Expression VisitConstruct(Construct cons)
        {
            if (cons == null)
            {
                return null;
            }
            return base.VisitConstruct((Construct) cons.Clone());
        }

        public override Expression VisitConstructArray(ConstructArray consArr)
        {
            if (consArr == null)
            {
                return null;
            }
            return base.VisitConstructArray((ConstructArray) consArr.Clone());
        }

        public override DelegateNode VisitDelegateNode(DelegateNode delegateNode) => 
            (this.VisitTypeNode(delegateNode) as DelegateNode);

        public override Statement VisitEndFilter(EndFilter endFilter)
        {
            if (endFilter == null)
            {
                return null;
            }
            return base.VisitEndFilter((EndFilter) endFilter.Clone());
        }

        public override Statement VisitEndFinally(EndFinally endFinally)
        {
            if (endFinally == null)
            {
                return null;
            }
            return base.VisitEndFinally((EndFinally) endFinally.Clone());
        }

        public override EnsuresExceptional VisitEnsuresExceptional(EnsuresExceptional exceptional)
        {
            if (exceptional == null)
            {
                return null;
            }
            return base.VisitEnsuresExceptional((EnsuresExceptional) exceptional.Clone());
        }

        public override EnsuresList VisitEnsuresList(EnsuresList Ensures)
        {
            if (Ensures == null)
            {
                return null;
            }
            return base.VisitEnsuresList(Ensures.Clone());
        }

        public override EnsuresNormal VisitEnsuresNormal(EnsuresNormal normal)
        {
            if (normal == null)
            {
                return null;
            }
            return base.VisitEnsuresNormal((EnsuresNormal) normal.Clone());
        }

        public override Event VisitEvent(Event evnt)
        {
            if (evnt == null)
            {
                return null;
            }
            Event event2 = (Event) this.DuplicateFor[evnt.UniqueKey];
            if (event2 != null)
            {
                return event2;
            }
            this.DuplicateFor[evnt.UniqueKey] = event2 = (Event) evnt.Clone();
            if (this.CopyDocumentation)
            {
                event2.Documentation = evnt.Documentation;
            }
            event2.HandlerAdder = this.VisitMethod(evnt.HandlerAdder);
            event2.HandlerCaller = this.VisitMethod(evnt.HandlerCaller);
            event2.HandlerRemover = this.VisitMethod(evnt.HandlerRemover);
            event2.OtherMethods = this.VisitMethodList(evnt.OtherMethods);
            event2.DeclaringType = this.TargetType;
            return base.VisitEvent(event2);
        }

        public virtual ExceptionHandler VisitExceptionHandler(ExceptionHandler handler)
        {
            if (handler == null)
            {
                return null;
            }
            handler = (ExceptionHandler) handler.Clone();
            handler.BlockAfterHandlerEnd = this.VisitBlock(handler.BlockAfterHandlerEnd);
            handler.BlockAfterTryEnd = this.VisitBlock(handler.BlockAfterTryEnd);
            handler.FilterExpression = this.VisitBlock(handler.FilterExpression);
            handler.FilterType = this.VisitTypeReference(handler.FilterType);
            handler.HandlerStartBlock = this.VisitBlock(handler.HandlerStartBlock);
            handler.TryStartBlock = this.VisitBlock(handler.TryStartBlock);
            return handler;
        }

        public virtual ExceptionHandlerList VisitExceptionHandlerList(ExceptionHandlerList handlers)
        {
            if (handlers == null)
            {
                return null;
            }
            int count = handlers.Count;
            ExceptionHandlerList list = new ExceptionHandlerList(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(this.VisitExceptionHandler(handlers[i]));
            }
            return list;
        }

        public override Expression VisitExpression(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }
            NodeType nodeType = expression.NodeType;
            if ((nodeType == NodeType.Arglist) || (nodeType == NodeType.Dup))
            {
                expression = (Expression) expression.Clone();
            }
            else if (nodeType == NodeType.Pop)
            {
                UnaryExpression expression2 = expression as UnaryExpression;
                if (expression2 != null)
                {
                    expression2 = (UnaryExpression) expression2.Clone();
                    expression2.Operand = this.VisitExpression(expression2.Operand);
                    expression = expression2;
                }
                else
                {
                    expression = (Expression) expression.Clone();
                }
            }
            else
            {
                expression = (Expression) this.Visit(expression);
            }
            if (expression == null)
            {
                return null;
            }
            expression.Type = this.VisitTypeReference(expression.Type);
            return expression;
        }

        public override ExpressionList VisitExpressionList(ExpressionList expressions)
        {
            if (expressions == null)
            {
                return null;
            }
            return base.VisitExpressionList(expressions.Clone());
        }

        public override Statement VisitExpressionStatement(ExpressionStatement statement)
        {
            if (statement == null)
            {
                return null;
            }
            return base.VisitExpressionStatement((ExpressionStatement) statement.Clone());
        }

        public override Statement VisitFaultHandler(FaultHandler faultHandler)
        {
            if (faultHandler == null)
            {
                return null;
            }
            return base.VisitFaultHandler((FaultHandler) faultHandler.Clone());
        }

        public override FaultHandlerList VisitFaultHandlerList(FaultHandlerList faultHandlers)
        {
            if (faultHandlers == null)
            {
                return null;
            }
            return base.VisitFaultHandlerList(faultHandlers.Clone());
        }

        public override Field VisitField(Field field)
        {
            if (field == null)
            {
                return null;
            }
            Field field2 = (Field) this.DuplicateFor[field.UniqueKey];
            if (field2 != null)
            {
                return field2;
            }
            this.DuplicateFor[field.UniqueKey] = field2 = (Field) field.Clone();
            if (field.MarshallingInformation != null)
            {
                field2.MarshallingInformation = field.MarshallingInformation.Clone();
            }
            ParameterField field3 = field2 as ParameterField;
            if (field3 != null)
            {
                field3.Parameter = (Parameter) this.VisitParameter(field3.Parameter);
            }
            field2.DeclaringType = this.TargetType;
            if (this.CopyDocumentation)
            {
                field2.Documentation = field.Documentation;
            }
            return base.VisitField(field2);
        }

        public override Expression VisitIdentifier(Identifier identifier)
        {
            if (identifier == null)
            {
                return null;
            }
            return base.VisitIdentifier((Identifier) identifier.Clone());
        }

        public override Expression VisitIndexer(Indexer indexer)
        {
            if (indexer == null)
            {
                return null;
            }
            indexer = (Indexer) base.VisitIndexer((Indexer) indexer.Clone());
            if (indexer == null)
            {
                return null;
            }
            indexer.ElementType = this.VisitTypeReference(indexer.ElementType);
            return indexer;
        }

        public override InterfaceList VisitInterfaceReferenceList(InterfaceList interfaceReferences)
        {
            if (interfaceReferences == null)
            {
                return null;
            }
            return base.VisitInterfaceReferenceList(interfaceReferences.Clone());
        }

        public override Statement VisitLabeledStatement(LabeledStatement lStatement)
        {
            if (lStatement == null)
            {
                return null;
            }
            return base.VisitLabeledStatement((LabeledStatement) lStatement.Clone());
        }

        public override Expression VisitLiteral(Literal literal)
        {
            if (literal == null)
            {
                return null;
            }
            TypeNode type = this.VisitTypeReference(literal.Type);
            TypeNode node2 = literal.Value as TypeNode;
            if (node2 != null)
            {
                return new Literal(this.VisitTypeReference(node2), type, literal.SourceContext);
            }
            TypeNode[] nodeArray = literal.Value as TypeNode[];
            if (nodeArray != null)
            {
                int num = (nodeArray == null) ? 0 : nodeArray.Length;
                TypeNode[] nodeArray2 = (nodeArray == null) ? null : new TypeNode[num];
                for (int i = 0; i < num; i++)
                {
                    nodeArray2[i] = this.VisitTypeReference(nodeArray[i]);
                }
                return new Literal(nodeArray2, type);
            }
            object[] objArray = literal.Value as object[];
            if (objArray != null)
            {
                int length = objArray.Length;
                object[] objArray2 = new object[length];
                for (int j = 0; j < length; j++)
                {
                    Literal literal2 = objArray[j] as Literal;
                    if (literal2 != null)
                    {
                        objArray2[j] = this.VisitLiteral(literal2);
                    }
                    else
                    {
                        objArray2[j] = objArray[j];
                    }
                }
                return new Literal(objArray2, type);
            }
            Literal literal3 = (Literal) literal.Clone();
            literal3.Type = type;
            return literal3;
        }

        public override Expression VisitLocal(Local local)
        {
            if (local == null)
            {
                return null;
            }
            Local local2 = (Local) this.DuplicateFor[local.UniqueKey];
            if (local2 != null)
            {
                return local2;
            }
            this.DuplicateFor[local.UniqueKey] = local2 = (Local) local.Clone();
            return base.VisitLocal(local2);
        }

        public override Expression VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding == null)
            {
                return null;
            }
            memberBinding = (MemberBinding) memberBinding.Clone();
            memberBinding.TargetObject = this.VisitExpression(memberBinding.TargetObject);
            memberBinding.Type = this.VisitTypeReference(memberBinding.Type);
            memberBinding.BoundMember = this.VisitMemberReference(memberBinding.BoundMember);
            return memberBinding;
        }

        public override MemberList VisitMemberList(MemberList members)
        {
            if (members == null)
            {
                return null;
            }
            MemberList list = members.Clone();
            for (int i = 0; i < list.Count; i++)
            {
                Member node = list[i];
                if (this.RecordOriginalAsTemplate && (node is TypeNode))
                {
                    list[i] = null;
                }
                else
                {
                    list[i] = (Member) this.Visit(node);
                }
            }
            return list;
        }

        public virtual Member VisitMemberReference(Member member)
        {
            if (member == null)
            {
                return null;
            }
            Member correspondingMember = (Member) this.DuplicateFor[member.UniqueKey];
            if (correspondingMember == null)
            {
                TypeNode type = member as TypeNode;
                if (type != null)
                {
                    return this.VisitTypeReference(type);
                }
                if (this.RecordOriginalAsTemplate)
                {
                    return member;
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
                        return method2.GetTemplateInstance(this.TargetType, typeArguments);
                    }
                    return method;
                }
                TypeNode specializedType = this.VisitTypeReference(member.DeclaringType);
                if (specializedType == null)
                {
                    return member;
                }
                if (specializedType == member.DeclaringType)
                {
                    return member;
                }
                correspondingMember = (Member) this.DuplicateFor[member.UniqueKey];
                if (correspondingMember != null)
                {
                    return correspondingMember;
                }
                correspondingMember = Specializer.GetCorrespondingMember(member, specializedType);
                if (correspondingMember != null)
                {
                    return correspondingMember;
                }
                TypeNode targetType = this.TargetType;
                this.TargetType = specializedType;
                correspondingMember = (Member) this.Visit(member);
                this.TargetType = targetType;
            }
            return correspondingMember;
        }

        public virtual MemberList VisitMemberReferenceList(MemberList members)
        {
            if (members == null)
            {
                return null;
            }
            int count = members.Count;
            MemberList list = new MemberList(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(this.VisitMemberReference(members[i]));
            }
            return list;
        }

        public override Method VisitMethod(Method method)
        {
            if (method == null)
            {
                return null;
            }
            Method method2 = (Method) this.DuplicateFor[method.UniqueKey];
            if (method2 != null)
            {
                return method2;
            }
            if (TargetPlatform.UseGenerics)
            {
                this.FindTypesToBeDuplicated(method.TemplateParameters);
            }
            return this.VisitMethodInternal(method);
        }

        public override Expression VisitMethodCall(MethodCall call)
        {
            if (call == null)
            {
                return null;
            }
            return base.VisitMethodCall((MethodCall) call.Clone());
        }

        public override MethodContract VisitMethodContract(MethodContract contract)
        {
            if (contract == null)
            {
                return null;
            }
            MethodContract contract2 = (MethodContract) this.DuplicateFor[contract.UniqueKey];
            if (contract2 == null)
            {
                contract2 = (MethodContract) contract.Clone();
                contract2.contractInitializer = this.VisitBlock(contract.ContractInitializer);
                contract2.postPreamble = this.VisitBlock(contract.PostPreamble);
                contract2.DeclaringMethod = this.TargetMethod;
                contract2.ensures = this.VisitEnsuresList(contract.Ensures);
                contract2.modelEnsures = this.VisitEnsuresList(contract.ModelEnsures);
                contract2.modifies = this.VisitExpressionList(contract.Modifies);
                contract2.requires = this.VisitRequiresList(contract.Requires);
            }
            return contract2;
        }

        public Method VisitMethodInternal(Method method)
        {
            if (method == null)
            {
                return null;
            }
            Method method2 = (Method) this.DuplicateFor[method.UniqueKey];
            if (method2 == null)
            {
                this.DuplicateFor[method.UniqueKey] = method2 = (Method) method.Clone();
                method2.ProviderHandle = null;
                method2.LocalList = null;
                Method targetMethod = this.TargetMethod;
                this.TargetMethod = method2;
                if (TargetPlatform.UseGenerics)
                {
                    method2.TemplateParameters = this.VisitTypeParameterList(method.TemplateParameters);
                }
                if (this.CopyDocumentation)
                {
                    method2.Documentation = method.Documentation;
                }
                method2.OverriddenMember = null;
                method2.ImplementedInterfaceMethods = this.VisitMethodReferenceList(method.ImplementedInterfaceMethods);
                method2.ImplicitlyImplementedInterfaceMethods = null;
                method2.DeclaringType = this.TargetType;
                if (!method.IsAbstract)
                {
                    method2.Body = this.DummyBody;
                }
                if (this.RecordOriginalAsTemplate)
                {
                    if (method.Template != null)
                    {
                        method2.Template = method.Template;
                    }
                    else
                    {
                        method2.Template = method;
                    }
                }
                method2.PInvokeModule = this.VisitModuleReference(method2.PInvokeModule);
                if (method.ReturnTypeMarshallingInformation != null)
                {
                    method2.ReturnTypeMarshallingInformation = method.ReturnTypeMarshallingInformation.Clone();
                }
                method2.ThisParameter = (This) this.VisitParameter(method2.ThisParameter);
                method2.ProvideContract = null;
                method2.contract = null;
                method2 = base.VisitMethod(method2);
                method2.DeclaringMember = (Member) this.Visit(method2.DeclaringMember);
                method2.fullName = null;
                method2.DocumentationId = null;
                method2.ProviderHandle = method;
                method2.Attributes = null;
                method2.ProvideMethodAttributes = new Method.MethodAttributeProvider(this.ProvideMethodAttributes);
                method2.ProvideContract = new Method.MethodContractProvider(this.ProvideMethodContract);
                if (!this.SkipBodies && !method.IsAbstract)
                {
                    method2.Body = null;
                    method2.ProvideBody = new Method.MethodBodyProvider(this.ProvideMethodBody);
                }
                if (this.SkipBodies)
                {
                    method2.Instructions = new InstructionList(0);
                }
                this.TargetMethod = targetMethod;
            }
            return method2;
        }

        public virtual MethodList VisitMethodList(MethodList methods)
        {
            if (methods == null)
            {
                return null;
            }
            int count = methods.Count;
            MethodList list = new MethodList(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(this.VisitMethod(methods[i]));
            }
            return list;
        }

        public virtual MethodList VisitMethodReferenceList(MethodList methods)
        {
            if (methods == null)
            {
                return null;
            }
            int count = methods.Count;
            MethodList list = new MethodList(count);
            for (int i = 0; i < count; i++)
            {
                list.Add((Method) this.VisitMemberReference(methods[i]));
            }
            return list;
        }

        public override Module VisitModule(Module module)
        {
            if (module == null)
            {
                return null;
            }
            Module module2 = (Module) module.Clone();
            if (this.TargetModule == null)
            {
                this.TargetModule = module2;
            }
            this.FindTypesToBeDuplicated(module.Types);
            return base.VisitModule(module2);
        }

        public virtual Module VisitModuleReference(Module module)
        {
            if (module == null)
            {
                return null;
            }
            Module module2 = (Module) this.DuplicateFor[module.UniqueKey];
            if (module2 != null)
            {
                return module2;
            }
            int num = 0;
            int num2 = (this.TargetModule.ModuleReferences == null) ? 0 : this.TargetModule.ModuleReferences.Count;
            while (num < num2)
            {
                ModuleReference reference = this.TargetModule.ModuleReferences[num];
                if ((reference != null) && (string.Compare(module.Name, reference.Name, true, CultureInfo.InvariantCulture) == 0))
                {
                    this.DuplicateFor[module.UniqueKey] = reference.Module;
                    return reference.Module;
                }
                num++;
            }
            if (this.TargetModule.ModuleReferences == null)
            {
                this.TargetModule.ModuleReferences = new ModuleReferenceList();
            }
            this.TargetModule.ModuleReferences.Add(new ModuleReference(module.Name, module));
            this.DuplicateFor[module.UniqueKey] = module;
            return module;
        }

        public override ModuleReference VisitModuleReference(ModuleReference moduleReference)
        {
            if (moduleReference == null)
            {
                return null;
            }
            return base.VisitModuleReference((ModuleReference) moduleReference.Clone());
        }

        public override Expression VisitNamedArgument(NamedArgument namedArgument)
        {
            if (namedArgument == null)
            {
                return null;
            }
            return base.VisitNamedArgument((NamedArgument) namedArgument.Clone());
        }

        public virtual TypeNodeList VisitNestedTypes(TypeNode declaringType, TypeNodeList types)
        {
            if (types == null)
            {
                return null;
            }
            TypeNode targetType = this.TargetType;
            this.TargetType = declaringType;
            TypeNodeList list = types.Clone();
            int num = 0;
            int count = types.Count;
            while (num < count)
            {
                TypeNode type = types[num];
                if (type != null)
                {
                    TypeNode node4;
                    if (TargetPlatform.UseGenerics)
                    {
                        node4 = list[num] = this.VisitTypeNode(type, null, null, null, true);
                    }
                    else
                    {
                        node4 = list[num] = this.VisitTypeReference(type);
                    }
                    if ((node4 != type) && (node4 != null))
                    {
                        if (this.RecordOriginalAsTemplate)
                        {
                            node4.Template = type;
                        }
                        node4.DeclaringType = declaringType;
                        node4.DeclaringModule = declaringType.DeclaringModule;
                    }
                }
                num++;
            }
            int num3 = 0;
            int num4 = types.Count;
            while (num3 < num4)
            {
                TypeNode node6 = types[num3];
                if (node6 != null)
                {
                    TypeNodeList nestedTypes = node6.NestedTypes;
                    if ((nestedTypes != null) && (nestedTypes.Count != 0))
                    {
                        TypeNode node7 = list[num3];
                        if (node7 != null)
                        {
                            this.VisitNestedTypes(node7, nestedTypes);
                        }
                    }
                }
                num3++;
            }
            this.TargetType = targetType;
            return list;
        }

        public override Expression VisitOldExpression(OldExpression oldExpression)
        {
            if (oldExpression == null)
            {
                return null;
            }
            return base.VisitOldExpression((OldExpression) oldExpression.Clone());
        }

        public override Expression VisitParameter(Parameter parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            Parameter parameter2 = (Parameter) this.DuplicateFor[parameter.UniqueKey];
            if (parameter2 != null)
            {
                if (parameter2.DeclaringMethod == null)
                {
                    parameter2.DeclaringMethod = this.TargetMethod;
                }
                return parameter2;
            }
            this.DuplicateFor[parameter.UniqueKey] = parameter2 = (Parameter) parameter.Clone();
            if (parameter2.MarshallingInformation != null)
            {
                parameter2.MarshallingInformation = parameter2.MarshallingInformation.Clone();
            }
            parameter2.DeclaringMethod = this.TargetMethod;
            parameter2.paramArrayElementType = null;
            return base.VisitParameter(parameter2);
        }

        public override ParameterList VisitParameterList(ParameterList parameterList)
        {
            if (parameterList == null)
            {
                return null;
            }
            return base.VisitParameterList(parameterList.Clone());
        }

        public override Expression VisitPostfixExpression(PostfixExpression pExpr)
        {
            if (pExpr == null)
            {
                return null;
            }
            return base.VisitPostfixExpression((PostfixExpression) pExpr.Clone());
        }

        public override Expression VisitPrefixExpression(PrefixExpression pExpr)
        {
            if (pExpr == null)
            {
                return null;
            }
            return base.VisitPrefixExpression((PrefixExpression) pExpr.Clone());
        }

        public override Property VisitProperty(Property property)
        {
            if (property == null)
            {
                return null;
            }
            Property property2 = (Property) this.DuplicateFor[property.UniqueKey];
            if (property2 == null)
            {
                this.DuplicateFor[property.UniqueKey] = property2 = (Property) property.Clone();
                property2.Attributes = this.VisitAttributeList(property.Attributes);
                if (this.CopyDocumentation)
                {
                    property2.Documentation = property.Documentation;
                }
                property2.Type = this.VisitTypeReference(property.Type);
                property2.Getter = this.VisitMethod(property.Getter);
                property2.Setter = this.VisitMethod(property.Setter);
                property2.OtherMethods = this.VisitMethodList(property.OtherMethods);
                property2.DeclaringType = this.TargetType;
                property2.Parameters = this.VisitParameterList(property2.Parameters);
            }
            return property2;
        }

        public override RequiresList VisitRequiresList(RequiresList Requires)
        {
            if (Requires == null)
            {
                return null;
            }
            return base.VisitRequiresList(Requires.Clone());
        }

        public override RequiresOtherwise VisitRequiresOtherwise(RequiresOtherwise otherwise)
        {
            if (otherwise == null)
            {
                return null;
            }
            RequiresOtherwise otherwise2 = (RequiresOtherwise) this.DuplicateFor[otherwise.UniqueKey];
            if (otherwise2 != null)
            {
                return otherwise2;
            }
            this.DuplicateFor[otherwise.UniqueKey] = otherwise2 = (RequiresOtherwise) otherwise.Clone();
            return base.VisitRequiresOtherwise(otherwise2);
        }

        public override RequiresPlain VisitRequiresPlain(RequiresPlain plain)
        {
            if (plain == null)
            {
                return null;
            }
            RequiresPlain plain2 = (RequiresPlain) this.DuplicateFor[plain.UniqueKey];
            if (plain2 != null)
            {
                return plain2;
            }
            this.DuplicateFor[plain.UniqueKey] = plain2 = (RequiresPlain) plain.Clone();
            return base.VisitRequiresPlain(plain2);
        }

        public override Statement VisitReturn(Return Return)
        {
            if (Return == null)
            {
                return null;
            }
            return base.VisitReturn((Return) Return.Clone());
        }

        public override Expression VisitReturnValue(ReturnValue retval)
        {
            if (retval == null)
            {
                return null;
            }
            ReturnValue returnValue = (ReturnValue) this.DuplicateFor[retval.UniqueKey];
            if (returnValue != null)
            {
                return returnValue;
            }
            this.DuplicateFor[retval.UniqueKey] = returnValue = (ReturnValue) retval.Clone();
            return base.VisitReturnValue(returnValue);
        }

        public override SecurityAttribute VisitSecurityAttribute(SecurityAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return base.VisitSecurityAttribute((SecurityAttribute) attribute.Clone());
        }

        public override SecurityAttributeList VisitSecurityAttributeList(SecurityAttributeList attributes)
        {
            if (attributes == null)
            {
                return null;
            }
            return base.VisitSecurityAttributeList(attributes.Clone());
        }

        public override StatementList VisitStatementList(StatementList statements)
        {
            if (statements == null)
            {
                return null;
            }
            return base.VisitStatementList(statements.Clone());
        }

        public override Statement VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            if (switchInstruction == null)
            {
                return null;
            }
            switchInstruction = (SwitchInstruction) base.VisitSwitchInstruction((SwitchInstruction) switchInstruction.Clone());
            if (switchInstruction == null)
            {
                return null;
            }
            switchInstruction.Targets = this.VisitBlockList(switchInstruction.Targets);
            return switchInstruction;
        }

        public virtual TypeNode VisitTemplateTypeReference(TypeNode type)
        {
            if (type == null)
            {
                return null;
            }
            if (!this.RecordOriginalAsTemplate)
            {
                TypeNode node = (TypeNode) this.DuplicateFor[type.UniqueKey];
                if (node != null)
                {
                    return node;
                }
                if (this.TypesToBeDuplicated[type.UniqueKey] != null)
                {
                    TypeNode targetType = this.TargetType;
                    this.TargetType = this.VisitTemplateTypeReference(type.DeclaringType);
                    node = this.VisitTypeNode(type, null, null, null, true);
                    this.TargetType = targetType;
                    return node;
                }
            }
            return type;
        }

        public override Expression VisitTernaryExpression(TernaryExpression expression)
        {
            if (expression == null)
            {
                return null;
            }
            return base.VisitTernaryExpression((TernaryExpression) expression.Clone());
        }

        public override Expression VisitThis(This This)
        {
            if (This == null)
            {
                return null;
            }
            This @this = (This) this.DuplicateFor[This.UniqueKey];
            if (@this != null)
            {
                return @this;
            }
            this.DuplicateFor[This.UniqueKey] = @this = (This) This.Clone();
            return base.VisitThis(@this);
        }

        public override Statement VisitThrow(Throw Throw)
        {
            if (Throw == null)
            {
                return null;
            }
            return base.VisitThrow((Throw) Throw.Clone());
        }

        public override TypeContract VisitTypeContract(TypeContract contract)
        {
            if (this.RecordOriginalAsTemplate)
            {
                return null;
            }
            if (contract == null)
            {
                return null;
            }
            contract = (TypeContract) contract.Clone();
            contract.DeclaringType = this.VisitTypeReference(contract.DeclaringType);
            contract.Invariants = this.VisitInvariantList(contract.invariants);
            return contract;
        }

        public override TypeModifier VisitTypeModifier(TypeModifier typeModifier)
        {
            if (typeModifier == null)
            {
                return null;
            }
            return base.VisitTypeModifier((TypeModifier) typeModifier.Clone());
        }

        public override TypeNode VisitTypeNode(TypeNode type)
        {
            if (type == null)
            {
                return null;
            }
            TypeNode declaringType = this.VisitTypeNode(type, null, null, null, true);
            TypeNodeList nestedTypes = type.NestedTypes;
            if ((nestedTypes != null) && (nestedTypes.Count > 0))
            {
                this.VisitNestedTypes(declaringType, nestedTypes);
            }
            return declaringType;
        }

        internal TypeNode VisitTypeNode(TypeNode type, Identifier mangledName, TypeNodeList templateArguments, TypeNode template, bool delayVisitToNestedTypes)
        {
            if (type == null)
            {
                return null;
            }
            TypeNode node = (TypeNode) this.DuplicateFor[type.UniqueKey];
            if (node == null)
            {
                this.DuplicateFor[type.UniqueKey] = node = (TypeNode) type.Clone();
                if (mangledName != null)
                {
                    this.TargetModule.StructurallyEquivalentType[mangledName.UniqueIdKey] = node;
                    node.TemplateArguments = templateArguments;
                }
                node.arrayTypes = null;
                node.constructors = null;
                node.consolidatedTemplateArguments = null;
                node.consolidatedTemplateParameters = null;
                node.DocumentationId = null;
                if (this.CopyDocumentation)
                {
                    node.Documentation = type.Documentation;
                }
                node.defaultMembers = null;
                node.explicitCoercionFromTable = null;
                node.explicitCoercionMethods = null;
                node.implicitCoercionFromTable = null;
                node.implicitCoercionMethods = null;
                node.implicitCoercionToTable = null;
                node.memberCount = 0;
                node.memberTable = null;
                node.modifierTable = null;
                node.NestedTypes = null;
                node.pointerType = null;
                node.ProviderHandle = null;
                node.ProvideTypeAttributes = null;
                node.ProvideTypeMembers = null;
                node.ProvideNestedTypes = null;
                node.referenceType = null;
                node.runtimeType = null;
                node.structurallyEquivalentMethod = null;
                TypeParameter parameter = node as TypeParameter;
                if (parameter != null)
                {
                    parameter.structuralElementTypes = null;
                }
                ClassParameter parameter2 = node as ClassParameter;
                if (parameter2 != null)
                {
                    parameter2.structuralElementTypes = null;
                }
                node.szArrayTypes = null;
                if (this.RecordOriginalAsTemplate && !(node is ITypeParameter))
                {
                    node.Template = type;
                }
                node.TemplateArguments = null;
                node.TemplateInstances = null;
                node.DeclaringModule = this.TargetModule;
                node.DeclaringType = (node is ITypeParameter) ? null : this.TargetType;
                node.ProviderHandle = type;
                node.Attributes = null;
                node.SecurityAttributes = null;
                node.ProvideTypeAttributes = new TypeNode.TypeAttributeProvider(this.ProvideTypeAttributes);
                Class class2 = node as Class;
                if (class2 != null)
                {
                    class2.BaseClass = null;
                }
                node.Interfaces = null;
                node.templateParameters = null;
                node.consolidatedTemplateParameters = null;
                node.ProvideTypeSignature = new TypeNode.TypeSignatureProvider(this.ProvideTypeSignature);
                if (!this.RecordOriginalAsTemplate)
                {
                    node.members = null;
                    node.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);
                }
                else
                {
                    node.members = null;
                    node.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);
                }
                DelegateNode node2 = node as DelegateNode;
                if (node2 != null)
                {
                    node2.Parameters = null;
                    node2.ReturnType = null;
                }
                node.Contract = null;
                node.membersBeingPopulated = false;
            }
            return node;
        }

        public override TypeNodeList VisitTypeNodeList(TypeNodeList types)
        {
            if (types == null)
            {
                return null;
            }
            types = base.VisitTypeNodeList(types.Clone());
            if (this.TargetModule != null)
            {
                if (types == null)
                {
                    return null;
                }
                if (this.TargetModule.Types == null)
                {
                    this.TargetModule.Types = new TypeNodeList();
                }
                int num = 0;
                int count = types.Count;
                while (num < count)
                {
                    this.TargetModule.Types.Add(types[num]);
                    num++;
                }
            }
            return types;
        }

        public override TypeNode VisitTypeParameter(TypeNode typeParameter)
        {
            if (typeParameter == null)
            {
                return null;
            }
            TypeNode node = (TypeNode) this.DuplicateFor[typeParameter.UniqueKey];
            if (node != null)
            {
                return base.VisitTypeParameter(node);
            }
            if (this.TypesToBeDuplicated[typeParameter.UniqueKey] != null)
            {
                node = this.VisitTypeNode(typeParameter);
                TypeParameter parameter = typeParameter as TypeParameter;
                if (parameter != null)
                {
                    TypeParameter parameter2 = (TypeParameter) node;
                    parameter2.structuralElementTypes = this.VisitTypeReferenceList(parameter.StructuralElementTypes);
                }
                else
                {
                    ClassParameter parameter3 = typeParameter as ClassParameter;
                    if (parameter3 != null)
                    {
                        ClassParameter parameter4 = (ClassParameter) node;
                        parameter4.structuralElementTypes = this.VisitTypeReferenceList(parameter3.StructuralElementTypes);
                    }
                }
            }
            return base.VisitTypeParameter(typeParameter);
        }

        public override TypeNodeList VisitTypeParameterList(TypeNodeList typeParameters)
        {
            if (typeParameters == null)
            {
                return null;
            }
            return base.VisitTypeParameterList(typeParameters.Clone());
        }

        public override TypeNode VisitTypeReference(TypeNode type)
        {
            if (type == null)
            {
                return null;
            }
            TypeNode genericTemplateInstance = (TypeNode) this.DuplicateFor[type.UniqueKey];
            if ((genericTemplateInstance == null) || ((genericTemplateInstance.Template == type) && !this.RecordOriginalAsTemplate))
            {
                TypeNode node2;
                TypeModifier modifier;
                TypeNode node3;
                TypeNode node4;
                TypeNodeList list;
                if (this.RecordOriginalAsTemplate)
                {
                    return type;
                }
                switch (type.NodeType)
                {
                    case NodeType.ClassParameter:
                    case NodeType.TypeParameter:
                        if (this.RecordOriginalAsTemplate)
                        {
                            return type;
                        }
                        if (this.TypesToBeDuplicated[type.UniqueKey] == null)
                        {
                            return type;
                        }
                        genericTemplateInstance = this.VisitTypeNode(type);
                        goto Label_06E8;

                    case NodeType.DelegateNode:
                    {
                        FunctionType type4 = type as FunctionType;
                        if (type4 == null)
                        {
                            break;
                        }
                        genericTemplateInstance = FunctionType.For(this.VisitTypeReference(type4.ReturnType), this.VisitParameterList(type4.Parameters), this.TargetType);
                        goto Label_06E8;
                    }
                    case NodeType.ArrayType:
                    {
                        ArrayType type3 = (ArrayType) type;
                        node2 = this.VisitTypeReference(type3.ElementType);
                        if (node2 == type3.ElementType)
                        {
                            return type3;
                        }
                        if (node2 == null)
                        {
                            return null;
                        }
                        genericTemplateInstance = node2.GetArrayType(type3.Rank, type3.Sizes, type3.LowerBounds);
                        goto Label_06E8;
                    }
                    case NodeType.InterfaceExpression:
                    {
                        InterfaceExpression expression7 = (InterfaceExpression) type.Clone();
                        expression7.Expression = this.VisitExpression(expression7.Expression);
                        expression7.TemplateArguments = this.VisitTypeReferenceList(expression7.TemplateArguments);
                        return expression7;
                    }
                    case NodeType.OptionalModifier:
                        modifier = (TypeModifier) type;
                        node3 = this.VisitTypeReference(modifier.ModifiedType);
                        node4 = this.VisitTypeReference(modifier.Modifier);
                        if ((node3 != null) && (node4 != null))
                        {
                            return OptionalModifier.For(node4, node3);
                        }
                        return null;

                    case NodeType.Pointer:
                    {
                        Pointer pointer = (Pointer) type;
                        node2 = this.VisitTypeReference(pointer.ElementType);
                        if (!(node2 == pointer.ElementType))
                        {
                            if (node2 == null)
                            {
                                return null;
                            }
                            genericTemplateInstance = node2.GetPointerType();
                            goto Label_06E8;
                        }
                        return pointer;
                    }
                    case NodeType.Reference:
                    {
                        Reference reference = (Reference) type;
                        node2 = this.VisitTypeReference(reference.ElementType);
                        if (!(node2 == reference.ElementType))
                        {
                            if (node2 == null)
                            {
                                return null;
                            }
                            genericTemplateInstance = node2.GetReferenceType();
                            goto Label_06E8;
                        }
                        return reference;
                    }
                    case NodeType.RequiredModifier:
                        modifier = (TypeModifier) type;
                        node3 = this.VisitTypeReference(modifier.ModifiedType);
                        node4 = this.VisitTypeReference(modifier.Modifier);
                        if ((node3 != null) && (node4 != null))
                        {
                            return RequiredModifier.For(node4, node3);
                        }
                        return null;

                    case NodeType.FunctionPointer:
                    {
                        FunctionPointer pointer2 = (FunctionPointer) type.Clone();
                        pointer2.ParameterTypes = this.VisitTypeReferenceList(pointer2.ParameterTypes);
                        pointer2.ReturnType = this.VisitTypeReference(pointer2.ReturnType);
                        return pointer2;
                    }
                    case NodeType.BoxedTypeExpression:
                    {
                        BoxedTypeExpression expression2 = (BoxedTypeExpression) type.Clone();
                        expression2.ElementType = this.VisitTypeReference(expression2.ElementType);
                        return expression2;
                    }
                    case NodeType.ClassExpression:
                    {
                        ClassExpression expression3 = (ClassExpression) type.Clone();
                        expression3.Expression = this.VisitExpression(expression3.Expression);
                        expression3.TemplateArguments = this.VisitTypeReferenceList(expression3.TemplateArguments);
                        return expression3;
                    }
                    case NodeType.FlexArrayTypeExpression:
                    {
                        FlexArrayTypeExpression expression4 = (FlexArrayTypeExpression) type.Clone();
                        expression4.ElementType = this.VisitTypeReference(expression4.ElementType);
                        return expression4;
                    }
                    case NodeType.ArrayTypeExpression:
                    {
                        ArrayTypeExpression expression = (ArrayTypeExpression) type.Clone();
                        node2 = this.VisitTypeReference(expression.ElementType);
                        if (node2 != null)
                        {
                            expression.ElementType = node2;
                        }
                        return expression;
                    }
                    case NodeType.NonEmptyStreamTypeExpression:
                    {
                        NonEmptyStreamTypeExpression expression8 = (NonEmptyStreamTypeExpression) type.Clone();
                        expression8.ElementType = this.VisitTypeReference(expression8.ElementType);
                        return expression8;
                    }
                    case NodeType.NonNullableTypeExpression:
                    {
                        NonNullableTypeExpression expression10 = (NonNullableTypeExpression) type.Clone();
                        expression10.ElementType = this.VisitTypeReference(expression10.ElementType);
                        return expression10;
                    }
                    case NodeType.NonNullTypeExpression:
                    {
                        NonNullTypeExpression expression9 = (NonNullTypeExpression) type.Clone();
                        expression9.ElementType = this.VisitTypeReference(expression9.ElementType);
                        return expression9;
                    }
                    case NodeType.NullableTypeExpression:
                    {
                        NullableTypeExpression expression11 = (NullableTypeExpression) type.Clone();
                        expression11.ElementType = this.VisitTypeReference(expression11.ElementType);
                        return expression11;
                    }
                    case NodeType.InvariantTypeExpression:
                    {
                        InvariantTypeExpression expression6 = (InvariantTypeExpression) type.Clone();
                        expression6.ElementType = this.VisitTypeReference(expression6.ElementType);
                        return expression6;
                    }
                    case NodeType.FunctionTypeExpression:
                    {
                        FunctionTypeExpression expression5 = (FunctionTypeExpression) type.Clone();
                        expression5.Parameters = this.VisitParameterList(expression5.Parameters);
                        expression5.ReturnType = this.VisitTypeReference(expression5.ReturnType);
                        return expression5;
                    }
                }
                if (type.Template == null)
                {
                    return type;
                }
                TypeNode template = type.Template;
                if (!this.RecordOriginalAsTemplate)
                {
                    template = this.VisitTemplateTypeReference(type.Template);
                }
                bool flag = template != type.Template;
                int count = type.Template.ConsolidatedTemplateParameters.Count;
                int capacity = template.ConsolidatedTemplateParameters.Count;
                if (capacity != count)
                {
                    int num3 = capacity - count;
                    list = new TypeNodeList(capacity);
                    for (int i = 0; i < capacity; i++)
                    {
                        if (i < num3)
                        {
                            list.Add(template.ConsolidatedTemplateParameters[i]);
                        }
                        else
                        {
                            list.Add(this.VisitTypeReference(type.ConsolidatedTemplateArguments[i - num3]));
                        }
                    }
                }
                else
                {
                    list = (type.ConsolidatedTemplateArguments == null) ? new TypeNodeList() : type.ConsolidatedTemplateArguments.Clone();
                    int num5 = 0;
                    int num6 = (list == null) ? 0 : list.Count;
                    while (num5 < num6)
                    {
                        TypeNode node6 = list[num5];
                        if (node6 != null)
                        {
                            TypeNode node7 = this.VisitTypeReference(node6);
                            if (node6 != node7)
                            {
                                flag = true;
                            }
                            list[num5] = node7;
                        }
                        num5++;
                    }
                }
                if (!flag)
                {
                    this.DuplicateFor[type.UniqueKey] = type;
                    return type;
                }
                genericTemplateInstance = template.GetGenericTemplateInstance(this.TargetModule, list);
                this.DuplicateFor[type.UniqueKey] = genericTemplateInstance;
            }
            return genericTemplateInstance;
        Label_06E8:
            this.DuplicateFor[type.UniqueKey] = genericTemplateInstance;
            return genericTemplateInstance;
        }

        public override TypeNodeList VisitTypeReferenceList(TypeNodeList typeReferences)
        {
            if (typeReferences == null)
            {
                return null;
            }
            return base.VisitTypeReferenceList(typeReferences.Clone());
        }

        public override Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression == null)
            {
                return null;
            }
            unaryExpression = (UnaryExpression) base.VisitUnaryExpression((UnaryExpression) unaryExpression.Clone());
            return unaryExpression;
        }

        public TrivialHashtable DuplicateFor { get; private set; }

        public TrivialHashtable TypesToBeDuplicated { get; private set; }
    }
}

