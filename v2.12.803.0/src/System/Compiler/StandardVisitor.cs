﻿namespace System.Compiler
{
    using System;

    internal class StandardVisitor : Visitor
    {
        public Visitor callingVisitor;
        protected bool memberListNamesChanged;

        public StandardVisitor()
        {
        }

        public StandardVisitor(Visitor callingVisitor)
        {
            this.callingVisitor = callingVisitor;
        }

        public virtual TypeNode GetTypeView(TypeNode type) => 
            System.Compiler.TypeViewer.GetTypeView(this.TypeViewer, type);

        public virtual Visitor GetVisitorFor(Node node)
        {
            if (node == null)
            {
                return null;
            }
            return (Visitor) node.GetVisitorFor(this, base.GetType().Name);
        }

        public override Node Visit(Node node)
        {
            if (node == null)
            {
                return null;
            }
            switch (node.NodeType)
            {
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
                case NodeType.AddEventHandler:
                case NodeType.As:
                case NodeType.Comma:
                case NodeType.ExplicitCoercion:
                case NodeType.Is:
                case NodeType.LogicalAnd:
                case NodeType.LogicalOr:
                case NodeType.NullCoalesingExpression:
                case NodeType.RemoveEventHandler:
                case NodeType.Iff:
                case NodeType.Implies:
                case NodeType.Maplet:
                case NodeType.Range:
                    return this.VisitBinaryExpression((BinaryExpression) node);

                case NodeType.Arglist:
                    return this.VisitExpression((Expression) node);

                case NodeType.Branch:
                    return this.VisitBranch((Branch) node);

                case NodeType.Call:
                case NodeType.Calli:
                case NodeType.Callvirt:
                case NodeType.Jmp:
                case NodeType.MethodCall:
                    return this.VisitMethodCall((MethodCall) node);

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
                case NodeType.Decrement:
                case NodeType.DefaultValue:
                case NodeType.Increment:
                case NodeType.OutAddress:
                case NodeType.Parentheses:
                case NodeType.RefAddress:
                case NodeType.Typeof:
                case NodeType.UnaryPlus:
                    return this.VisitUnaryExpression((UnaryExpression) node);

                case NodeType.Cpblk:
                case NodeType.Initblk:
                    return this.VisitTernaryExpression((TernaryExpression) node);

                case NodeType.DebugBreak:
                    return node;

                case NodeType.Dup:
                    return this.VisitExpression((Expression) node);

                case NodeType.EndFilter:
                    return this.VisitEndFilter((EndFilter) node);

                case NodeType.EndFinally:
                    return this.VisitEndFinally((EndFinally) node);

                case NodeType.Nop:
                case NodeType.SwitchCaseBottom:
                    return node;

                case NodeType.Pop:
                    return this.VisitExpression((Expression) node);

                case NodeType.Rethrow:
                case NodeType.Throw:
                    return this.VisitThrow((Throw) node);

                case NodeType.SwitchInstruction:
                    return this.VisitSwitchInstruction((SwitchInstruction) node);

                case NodeType.AddressDereference:
                    return this.VisitAddressDereference((AddressDereference) node);

                case NodeType.AssignmentStatement:
                    return this.VisitAssignmentStatement((AssignmentStatement) node);

                case NodeType.Block:
                    return this.VisitBlock((Block) node);

                case NodeType.Construct:
                    return this.VisitConstruct((Construct) node);

                case NodeType.ConstructArray:
                    return this.VisitConstructArray((ConstructArray) node);

                case NodeType.ExpressionStatement:
                    return this.VisitExpressionStatement((ExpressionStatement) node);

                case NodeType.Identifier:
                    return this.VisitIdentifier((Identifier) node);

                case NodeType.Indexer:
                    return this.VisitIndexer((Indexer) node);

                case NodeType.Literal:
                    return this.VisitLiteral((Literal) node);

                case NodeType.MemberBinding:
                    return this.VisitMemberBinding((MemberBinding) node);

                case NodeType.NamedArgument:
                    return this.VisitNamedArgument((NamedArgument) node);

                case NodeType.Return:
                    return this.VisitReturn((Return) node);

                case NodeType.This:
                    return this.VisitThis((This) node);

                case NodeType.ArrayType:
                    return null;

                case NodeType.Assembly:
                    return this.VisitAssembly((AssemblyNode) node);

                case NodeType.AssemblyReference:
                    return this.VisitAssemblyReference((AssemblyReference) node);

                case NodeType.Attribute:
                    return this.VisitAttributeNode((AttributeNode) node);

                case NodeType.Class:
                    return this.VisitClass((Class) node);

                case NodeType.ClassParameter:
                case NodeType.TypeParameter:
                    return this.VisitTypeParameter((TypeNode) node);

                case NodeType.DelegateNode:
                    return this.VisitDelegateNode((DelegateNode) node);

                case NodeType.EnumNode:
                    return this.VisitEnumNode((EnumNode) node);

                case NodeType.Event:
                    return this.VisitEvent((Event) node);

                case NodeType.Field:
                    return this.VisitField((Field) node);

                case NodeType.InstanceInitializer:
                    return this.VisitInstanceInitializer((InstanceInitializer) node);

                case NodeType.Interface:
                    return this.VisitInterface((Interface) node);

                case NodeType.Local:
                    return this.VisitLocal((Local) node);

                case NodeType.Method:
                    return this.VisitMethod((Method) node);

                case NodeType.Module:
                    return this.VisitModule((Module) node);

                case NodeType.ModuleReference:
                    return this.VisitModuleReference((ModuleReference) node);

                case NodeType.OptionalModifier:
                case NodeType.RequiredModifier:
                    return this.VisitTypeModifier((TypeModifier) node);

                case NodeType.Parameter:
                    return this.VisitParameter((Parameter) node);

                case NodeType.Property:
                    return this.VisitProperty((Property) node);

                case NodeType.SecurityAttribute:
                    return this.VisitSecurityAttribute((SecurityAttribute) node);

                case NodeType.StaticInitializer:
                    return this.VisitStaticInitializer((StaticInitializer) node);

                case NodeType.Struct:
                    return this.VisitStruct((Struct) node);

                case NodeType.Assertion:
                    return this.VisitAssertion((Assertion) node);

                case NodeType.AssignmentExpression:
                    return this.VisitAssignmentExpression((AssignmentExpression) node);

                case NodeType.Assumption:
                    return this.VisitAssumption((Assumption) node);

                case NodeType.BlockExpression:
                    return this.VisitBlockExpression((BlockExpression) node);

                case NodeType.LabeledStatement:
                    return this.VisitLabeledStatement((LabeledStatement) node);

                case NodeType.PostfixExpression:
                    return this.VisitPostfixExpression((PostfixExpression) node);

                case NodeType.PrefixExpression:
                    return this.VisitPrefixExpression((PrefixExpression) node);

                case NodeType.EnsuresExceptional:
                    return this.VisitEnsuresExceptional((EnsuresExceptional) node);

                case NodeType.EnsuresNormal:
                    return this.VisitEnsuresNormal((EnsuresNormal) node);

                case NodeType.Invariant:
                    return this.VisitInvariant((Invariant) node);

                case NodeType.MethodContract:
                    return this.VisitMethodContract((MethodContract) node);

                case NodeType.OldExpression:
                    return this.VisitOldExpression((OldExpression) node);

                case NodeType.RequiresOtherwise:
                    return this.VisitRequiresOtherwise((RequiresOtherwise) node);

                case NodeType.RequiresPlain:
                    return this.VisitRequiresPlain((RequiresPlain) node);

                case NodeType.ReturnValue:
                    return this.VisitReturnValue((ReturnValue) node);

                case NodeType.TypeContract:
                    return this.VisitTypeContract((TypeContract) node);
            }
            return this.VisitUnknownNodeType(node);
        }

        public virtual Expression VisitAddressDereference(AddressDereference addr)
        {
            if (addr == null)
            {
                return null;
            }
            addr.Address = this.VisitExpression(addr.Address);
            return addr;
        }

        public virtual Expression VisitArglistArgumentExpression(ArglistArgumentExpression argexp)
        {
            if (argexp == null)
            {
                return null;
            }
            argexp.Operands = this.VisitExpressionList(argexp.Operands);
            return argexp;
        }

        public virtual Expression VisitArglistExpression(ArglistExpression argexp)
        {
            if (argexp == null)
            {
                return null;
            }
            return argexp;
        }

        public virtual AssemblyNode VisitAssembly(AssemblyNode assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            this.VisitModule(assembly);
            assembly.ModuleAttributes = this.VisitAttributeList(assembly.ModuleAttributes);
            assembly.SecurityAttributes = this.VisitSecurityAttributeList(assembly.SecurityAttributes);
            return assembly;
        }

        public virtual AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference) => 
            assemblyReference;

        public virtual Statement VisitAssertion(Assertion assertion)
        {
            if (assertion == null)
            {
                return null;
            }
            assertion.Condition = this.VisitExpression(assertion.Condition);
            return assertion;
        }

        public virtual Expression VisitAssignmentExpression(AssignmentExpression assignment)
        {
            if (assignment == null)
            {
                return null;
            }
            assignment.AssignmentStatement = (Statement) this.Visit(assignment.AssignmentStatement);
            return assignment;
        }

        public virtual Statement VisitAssignmentStatement(AssignmentStatement assignment)
        {
            if (assignment == null)
            {
                return null;
            }
            assignment.Target = this.VisitTargetExpression(assignment.Target);
            assignment.Source = this.VisitExpression(assignment.Source);
            return assignment;
        }

        public virtual Statement VisitAssumption(Assumption assumption)
        {
            if (assumption == null)
            {
                return null;
            }
            assumption.Condition = this.VisitExpression(assumption.Condition);
            return assumption;
        }

        public virtual Expression VisitAttributeConstructor(AttributeNode attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return this.VisitExpression(attribute.Constructor);
        }

        public virtual AttributeList VisitAttributeList(AttributeList attributes)
        {
            if (attributes == null)
            {
                return null;
            }
            int num = 0;
            int count = attributes.Count;
            while (num < count)
            {
                attributes[num] = this.VisitAttributeNode(attributes[num]);
                num++;
            }
            return attributes;
        }

        public virtual AttributeNode VisitAttributeNode(AttributeNode attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            attribute.Constructor = this.VisitAttributeConstructor(attribute);
            attribute.Expressions = this.VisitExpressionList(attribute.Expressions);
            return attribute;
        }

        public virtual Expression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression == null)
            {
                return null;
            }
            binaryExpression.Operand1 = this.VisitExpression(binaryExpression.Operand1);
            binaryExpression.Operand2 = this.VisitExpression(binaryExpression.Operand2);
            return binaryExpression;
        }

        public virtual Block VisitBlock(Block block)
        {
            if (block == null)
            {
                return null;
            }
            block.Statements = this.VisitStatementList(block.Statements);
            return block;
        }

        public virtual Expression VisitBlockExpression(BlockExpression blockExpression)
        {
            if (blockExpression == null)
            {
                return null;
            }
            blockExpression.Block = this.VisitBlock(blockExpression.Block);
            return blockExpression;
        }

        public virtual BlockList VisitBlockList(BlockList blockList)
        {
            if (blockList == null)
            {
                return null;
            }
            int num = 0;
            int count = blockList.Count;
            while (num < count)
            {
                blockList[num] = this.VisitBlock(blockList[num]);
                num++;
            }
            return blockList;
        }

        public virtual Statement VisitBranch(Branch branch)
        {
            if (branch == null)
            {
                return null;
            }
            branch.Condition = this.VisitExpression(branch.Condition);
            return branch;
        }

        public virtual Class VisitClass(Class Class) => 
            ((Class) this.VisitTypeNode(Class));

        public virtual Expression VisitConstruct(Construct cons)
        {
            if (cons == null)
            {
                return null;
            }
            cons.Constructor = this.VisitExpression(cons.Constructor);
            cons.Operands = this.VisitExpressionList(cons.Operands);
            cons.Owner = this.VisitExpression(cons.Owner);
            return cons;
        }

        public virtual Expression VisitConstructArray(ConstructArray consArr)
        {
            if (consArr == null)
            {
                return null;
            }
            consArr.ElementType = this.VisitTypeReference(consArr.ElementType);
            consArr.Operands = this.VisitExpressionList(consArr.Operands);
            consArr.Initializers = this.VisitExpressionList(consArr.Initializers);
            consArr.Owner = this.VisitExpression(consArr.Owner);
            return consArr;
        }

        public virtual DelegateNode VisitDelegateNode(DelegateNode delegateNode)
        {
            if (delegateNode == null)
            {
                return null;
            }
            delegateNode = (DelegateNode) this.VisitTypeNode(delegateNode);
            if (delegateNode == null)
            {
                return null;
            }
            delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
            delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
            return delegateNode;
        }

        public virtual Statement VisitEndFilter(EndFilter endFilter)
        {
            if (endFilter == null)
            {
                return null;
            }
            endFilter.Value = this.VisitExpression(endFilter.Value);
            return endFilter;
        }

        public virtual Statement VisitEndFinally(EndFinally endFinally) => 
            endFinally;

        public virtual EnsuresExceptional VisitEnsuresExceptional(EnsuresExceptional exceptional)
        {
            if (exceptional == null)
            {
                return null;
            }
            exceptional.PostCondition = this.VisitExpression(exceptional.PostCondition);
            exceptional.Type = this.VisitTypeReference(exceptional.Type);
            exceptional.Variable = this.VisitExpression(exceptional.Variable);
            exceptional.UserMessage = this.VisitExpression(exceptional.UserMessage);
            return exceptional;
        }

        public virtual EnsuresList VisitEnsuresList(EnsuresList Ensures)
        {
            if (Ensures == null)
            {
                return null;
            }
            int num = 0;
            int count = Ensures.Count;
            while (num < count)
            {
                Ensures[num] = (Ensures) this.Visit(Ensures[num]);
                num++;
            }
            return Ensures;
        }

        public virtual EnsuresNormal VisitEnsuresNormal(EnsuresNormal normal)
        {
            if (normal == null)
            {
                return null;
            }
            normal.PostCondition = this.VisitExpression(normal.PostCondition);
            normal.UserMessage = this.VisitExpression(normal.UserMessage);
            return normal;
        }

        public virtual EnumNode VisitEnumNode(EnumNode enumNode) => 
            ((EnumNode) this.VisitTypeNode(enumNode));

        public virtual Event VisitEvent(Event evnt)
        {
            if (evnt == null)
            {
                return null;
            }
            evnt.Attributes = this.VisitAttributeList(evnt.Attributes);
            evnt.HandlerType = this.VisitTypeReference(evnt.HandlerType);
            return evnt;
        }

        public virtual Expression VisitExpression(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }
            NodeType nodeType = expression.NodeType;
            if ((nodeType != NodeType.Arglist) && (nodeType != NodeType.Dup))
            {
                if (nodeType != NodeType.Pop)
                {
                    return (Expression) this.Visit(expression);
                }
            }
            else
            {
                return expression;
            }
            UnaryExpression expression2 = expression as UnaryExpression;
            if (expression2 != null)
            {
                expression2.Operand = this.VisitExpression(expression2.Operand);
                return expression2;
            }
            return expression;
        }

        public override ExpressionList VisitExpressionList(ExpressionList expressions)
        {
            if (expressions == null)
            {
                return null;
            }
            int num = 0;
            int count = expressions.Count;
            while (num < count)
            {
                expressions[num] = this.VisitExpression(expressions[num]);
                num++;
            }
            return expressions;
        }

        public virtual Statement VisitExpressionStatement(ExpressionStatement statement)
        {
            if (statement == null)
            {
                return null;
            }
            statement.Expression = this.VisitExpression(statement.Expression);
            return statement;
        }

        public virtual Statement VisitFaultHandler(FaultHandler faultHandler)
        {
            if (faultHandler == null)
            {
                return null;
            }
            faultHandler.Block = this.VisitBlock(faultHandler.Block);
            return faultHandler;
        }

        public virtual FaultHandlerList VisitFaultHandlerList(FaultHandlerList faultHandlers)
        {
            if (faultHandlers == null)
            {
                return null;
            }
            int num = 0;
            int count = faultHandlers.Count;
            while (num < count)
            {
                faultHandlers[num] = (FaultHandler) this.VisitFaultHandler(faultHandlers[num]);
                num++;
            }
            return faultHandlers;
        }

        public virtual Field VisitField(Field field)
        {
            if (field == null)
            {
                return null;
            }
            field.Attributes = this.VisitAttributeList(field.Attributes);
            field.Type = this.VisitTypeReference(field.Type);
            field.Initializer = this.VisitExpression(field.Initializer);
            field.ImplementedInterfaces = this.VisitInterfaceReferenceList(field.ImplementedInterfaces);
            return field;
        }

        public virtual Expression VisitIdentifier(Identifier identifier) => 
            identifier;

        public virtual Expression VisitIndexer(Indexer indexer)
        {
            if (indexer == null)
            {
                return null;
            }
            indexer.Object = this.VisitExpression(indexer.Object);
            indexer.Operands = this.VisitExpressionList(indexer.Operands);
            return indexer;
        }

        public virtual InstanceInitializer VisitInstanceInitializer(InstanceInitializer cons) => 
            ((InstanceInitializer) this.VisitMethod(cons));

        public virtual Interface VisitInterface(Interface Interface) => 
            ((Interface) this.VisitTypeNode(Interface));

        public virtual Interface VisitInterfaceReference(Interface Interface) => 
            ((Interface) this.VisitTypeReference(Interface));

        public virtual InterfaceList VisitInterfaceReferenceList(InterfaceList interfaceReferences)
        {
            if (interfaceReferences == null)
            {
                return null;
            }
            int num = 0;
            int count = interfaceReferences.Count;
            while (num < count)
            {
                interfaceReferences[num] = this.VisitInterfaceReference(interfaceReferences[num]);
                num++;
            }
            return interfaceReferences;
        }

        public virtual Invariant VisitInvariant(Invariant invariant)
        {
            if (invariant == null)
            {
                return null;
            }
            invariant.Condition = this.VisitExpression(invariant.Condition);
            return invariant;
        }

        public virtual InvariantList VisitInvariantList(InvariantList invariants)
        {
            if (invariants == null)
            {
                return null;
            }
            int num = 0;
            int count = invariants.Count;
            while (num < count)
            {
                invariants[num] = this.VisitInvariant(invariants[num]);
                num++;
            }
            return invariants;
        }

        public virtual Statement VisitLabeledStatement(LabeledStatement lStatement)
        {
            if (lStatement == null)
            {
                return null;
            }
            lStatement.Statement = (Statement) this.Visit(lStatement.Statement);
            return lStatement;
        }

        public virtual Expression VisitLiteral(Literal literal) => 
            literal;

        public virtual Expression VisitLocal(Local local)
        {
            if (local == null)
            {
                return null;
            }
            local.Type = this.VisitTypeReference(local.Type);
            LocalBinding binding = local as LocalBinding;
            if (binding != null)
            {
                Local local2 = this.VisitLocal(binding.BoundLocal) as Local;
                if (local2 != null)
                {
                    binding.BoundLocal = local2;
                }
            }
            return local;
        }

        public virtual Expression VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding == null)
            {
                return null;
            }
            memberBinding.TargetObject = this.VisitExpression(memberBinding.TargetObject);
            return memberBinding;
        }

        public virtual MemberList VisitMemberList(MemberList members)
        {
            this.memberListNamesChanged = false;
            if (members == null)
            {
                return null;
            }
            int num = 0;
            int count = members.Count;
            while (num < count)
            {
                Member node = members[num];
                if (node != null)
                {
                    Identifier name = node.Name;
                    members[num] = (Member) this.Visit(node);
                    if (((members[num] != null) && (name != null)) && ((members[num].Name != null) && (members[num].Name.UniqueIdKey != name.UniqueIdKey)))
                    {
                        this.memberListNamesChanged = true;
                    }
                }
                num++;
            }
            return members;
        }

        public virtual Method VisitMethod(Method method)
        {
            if (method == null)
            {
                return null;
            }
            method.Attributes = this.VisitAttributeList(method.Attributes);
            method.ReturnAttributes = this.VisitAttributeList(method.ReturnAttributes);
            method.SecurityAttributes = this.VisitSecurityAttributeList(method.SecurityAttributes);
            method.ReturnType = this.VisitTypeReference(method.ReturnType);
            method.Parameters = this.VisitParameterList(method.Parameters);
            if (TargetPlatform.UseGenerics)
            {
                method.TemplateArguments = this.VisitTypeReferenceList(method.TemplateArguments);
                method.TemplateParameters = this.VisitTypeParameterList(method.TemplateParameters);
            }
            method.Contract = this.VisitMethodContract(method.Contract);
            method.Body = this.VisitBlock(method.Body);
            return method;
        }

        public virtual Expression VisitMethodCall(MethodCall call)
        {
            if (call == null)
            {
                return null;
            }
            call.Callee = this.VisitExpression(call.Callee);
            call.Operands = this.VisitExpressionList(call.Operands);
            call.Constraint = this.VisitTypeReference(call.Constraint);
            return call;
        }

        public virtual MethodContract VisitMethodContract(MethodContract contract)
        {
            if (contract == null)
            {
                return null;
            }
            contract.Requires = this.VisitRequiresList(contract.Requires);
            contract.Ensures = this.VisitEnsuresList(contract.Ensures);
            contract.ModelEnsures = this.VisitEnsuresList(contract.ModelEnsures);
            contract.Modifies = this.VisitExpressionList(contract.Modifies);
            return contract;
        }

        public virtual Module VisitModule(Module module)
        {
            if (module == null)
            {
                return null;
            }
            module.Attributes = this.VisitAttributeList(module.Attributes);
            module.Types = this.VisitTypeNodeList(module.Types);
            return module;
        }

        public virtual ModuleReference VisitModuleReference(ModuleReference moduleReference) => 
            moduleReference;

        public virtual Expression VisitNamedArgument(NamedArgument namedArgument)
        {
            if (namedArgument == null)
            {
                return null;
            }
            namedArgument.Value = this.VisitExpression(namedArgument.Value);
            return namedArgument;
        }

        public virtual Expression VisitOldExpression(OldExpression oldExpression)
        {
            if (oldExpression == null)
            {
                return null;
            }
            oldExpression.expression = this.VisitExpression(oldExpression.expression);
            return oldExpression;
        }

        public virtual Expression VisitParameter(Parameter parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            parameter.Attributes = this.VisitAttributeList(parameter.Attributes);
            parameter.Type = this.VisitTypeReference(parameter.Type);
            parameter.DefaultValue = this.VisitExpression(parameter.DefaultValue);
            ParameterBinding binding = parameter as ParameterBinding;
            if (binding != null)
            {
                Parameter parameter2 = this.VisitParameter(binding.BoundParameter) as Parameter;
                if (parameter2 != null)
                {
                    binding.BoundParameter = parameter2;
                }
            }
            return parameter;
        }

        public virtual ParameterList VisitParameterList(ParameterList parameterList)
        {
            if (parameterList == null)
            {
                return null;
            }
            int num = 0;
            int count = parameterList.Count;
            while (num < count)
            {
                parameterList[num] = (Parameter) this.VisitParameter(parameterList[num]);
                num++;
            }
            return parameterList;
        }

        public virtual Expression VisitPostfixExpression(PostfixExpression pExpr)
        {
            if (pExpr == null)
            {
                return null;
            }
            pExpr.Expression = this.VisitExpression(pExpr.Expression);
            return pExpr;
        }

        public virtual Expression VisitPrefixExpression(PrefixExpression pExpr)
        {
            if (pExpr == null)
            {
                return null;
            }
            pExpr.Expression = this.VisitExpression(pExpr.Expression);
            return pExpr;
        }

        public virtual Property VisitProperty(Property property)
        {
            if (property == null)
            {
                return null;
            }
            property.Attributes = this.VisitAttributeList(property.Attributes);
            property.Parameters = this.VisitParameterList(property.Parameters);
            property.Type = this.VisitTypeReference(property.Type);
            return property;
        }

        public virtual RequiresList VisitRequiresList(RequiresList Requires)
        {
            if (Requires == null)
            {
                return null;
            }
            int num = 0;
            int count = Requires.Count;
            while (num < count)
            {
                Requires[num] = (Requires) this.Visit(Requires[num]);
                num++;
            }
            return Requires;
        }

        public virtual RequiresOtherwise VisitRequiresOtherwise(RequiresOtherwise otherwise)
        {
            if (otherwise == null)
            {
                return null;
            }
            otherwise.Condition = this.VisitExpression(otherwise.Condition);
            otherwise.ThrowException = this.VisitExpression(otherwise.ThrowException);
            otherwise.UserMessage = this.VisitExpression(otherwise.UserMessage);
            return otherwise;
        }

        public virtual RequiresPlain VisitRequiresPlain(RequiresPlain plain)
        {
            if (plain == null)
            {
                return null;
            }
            plain.Condition = this.VisitExpression(plain.Condition);
            plain.UserMessage = this.VisitExpression(plain.UserMessage);
            plain.ExceptionType = this.VisitTypeReference(plain.ExceptionType);
            return plain;
        }

        public virtual Statement VisitReturn(Return Return)
        {
            if (Return == null)
            {
                return null;
            }
            Return.Expression = this.VisitExpression(Return.Expression);
            return Return;
        }

        public virtual Expression VisitReturnValue(ReturnValue returnValue)
        {
            if (returnValue == null)
            {
                return null;
            }
            returnValue.Type = this.VisitTypeReference(returnValue.Type);
            return returnValue;
        }

        public virtual SecurityAttribute VisitSecurityAttribute(SecurityAttribute attribute) => 
            attribute;

        public virtual SecurityAttributeList VisitSecurityAttributeList(SecurityAttributeList attributes)
        {
            if (attributes == null)
            {
                return null;
            }
            int num = 0;
            int count = attributes.Count;
            while (num < count)
            {
                attributes[num] = this.VisitSecurityAttribute(attributes[num]);
                num++;
            }
            return attributes;
        }

        public virtual StatementList VisitStatementList(StatementList statements)
        {
            if (statements == null)
            {
                return null;
            }
            int num = 0;
            int count = statements.Count;
            while (num < count)
            {
                statements[num] = (Statement) this.Visit(statements[num]);
                num++;
            }
            return statements;
        }

        public virtual StaticInitializer VisitStaticInitializer(StaticInitializer cons) => 
            ((StaticInitializer) this.VisitMethod(cons));

        public virtual Struct VisitStruct(Struct Struct) => 
            ((Struct) this.VisitTypeNode(Struct));

        public virtual Statement VisitSwitchInstruction(SwitchInstruction switchInstruction)
        {
            if (switchInstruction == null)
            {
                return null;
            }
            switchInstruction.Expression = this.VisitExpression(switchInstruction.Expression);
            return switchInstruction;
        }

        public virtual Expression VisitTargetExpression(Expression expression) => 
            this.VisitExpression(expression);

        public virtual Expression VisitTernaryExpression(TernaryExpression expression)
        {
            if (expression == null)
            {
                return null;
            }
            expression.Operand1 = this.VisitExpression(expression.Operand1);
            expression.Operand2 = this.VisitExpression(expression.Operand2);
            expression.Operand3 = this.VisitExpression(expression.Operand3);
            return expression;
        }

        public virtual Expression VisitThis(This This)
        {
            if (This == null)
            {
                return null;
            }
            This.Type = this.VisitTypeReference(This.Type);
            return This;
        }

        public virtual Statement VisitThrow(Throw Throw)
        {
            if (Throw == null)
            {
                return null;
            }
            Throw.Expression = this.VisitExpression(Throw.Expression);
            return Throw;
        }

        public virtual TypeContract VisitTypeContract(TypeContract contract)
        {
            if (contract == null)
            {
                return null;
            }
            contract.Invariants = this.VisitInvariantList(contract.Invariants);
            return contract;
        }

        public virtual TypeModifier VisitTypeModifier(TypeModifier typeModifier)
        {
            if (typeModifier == null)
            {
                return null;
            }
            typeModifier.Modifier = this.VisitTypeReference(typeModifier.Modifier);
            typeModifier.ModifiedType = this.VisitTypeReference(typeModifier.ModifiedType);
            return typeModifier;
        }

        public virtual TypeNode VisitTypeNode(TypeNode typeNode)
        {
            if (typeNode == null)
            {
                return null;
            }
            typeNode.Attributes = this.VisitAttributeList(typeNode.Attributes);
            typeNode.SecurityAttributes = this.VisitSecurityAttributeList(typeNode.SecurityAttributes);
            Class class2 = typeNode as Class;
            if (class2 != null)
            {
                class2.BaseClass = (Class) this.VisitTypeReference(class2.BaseClass);
            }
            typeNode.Interfaces = this.VisitInterfaceReferenceList(typeNode.Interfaces);
            typeNode.TemplateArguments = this.VisitTypeReferenceList(typeNode.TemplateArguments);
            typeNode.TemplateParameters = this.VisitTypeParameterList(typeNode.TemplateParameters);
            this.VisitMemberList(typeNode.Members);
            if (this.memberListNamesChanged)
            {
                typeNode.ClearMemberTable();
            }
            typeNode.Contract = this.VisitTypeContract(typeNode.Contract);
            return typeNode;
        }

        public virtual TypeNodeList VisitTypeNodeList(TypeNodeList types)
        {
            if (types == null)
            {
                return null;
            }
            for (int i = 0; i < types.Count; i++)
            {
                types[i] = (TypeNode) this.Visit(types[i]);
            }
            return types;
        }

        public virtual TypeNode VisitTypeParameter(TypeNode typeParameter)
        {
            if (typeParameter == null)
            {
                return null;
            }
            Class class2 = typeParameter as Class;
            if (class2 != null)
            {
                class2.BaseClass = (Class) this.VisitTypeReference(class2.BaseClass);
            }
            typeParameter.Attributes = this.VisitAttributeList(typeParameter.Attributes);
            typeParameter.Interfaces = this.VisitInterfaceReferenceList(typeParameter.Interfaces);
            return typeParameter;
        }

        public virtual TypeNodeList VisitTypeParameterList(TypeNodeList typeParameters)
        {
            if (typeParameters == null)
            {
                return null;
            }
            int num = 0;
            int count = typeParameters.Count;
            while (num < count)
            {
                typeParameters[num] = this.VisitTypeParameter(typeParameters[num]);
                num++;
            }
            return typeParameters;
        }

        public virtual TypeNode VisitTypeReference(TypeNode type) => 
            type;

        public virtual TypeReference VisitTypeReference(TypeReference type) => 
            type;

        public virtual TypeNodeList VisitTypeReferenceList(TypeNodeList typeReferences)
        {
            if (typeReferences == null)
            {
                return null;
            }
            int num = 0;
            int count = typeReferences.Count;
            while (num < count)
            {
                typeReferences[num] = this.VisitTypeReference(typeReferences[num]);
                num++;
            }
            return typeReferences;
        }

        public virtual Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression == null)
            {
                return null;
            }
            unaryExpression.Operand = this.VisitExpression(unaryExpression.Operand);
            return unaryExpression;
        }

        public virtual Node VisitUnknownNodeType(Node node)
        {
            Visitor visitorFor = this.GetVisitorFor(node);
            if (visitorFor != null)
            {
                if (this.callingVisitor != null)
                {
                    this.callingVisitor.TransferStateTo(visitorFor);
                }
                this.TransferStateTo(visitorFor);
                node = visitorFor.Visit(node);
                visitorFor.TransferStateTo(this);
                if (this.callingVisitor != null)
                {
                    visitorFor.TransferStateTo(this.callingVisitor);
                }
            }
            return node;
        }

        public virtual System.Compiler.TypeViewer TypeViewer =>
            null;
    }
}

