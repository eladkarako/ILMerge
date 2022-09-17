namespace System.Compiler
{
    using System;

    internal class Inspector
    {
        public virtual void Visit(Node node)
        {
            if (node != null)
            {
                switch (node.NodeType)
                {
                    case NodeType.OldExpression:
                        this.VisitOldExpression((OldExpression) node);
                        return;

                    case NodeType.Range:
                    case NodeType.Read:
                    case NodeType.Requires:
                    case NodeType.RequiresValidation:
                    case NodeType.ExceptionHandler:
                    case NodeType.Catch:
                    case NodeType.FaultHandler:
                    case NodeType.Filter:
                    case NodeType.Finally:
                    case NodeType.Instruction:
                    case NodeType.InterfaceExpression:
                    case NodeType.Namespace:
                    case NodeType.Try:
                    case NodeType.FunctionPointer:
                    case NodeType.Pointer:
                    case NodeType.Reference:
                    case NodeType.Array:
                    case NodeType.BlockReference:
                    case NodeType.CompilationParameters:
                    case NodeType.Document:
                    case NodeType.EndOfRecord:
                    case NodeType.Expression:
                    case NodeType.Guid:
                    case NodeType.List:
                    case NodeType.MarshallingInformation:
                    case NodeType.Member:
                    case NodeType.MemberReference:
                    case NodeType.MissingBlockReference:
                    case NodeType.MissingExpression:
                    case NodeType.MissingMemberReference:
                    case NodeType.String:
                    case NodeType.StringDictionary:
                    case NodeType.TypeNode:
                    case NodeType.Uri:
                    case NodeType.XmlNode:
                    case NodeType.AddEventHandler:
                    case NodeType.AliasDefinition:
                    case NodeType.AnonymousNestedFunction:
                    case NodeType.ApplyToAll:
                    case NodeType.ArglistArgumentExpression:
                    case NodeType.ArglistExpression:
                    case NodeType.ArrayTypeExpression:
                    case NodeType.As:
                    case NodeType.Base:
                    case NodeType.BoxedTypeExpression:
                    case NodeType.ClassExpression:
                    case NodeType.CoerceTuple:
                    case NodeType.CollectionEnumerator:
                    case NodeType.Comma:
                    case NodeType.Compilation:
                    case NodeType.CompilationUnit:
                    case NodeType.CompilationUnitSnippet:
                    case NodeType.Conditional:
                    case NodeType.ConstructDelegate:
                    case NodeType.ConstructFlexArray:
                    case NodeType.ConstructIterator:
                    case NodeType.ConstructTuple:
                    case NodeType.Continue:
                    case NodeType.CopyReference:
                    case NodeType.CurrentClosure:
                    case NodeType.Decrement:
                    case NodeType.DefaultValue:
                    case NodeType.DoWhile:
                    case NodeType.Exit:
                    case NodeType.ExplicitCoercion:
                    case NodeType.ExpressionSnippet:
                    case NodeType.FieldInitializerBlock:
                    case NodeType.Fixed:
                    case NodeType.FlexArrayTypeExpression:
                    case NodeType.For:
                    case NodeType.ForEach:
                    case NodeType.FunctionDeclaration:
                    case NodeType.FunctionTypeExpression:
                    case NodeType.Goto:
                    case NodeType.GotoCase:
                    case NodeType.If:
                    case NodeType.ImplicitThis:
                    case NodeType.Increment:
                    case NodeType.InvariantTypeExpression:
                    case NodeType.Is:
                    case NodeType.LabeledStatement:
                    case NodeType.LocalDeclaration:
                    case NodeType.LocalDeclarationsStatement:
                    case NodeType.Lock:
                    case NodeType.LogicalAnd:
                    case NodeType.LogicalOr:
                    case NodeType.LRExpression:
                    case NodeType.Iff:
                    case NodeType.Implies:
                        goto Label_070F;

                    case NodeType.RequiresOtherwise:
                        this.VisitRequiresOtherwise((RequiresOtherwise) node);
                        return;

                    case NodeType.RequiresPlain:
                        this.VisitRequiresPlain((RequiresPlain) node);
                        return;

                    case NodeType.ReturnValue:
                        this.VisitReturnValue((ReturnValue) node);
                        return;

                    case NodeType.TypeContract:
                        this.VisitTypeContract((TypeContract) node);
                        return;

                    case NodeType.MethodContract:
                        this.VisitMethodContract((MethodContract) node);
                        return;

                    case NodeType.Add:
                    case NodeType.Add_Ovf:
                    case NodeType.Add_Ovf_Un:
                    case NodeType.And:
                    case NodeType.Box:
                    case NodeType.Castclass:
                    case NodeType.Ceq:
                    case NodeType.Cgt:
                    case NodeType.Cgt_Un:
                    case NodeType.Clt:
                    case NodeType.Clt_Un:
                    case NodeType.Div:
                    case NodeType.Div_Un:
                    case NodeType.Isinst:
                    case NodeType.Ldvirtftn:
                    case NodeType.Mkrefany:
                    case NodeType.Mul:
                    case NodeType.Mul_Ovf:
                    case NodeType.Mul_Ovf_Un:
                    case NodeType.Or:
                    case NodeType.Refanyval:
                    case NodeType.Rem:
                    case NodeType.Rem_Un:
                    case NodeType.Shl:
                    case NodeType.Shr:
                    case NodeType.Shr_Un:
                    case NodeType.Sub:
                    case NodeType.Sub_Ovf:
                    case NodeType.Sub_Ovf_Un:
                    case NodeType.Unbox:
                    case NodeType.UnboxAny:
                    case NodeType.Xor:
                    case NodeType.Eq:
                    case NodeType.Ge:
                    case NodeType.Gt:
                    case NodeType.Le:
                    case NodeType.Lt:
                    case NodeType.Ne:
                        this.VisitBinaryExpression((BinaryExpression) node);
                        return;

                    case NodeType.Arglist:
                        this.VisitExpression((Expression) node);
                        break;

                    case NodeType.Branch:
                        this.VisitBranch((Branch) node);
                        return;

                    case NodeType.Call:
                    case NodeType.Calli:
                    case NodeType.Callvirt:
                    case NodeType.Jmp:
                    case NodeType.MethodCall:
                        this.VisitMethodCall((MethodCall) node);
                        return;

                    case NodeType.Ckfinite:
                    case NodeType.Conv_I:
                    case NodeType.Conv_I1:
                    case NodeType.Conv_I2:
                    case NodeType.Conv_I4:
                    case NodeType.Conv_I8:
                    case NodeType.Conv_Ovf_I:
                    case NodeType.Conv_Ovf_I_Un:
                    case NodeType.Conv_Ovf_I1:
                    case NodeType.Conv_Ovf_I1_Un:
                    case NodeType.Conv_Ovf_I2:
                    case NodeType.Conv_Ovf_I2_Un:
                    case NodeType.Conv_Ovf_I4:
                    case NodeType.Conv_Ovf_I4_Un:
                    case NodeType.Conv_Ovf_I8:
                    case NodeType.Conv_Ovf_I8_Un:
                    case NodeType.Conv_Ovf_U:
                    case NodeType.Conv_Ovf_U_Un:
                    case NodeType.Conv_Ovf_U1:
                    case NodeType.Conv_Ovf_U1_Un:
                    case NodeType.Conv_Ovf_U2:
                    case NodeType.Conv_Ovf_U2_Un:
                    case NodeType.Conv_Ovf_U4:
                    case NodeType.Conv_Ovf_U4_Un:
                    case NodeType.Conv_Ovf_U8:
                    case NodeType.Conv_Ovf_U8_Un:
                    case NodeType.Conv_R_Un:
                    case NodeType.Conv_R4:
                    case NodeType.Conv_R8:
                    case NodeType.Conv_U:
                    case NodeType.Conv_U1:
                    case NodeType.Conv_U2:
                    case NodeType.Conv_U4:
                    case NodeType.Conv_U8:
                    case NodeType.Ldftn:
                    case NodeType.Ldlen:
                    case NodeType.Ldtoken:
                    case NodeType.Localloc:
                    case NodeType.Neg:
                    case NodeType.Not:
                    case NodeType.ReadOnlyAddressOf:
                    case NodeType.Refanytype:
                    case NodeType.Sizeof:
                    case NodeType.SkipCheck:
                    case NodeType.AddressOf:
                    case NodeType.LogicalNot:
                        this.VisitUnaryExpression((UnaryExpression) node);
                        return;

                    case NodeType.Cpblk:
                    case NodeType.Initblk:
                        this.VisitTernaryExpression((TernaryExpression) node);
                        return;

                    case NodeType.DebugBreak:
                        return;

                    case NodeType.Dup:
                        this.VisitExpression((Expression) node);
                        return;

                    case NodeType.EndFilter:
                        this.VisitEndFilter((EndFilter) node);
                        return;

                    case NodeType.EndFinally:
                        this.VisitEndFinally((EndFinally) node);
                        return;

                    case NodeType.Nop:
                        return;

                    case NodeType.Pop:
                        this.VisitExpression((Expression) node);
                        return;

                    case NodeType.Rethrow:
                    case NodeType.Throw:
                        this.VisitThrow((Throw) node);
                        return;

                    case NodeType.SwitchInstruction:
                        this.VisitSwitchInstruction((SwitchInstruction) node);
                        return;

                    case NodeType.AddressDereference:
                        this.VisitAddressDereference((AddressDereference) node);
                        return;

                    case NodeType.AssignmentStatement:
                        this.VisitAssignmentStatement((AssignmentStatement) node);
                        return;

                    case NodeType.Block:
                        this.VisitBlock((Block) node);
                        return;

                    case NodeType.Construct:
                        this.VisitConstruct((Construct) node);
                        return;

                    case NodeType.ConstructArray:
                        this.VisitConstructArray((ConstructArray) node);
                        return;

                    case NodeType.ExpressionStatement:
                        this.VisitExpressionStatement((ExpressionStatement) node);
                        return;

                    case NodeType.Identifier:
                        this.VisitIdentifier((Identifier) node);
                        return;

                    case NodeType.Indexer:
                        this.VisitIndexer((Indexer) node);
                        return;

                    case NodeType.Literal:
                        this.VisitLiteral((Literal) node);
                        return;

                    case NodeType.MemberBinding:
                        this.VisitMemberBinding((MemberBinding) node);
                        return;

                    case NodeType.NamedArgument:
                        this.VisitNamedArgument((NamedArgument) node);
                        return;

                    case NodeType.Return:
                        this.VisitReturn((Return) node);
                        return;

                    case NodeType.This:
                        this.VisitThis((This) node);
                        return;

                    case NodeType.ArrayType:
                        break;

                    case NodeType.Assembly:
                        this.VisitAssembly((AssemblyNode) node);
                        return;

                    case NodeType.AssemblyReference:
                        this.VisitAssemblyReference((AssemblyReference) node);
                        return;

                    case NodeType.Attribute:
                        this.VisitAttributeNode((AttributeNode) node);
                        return;

                    case NodeType.Class:
                        this.VisitClass((Class) node);
                        return;

                    case NodeType.ClassParameter:
                    case NodeType.TypeParameter:
                        this.VisitTypeParameter((TypeNode) node);
                        return;

                    case NodeType.DelegateNode:
                        this.VisitDelegateNode((DelegateNode) node);
                        return;

                    case NodeType.EnumNode:
                        this.VisitEnumNode((EnumNode) node);
                        return;

                    case NodeType.Event:
                        this.VisitEvent((Event) node);
                        return;

                    case NodeType.Field:
                        this.VisitField((Field) node);
                        return;

                    case NodeType.InstanceInitializer:
                        this.VisitInstanceInitializer((InstanceInitializer) node);
                        return;

                    case NodeType.Interface:
                        this.VisitInterface((Interface) node);
                        return;

                    case NodeType.Local:
                        this.VisitLocal((Local) node);
                        return;

                    case NodeType.Method:
                        this.VisitMethod((Method) node);
                        return;

                    case NodeType.Module:
                        this.VisitModule((Module) node);
                        return;

                    case NodeType.ModuleReference:
                        this.VisitModuleReference((ModuleReference) node);
                        return;

                    case NodeType.OptionalModifier:
                    case NodeType.RequiredModifier:
                        this.VisitTypeModifier((TypeModifier) node);
                        return;

                    case NodeType.Parameter:
                        this.VisitParameter((Parameter) node);
                        return;

                    case NodeType.Property:
                        this.VisitProperty((Property) node);
                        return;

                    case NodeType.SecurityAttribute:
                        this.VisitSecurityAttribute((SecurityAttribute) node);
                        return;

                    case NodeType.StaticInitializer:
                        this.VisitStaticInitializer((StaticInitializer) node);
                        return;

                    case NodeType.Struct:
                        this.VisitStruct((Struct) node);
                        return;

                    case NodeType.Assertion:
                        this.VisitAssertion((Assertion) node);
                        return;

                    case NodeType.AssignmentExpression:
                        this.VisitAssignmentExpression((AssignmentExpression) node);
                        return;

                    case NodeType.Assumption:
                        this.VisitAssumption((Assumption) node);
                        return;

                    case NodeType.BlockExpression:
                        this.VisitBlockExpression((BlockExpression) node);
                        return;

                    case NodeType.EnsuresExceptional:
                        this.VisitEnsuresExceptional((EnsuresExceptional) node);
                        return;

                    case NodeType.EnsuresNormal:
                        this.VisitEnsuresNormal((EnsuresNormal) node);
                        return;

                    case NodeType.Invariant:
                        this.VisitInvariant((Invariant) node);
                        return;

                    default:
                        goto Label_070F;
                }
            }
            return;
        Label_070F:
            this.VisitUnknownNodeType(node);
        }

        public virtual void VisitAddressDereference(AddressDereference addr)
        {
            if (addr != null)
            {
                this.VisitExpression(addr.Address);
            }
        }

        public virtual void VisitAssembly(AssemblyNode assembly)
        {
            if (assembly != null)
            {
                this.VisitModule(assembly);
                this.VisitAttributeList(assembly.ModuleAttributes);
                this.VisitSecurityAttributeList(assembly.SecurityAttributes);
            }
        }

        public virtual AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference) => 
            assemblyReference;

        public virtual void VisitAssertion(Assertion assertion)
        {
            if (assertion != null)
            {
                this.VisitExpression(assertion.Condition);
            }
        }

        public virtual void VisitAssignmentExpression(AssignmentExpression assignment)
        {
            if (assignment != null)
            {
                this.Visit(assignment.AssignmentStatement);
            }
        }

        public virtual void VisitAssignmentStatement(AssignmentStatement assignment)
        {
            if (assignment != null)
            {
                this.VisitTargetExpression(assignment.Target);
                this.VisitExpression(assignment.Source);
            }
        }

        public virtual void VisitAssumption(Assumption assumption)
        {
            if (assumption != null)
            {
                this.VisitExpression(assumption.Condition);
            }
        }

        public virtual void VisitAttributeConstructor(AttributeNode attribute)
        {
            if (attribute != null)
            {
                this.VisitExpression(attribute.Constructor);
            }
        }

        public virtual void VisitAttributeList(AttributeList attributes)
        {
            if (attributes != null)
            {
                int num = 0;
                int count = attributes.Count;
                while (num < count)
                {
                    this.VisitAttributeNode(attributes[num]);
                    num++;
                }
            }
        }

        public virtual void VisitAttributeNode(AttributeNode attribute)
        {
            if (attribute != null)
            {
                this.VisitAttributeConstructor(attribute);
                this.VisitExpressionList(attribute.Expressions);
            }
        }

        public virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression != null)
            {
                this.VisitExpression(binaryExpression.Operand1);
                this.VisitExpression(binaryExpression.Operand2);
            }
        }

        public virtual void VisitBlock(Block block)
        {
            if (block != null)
            {
                this.VisitStatementList(block.Statements);
            }
        }

        public virtual void VisitBlockExpression(BlockExpression blockExpression)
        {
            if (blockExpression != null)
            {
                this.VisitBlock(blockExpression.Block);
            }
        }

        public virtual void VisitBlockList(BlockList blockList)
        {
            if (blockList != null)
            {
                int num = 0;
                int count = blockList.Count;
                while (num < count)
                {
                    this.VisitBlock(blockList[num]);
                    num++;
                }
            }
        }

        public virtual void VisitBranch(Branch branch)
        {
            if (branch != null)
            {
                this.VisitExpression(branch.Condition);
            }
        }

        public virtual void VisitClass(Class Class)
        {
            this.VisitTypeNode(Class);
        }

        public virtual void VisitConstruct(Construct cons)
        {
            if (cons != null)
            {
                this.VisitExpression(cons.Constructor);
                this.VisitExpressionList(cons.Operands);
            }
        }

        public virtual void VisitConstructArray(ConstructArray consArr)
        {
            if (consArr != null)
            {
                this.VisitTypeReference(consArr.ElementType);
                this.VisitExpressionList(consArr.Operands);
                this.VisitExpressionList(consArr.Initializers);
                this.VisitExpression(consArr.Owner);
            }
        }

        public virtual void VisitDelegateNode(DelegateNode delegateNode)
        {
            if (delegateNode != null)
            {
                this.VisitTypeNode(delegateNode);
                this.VisitParameterList(delegateNode.Parameters);
                this.VisitTypeReference(delegateNode.ReturnType);
            }
        }

        public virtual void VisitEndFilter(EndFilter endFilter)
        {
            if (endFilter != null)
            {
                this.VisitExpression(endFilter.Value);
            }
        }

        public virtual Statement VisitEndFinally(EndFinally endFinally) => 
            endFinally;

        public virtual void VisitEnsuresExceptional(EnsuresExceptional exceptional)
        {
            if (exceptional != null)
            {
                this.VisitExpression(exceptional.PostCondition);
                this.VisitTypeReference(exceptional.Type);
                this.VisitExpression(exceptional.Variable);
            }
        }

        public virtual void VisitEnsuresList(EnsuresList Ensures)
        {
            if (Ensures != null)
            {
                int num = 0;
                int count = Ensures.Count;
                while (num < count)
                {
                    this.Visit(Ensures[num]);
                    num++;
                }
            }
        }

        public virtual void VisitEnsuresNormal(EnsuresNormal normal)
        {
            if (normal != null)
            {
                this.VisitExpression(normal.PostCondition);
            }
        }

        public virtual void VisitEnumNode(EnumNode enumNode)
        {
            this.VisitTypeNode(enumNode);
        }

        public virtual void VisitEvent(Event evnt)
        {
            if (evnt != null)
            {
                this.VisitAttributeList(evnt.Attributes);
                this.VisitTypeReference(evnt.HandlerType);
            }
        }

        public virtual void VisitExpression(Expression expression)
        {
            if (expression != null)
            {
                NodeType nodeType = expression.NodeType;
                if ((nodeType != NodeType.Arglist) && (nodeType != NodeType.Dup))
                {
                    if (nodeType == NodeType.Pop)
                    {
                        UnaryExpression expression2 = expression as UnaryExpression;
                        if (expression2 != null)
                        {
                            this.VisitExpression(expression2.Operand);
                        }
                    }
                    else
                    {
                        this.Visit(expression);
                    }
                }
            }
        }

        public void VisitExpressionList(ExpressionList expressions)
        {
            if (expressions != null)
            {
                int num = 0;
                int count = expressions.Count;
                while (num < count)
                {
                    this.VisitExpression(expressions[num]);
                    num++;
                }
            }
        }

        public virtual void VisitExpressionStatement(ExpressionStatement statement)
        {
            if (statement != null)
            {
                this.VisitExpression(statement.Expression);
            }
        }

        public virtual void VisitField(Field field)
        {
            if (field != null)
            {
                this.VisitAttributeList(field.Attributes);
                this.VisitTypeReference(field.Type);
            }
        }

        public virtual void VisitIdentifier(Identifier identifier)
        {
        }

        public virtual void VisitIndexer(Indexer indexer)
        {
            if (indexer != null)
            {
                this.VisitExpression(indexer.Object);
                this.VisitExpressionList(indexer.Operands);
            }
        }

        public virtual void VisitInstanceInitializer(InstanceInitializer cons)
        {
            this.VisitMethod(cons);
        }

        public virtual void VisitInterface(Interface Interface)
        {
            this.VisitTypeNode(Interface);
        }

        public virtual void VisitInterfaceReference(Interface Interface)
        {
            this.VisitTypeReference(Interface);
        }

        public virtual void VisitInterfaceReferenceList(InterfaceList interfaceReferences)
        {
            if (interfaceReferences != null)
            {
                int num = 0;
                int count = interfaceReferences.Count;
                while (num < count)
                {
                    this.VisitInterfaceReference(interfaceReferences[num]);
                    num++;
                }
            }
        }

        public virtual void VisitInvariant(Invariant invariant)
        {
            if (invariant != null)
            {
                this.VisitExpression(invariant.Condition);
            }
        }

        public virtual void VisitInvariantList(InvariantList invariants)
        {
            if (invariants != null)
            {
                int num = 0;
                int count = invariants.Count;
                while (num < count)
                {
                    this.VisitInvariant(invariants[num]);
                    num++;
                }
            }
        }

        public virtual void VisitLiteral(Literal literal)
        {
        }

        public virtual void VisitLocal(Local local)
        {
            if (local != null)
            {
                this.VisitTypeReference(local.Type);
            }
        }

        public virtual void VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding != null)
            {
                this.VisitExpression(memberBinding.TargetObject);
            }
        }

        public virtual void VisitMemberList(MemberList members)
        {
            if (members != null)
            {
                int num = 0;
                int count = members.Count;
                while (num < count)
                {
                    this.Visit(members[num]);
                    num++;
                }
            }
        }

        public virtual void VisitMethod(Method method)
        {
            if (method != null)
            {
                this.VisitAttributeList(method.Attributes);
                this.VisitAttributeList(method.ReturnAttributes);
                this.VisitSecurityAttributeList(method.SecurityAttributes);
                this.VisitTypeReference(method.ReturnType);
                this.VisitParameterList(method.Parameters);
                if (TargetPlatform.UseGenerics)
                {
                    this.VisitTypeReferenceList(method.TemplateArguments);
                    this.VisitTypeParameterList(method.TemplateParameters);
                }
                this.VisitMethodContract(method.Contract);
                this.VisitBlock(method.Body);
            }
        }

        public virtual void VisitMethodCall(MethodCall call)
        {
            if (call != null)
            {
                this.VisitExpression(call.Callee);
                this.VisitExpressionList(call.Operands);
                this.VisitTypeReference(call.Constraint);
            }
        }

        public virtual void VisitMethodContract(MethodContract contract)
        {
            if (contract != null)
            {
                this.VisitRequiresList(contract.Requires);
                this.VisitEnsuresList(contract.Ensures);
                this.VisitEnsuresList(contract.ModelEnsures);
                this.VisitExpressionList(contract.Modifies);
            }
        }

        public virtual void VisitModule(Module module)
        {
            if (module != null)
            {
                this.VisitAttributeList(module.Attributes);
                this.VisitTypeNodeList(module.Types);
            }
        }

        public virtual void VisitModuleReference(ModuleReference moduleReference)
        {
        }

        public virtual void VisitNamedArgument(NamedArgument namedArgument)
        {
            if (namedArgument != null)
            {
                this.VisitExpression(namedArgument.Value);
            }
        }

        public virtual void VisitOldExpression(OldExpression oldExpression)
        {
            if (oldExpression != null)
            {
                this.VisitExpression(oldExpression.expression);
            }
        }

        public virtual void VisitParameter(Parameter parameter)
        {
            if (parameter != null)
            {
                this.VisitAttributeList(parameter.Attributes);
                this.VisitTypeReference(parameter.Type);
                this.VisitExpression(parameter.DefaultValue);
            }
        }

        public virtual void VisitParameterList(ParameterList parameterList)
        {
            if (parameterList != null)
            {
                int num = 0;
                int count = parameterList.Count;
                while (num < count)
                {
                    this.VisitParameter(parameterList[num]);
                    num++;
                }
            }
        }

        public virtual void VisitProperty(Property property)
        {
            if (property != null)
            {
                this.VisitAttributeList(property.Attributes);
                this.VisitParameterList(property.Parameters);
                this.VisitTypeReference(property.Type);
            }
        }

        public virtual void VisitRequiresList(RequiresList Requires)
        {
            if (Requires != null)
            {
                int num = 0;
                int count = Requires.Count;
                while (num < count)
                {
                    this.Visit(Requires[num]);
                    num++;
                }
            }
        }

        public virtual void VisitRequiresOtherwise(RequiresOtherwise otherwise)
        {
            if (otherwise != null)
            {
                this.VisitExpression(otherwise.Condition);
                this.VisitExpression(otherwise.ThrowException);
            }
        }

        public virtual void VisitRequiresPlain(RequiresPlain plain)
        {
            if (plain != null)
            {
                this.VisitExpression(plain.Condition);
            }
        }

        public virtual void VisitReturn(Return Return)
        {
            if (Return != null)
            {
                this.VisitExpression(Return.Expression);
            }
        }

        public virtual void VisitReturnValue(ReturnValue returnValue)
        {
            if (returnValue != null)
            {
                this.VisitTypeReference(returnValue.Type);
            }
        }

        public virtual void VisitSecurityAttribute(SecurityAttribute attribute)
        {
        }

        public virtual void VisitSecurityAttributeList(SecurityAttributeList attributes)
        {
            if (attributes != null)
            {
                int num = 0;
                int count = attributes.Count;
                while (num < count)
                {
                    this.VisitSecurityAttribute(attributes[num]);
                    num++;
                }
            }
        }

        public virtual void VisitStatementList(StatementList statements)
        {
            if (statements != null)
            {
                int num = 0;
                int count = statements.Count;
                while (num < count)
                {
                    this.Visit(statements[num]);
                    num++;
                }
            }
        }

        public virtual void VisitStaticInitializer(StaticInitializer cons)
        {
            this.VisitMethod(cons);
        }

        public virtual void VisitStruct(Struct Struct)
        {
            this.VisitTypeNode(Struct);
        }

        public virtual void VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            if (switchInstruction != null)
            {
                this.VisitExpression(switchInstruction.Expression);
            }
        }

        public virtual void VisitTargetExpression(Expression expression)
        {
            this.VisitExpression(expression);
        }

        public virtual void VisitTernaryExpression(TernaryExpression expression)
        {
            if (expression != null)
            {
                this.VisitExpression(expression.Operand1);
                this.VisitExpression(expression.Operand2);
                this.VisitExpression(expression.Operand3);
            }
        }

        public virtual void VisitThis(This This)
        {
            if (This != null)
            {
                this.VisitTypeReference(This.Type);
            }
        }

        public virtual void VisitThrow(Throw Throw)
        {
            if (Throw != null)
            {
                this.VisitExpression(Throw.Expression);
            }
        }

        public virtual void VisitTypeContract(TypeContract contract)
        {
            if (contract != null)
            {
                this.VisitInvariantList(contract.Invariants);
            }
        }

        public virtual void VisitTypeModifier(TypeModifier typeModifier)
        {
            if (typeModifier != null)
            {
                this.VisitTypeReference(typeModifier.Modifier);
                this.VisitTypeReference(typeModifier.ModifiedType);
            }
        }

        public virtual void VisitTypeNode(TypeNode typeNode)
        {
            if (typeNode != null)
            {
                this.VisitAttributeList(typeNode.Attributes);
                this.VisitSecurityAttributeList(typeNode.SecurityAttributes);
                Class class2 = typeNode as Class;
                if (class2 != null)
                {
                    this.VisitTypeReference(class2.BaseClass);
                }
                this.VisitInterfaceReferenceList(typeNode.Interfaces);
                this.VisitTypeReferenceList(typeNode.TemplateArguments);
                this.VisitTypeParameterList(typeNode.TemplateParameters);
                this.VisitMemberList(typeNode.Members);
                this.VisitTypeContract(typeNode.Contract);
            }
        }

        public virtual void VisitTypeNodeList(TypeNodeList types)
        {
            if (types != null)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    this.Visit(types[i]);
                }
            }
        }

        public virtual void VisitTypeParameter(TypeNode typeParameter)
        {
            if (typeParameter != null)
            {
                Class class2 = typeParameter as Class;
                if (class2 != null)
                {
                    this.VisitTypeReference(class2.BaseClass);
                }
                this.VisitAttributeList(typeParameter.Attributes);
                this.VisitInterfaceReferenceList(typeParameter.Interfaces);
            }
        }

        public virtual void VisitTypeParameterList(TypeNodeList typeParameters)
        {
            if (typeParameters != null)
            {
                int num = 0;
                int count = typeParameters.Count;
                while (num < count)
                {
                    this.VisitTypeParameter(typeParameters[num]);
                    num++;
                }
            }
        }

        public virtual void VisitTypeReference(TypeNode type)
        {
        }

        public virtual void VisitTypeReference(TypeReference type)
        {
        }

        public virtual void VisitTypeReferenceList(TypeNodeList typeReferences)
        {
            if (typeReferences != null)
            {
                int num = 0;
                int count = typeReferences.Count;
                while (num < count)
                {
                    this.VisitTypeReference(typeReferences[num]);
                    num++;
                }
            }
        }

        public virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression != null)
            {
                this.VisitExpression(unaryExpression.Operand);
            }
        }

        public virtual void VisitUnknownNodeType(Node node)
        {
        }
    }
}

