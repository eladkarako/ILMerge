namespace System.Compiler
{
    using System;

    internal class MethodBodySpecializer : Specializer
    {
        public TrivialHashtable alreadyVisitedNodes;
        public Method dummyMethod;
        public Method methodBeingSpecialized;

        public MethodBodySpecializer(Visitor callingVisitor) : base(callingVisitor)
        {
            this.alreadyVisitedNodes = new TrivialHashtable();
        }

        public MethodBodySpecializer(Module module, TypeNodeList pars, TypeNodeList args) : base(module, pars, args)
        {
            this.alreadyVisitedNodes = new TrivialHashtable();
        }

        public override Node Visit(Node node)
        {
            Literal literal = node as Literal;
            if ((literal != null) && (literal.Value == null))
            {
                return literal;
            }
            Expression expression = node as Expression;
            if (((expression != null) && !(expression is Local)) && !(expression is Parameter))
            {
                expression.Type = this.VisitTypeReference(expression.Type);
            }
            return base.Visit(node);
        }

        public override Expression VisitAddressDereference(AddressDereference addr)
        {
            if (addr == null)
            {
                return null;
            }
            bool flag = (addr.Address != null) && (addr.Address.NodeType == NodeType.Unbox);
            addr.Address = this.VisitExpression(addr.Address);
            if (addr.Address == null)
            {
                return null;
            }
            if (flag && (addr.Address.NodeType != NodeType.Unbox))
            {
                return addr.Address;
            }
            Reference type = addr.Address.Type as Reference;
            if (type != null)
            {
                addr.Type = type.ElementType;
            }
            return addr;
        }

        public override Statement VisitAssignmentStatement(AssignmentStatement assignment)
        {
            assignment = (AssignmentStatement) base.VisitAssignmentStatement(assignment);
            if (assignment == null)
            {
                return null;
            }
            Expression target = assignment.Target;
            Expression source = assignment.Source;
            TypeNode type = target?.Type;
            TypeNode node2 = source?.Type;
            if ((type != null) && (node2 != null))
            {
                if (type.IsValueType)
                {
                    if (node2 is Reference)
                    {
                        assignment.Source = new AddressDereference(source, type);
                        return assignment;
                    }
                    if (!node2.IsValueType && (((node2 != CoreSystemTypes.Object) || !(source is Literal)) || (target.NodeType != NodeType.AddressDereference)))
                    {
                        assignment.Source = new AddressDereference(new BinaryExpression(source, new MemberBinding(null, node2), NodeType.Unbox), node2);
                    }
                    return assignment;
                }
                if (node2.IsValueType && !(type is Reference))
                {
                    assignment.Source = new BinaryExpression(source, new MemberBinding(null, node2), NodeType.Box, type);
                }
            }
            return assignment;
        }

        public override Expression VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression == null)
            {
                return null;
            }
            bool flag = (binaryExpression.Operand1 != null) && (binaryExpression.Operand1.NodeType == NodeType.Isinst);
            binaryExpression = (BinaryExpression) base.VisitBinaryExpression(binaryExpression);
            if (binaryExpression == null)
            {
                return null;
            }
            Expression expression = binaryExpression.Operand1;
            Expression expression2 = binaryExpression.Operand2;
            Literal literal = expression2 as Literal;
            TypeNode type = (literal == null) ? null : (literal.Value as TypeNode);
            if (binaryExpression.NodeType == NodeType.Castclass)
            {
                if (type == null)
                {
                    return binaryExpression;
                }
                if (type.IsValueType)
                {
                    return new AddressDereference(new BinaryExpression(expression, literal, NodeType.Unbox), type) { Type = type };
                }
                if (((expression == null) || (expression.Type == null)) || !expression.Type.IsValueType)
                {
                    return binaryExpression;
                }
                return new BinaryExpression(expression, new MemberBinding(null, expression.Type), NodeType.Box, type);
            }
            if (binaryExpression.NodeType == NodeType.Unbox)
            {
                if (((expression != null) && (expression.Type != null)) && expression.Type.IsValueType)
                {
                    return expression;
                }
                return binaryExpression;
            }
            if (binaryExpression.NodeType == NodeType.Eq)
            {
                if ((((literal == null) || (literal.Value != null)) || ((expression == null) || (expression.Type == null))) || !expression.Type.IsValueType)
                {
                    literal = expression as Literal;
                    if ((((literal == null) || (literal.Value != null)) || ((expression2 == null) || (expression2.Type == null))) || !expression2.Type.IsValueType)
                    {
                        return binaryExpression;
                    }
                }
                return Literal.False;
            }
            if (binaryExpression.NodeType == NodeType.Ne)
            {
                if ((((literal != null) && (literal.Value == null)) && ((expression != null) && (expression.Type != null))) && expression.Type.IsValueType)
                {
                    if (flag && (expression.Type == CoreSystemTypes.Boolean))
                    {
                        return expression;
                    }
                    return Literal.True;
                }
                literal = expression as Literal;
                if ((((literal == null) || (literal.Value != null)) || ((expression2 == null) || (expression2.Type == null))) || !expression2.Type.IsValueType)
                {
                    return binaryExpression;
                }
                return Literal.True;
            }
            if (((binaryExpression.NodeType != NodeType.Isinst) || (expression == null)) || ((expression.Type == null) || !expression.Type.IsValueType))
            {
                return binaryExpression;
            }
            if (expression.Type == type)
            {
                return Literal.True;
            }
            return Literal.False;
        }

        public override Statement VisitBranch(Branch branch)
        {
            branch = (Branch) base.VisitBranch(branch);
            if (branch == null)
            {
                return null;
            }
            if ((branch.Condition != null) && !(branch.Condition is BinaryExpression))
            {
                TypeNode type = branch.Condition.Type;
                if (((type == null) || type.IsPrimitiveInteger) || ((type == CoreSystemTypes.Boolean) || !type.IsValueType))
                {
                    return branch;
                }
                if (branch.Condition.NodeType == NodeType.LogicalNot)
                {
                    return null;
                }
                branch.Condition = null;
            }
            return branch;
        }

        public override Expression VisitConstruct(Construct cons)
        {
            cons = (Construct) base.VisitConstruct(cons);
            if (cons == null)
            {
                return null;
            }
            MemberBinding constructor = cons.Constructor as MemberBinding;
            if (constructor != null)
            {
                Method boundMember = constructor.BoundMember as Method;
                if (boundMember == null)
                {
                    return cons;
                }
                ParameterList parameters = boundMember.Parameters;
                if (parameters == null)
                {
                    return cons;
                }
                ExpressionList operands = cons.Operands;
                int count = (operands == null) ? 0 : operands.Count;
                if (count > parameters.Count)
                {
                    count = parameters.Count;
                }
                for (int i = 0; i < count; i++)
                {
                    Expression expression = operands[i];
                    if (expression != null)
                    {
                        Parameter parameter = parameters[i];
                        if ((((parameter != null) && (expression.Type != null)) && ((parameter.Type != null) && expression.Type.IsValueType)) && !parameter.Type.IsValueType)
                        {
                            operands[i] = new BinaryExpression(expression, new MemberBinding(null, expression.Type), NodeType.Box, parameter.Type);
                        }
                    }
                }
            }
            return cons;
        }

        public override Expression VisitExpression(Expression expression)
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
                expression.Type = this.VisitTypeReference(expression.Type);
                return expression;
            }
            expression.Type = this.VisitTypeReference(expression.Type);
            UnaryExpression expression2 = expression as UnaryExpression;
            if (expression2 != null)
            {
                expression2.Operand = this.VisitExpression(expression2.Operand);
                return expression2;
            }
            return expression;
        }

        public override Expression VisitIndexer(Indexer indexer)
        {
            indexer = (Indexer) base.VisitIndexer(indexer);
            if ((indexer == null) || (indexer.Object == null))
            {
                return null;
            }
            ArrayType type = indexer.Object.Type as ArrayType;
            if (type != null)
            {
                indexer.Type = indexer.ElementType = type.ElementType;
                return indexer;
            }
            indexer.ElementType = this.VisitTypeReference(indexer.ElementType);
            return indexer;
        }

        public override Expression VisitLiteral(Literal literal)
        {
            if (literal == null)
            {
                return null;
            }
            TypeNode type = literal.Value as TypeNode;
            if ((type != null) && (literal.Type == CoreSystemTypes.Type))
            {
                return new Literal(this.VisitTypeReference(type), literal.Type, literal.SourceContext);
            }
            return (Literal) literal.Clone();
        }

        public override Expression VisitLocal(Local local)
        {
            if (local == null)
            {
                return null;
            }
            if (this.alreadyVisitedNodes[local.UniqueKey] != null)
            {
                return local;
            }
            this.alreadyVisitedNodes[local.UniqueKey] = local;
            return base.VisitLocal(local);
        }

        public override Expression VisitMemberBinding(MemberBinding memberBinding)
        {
            if (memberBinding == null)
            {
                return null;
            }
            Expression expression2 = memberBinding.TargetObject = this.VisitExpression(memberBinding.TargetObject);
            Member methodBeingSpecialized = this.VisitMemberReference(memberBinding.BoundMember);
            if (methodBeingSpecialized == this.dummyMethod)
            {
                methodBeingSpecialized = this.methodBeingSpecialized;
            }
            memberBinding.BoundMember = methodBeingSpecialized;
            if (memberBinding == null)
            {
                return null;
            }
            Method boundMember = memberBinding.BoundMember as Method;
            if ((((boundMember != null) && (expression2 != null)) && ((expression2.Type != null) && expression2.Type.IsValueType)) && (expression2.NodeType != NodeType.This))
            {
                if ((boundMember.DeclaringType != null) && boundMember.DeclaringType.IsValueType)
                {
                    memberBinding.TargetObject = new UnaryExpression(memberBinding.TargetObject, NodeType.AddressOf, memberBinding.TargetObject.Type.GetReferenceType());
                }
                else
                {
                    MemberBinding binding = new MemberBinding(null, memberBinding.TargetObject.Type);
                    memberBinding.TargetObject = new BinaryExpression(memberBinding.TargetObject, binding, NodeType.Box, boundMember.DeclaringType);
                }
            }
            TypeNode type = memberBinding.BoundMember as TypeNode;
            if (type != null)
            {
                TypeNode node2 = this.VisitTypeReference(type);
                memberBinding.BoundMember = node2;
            }
            return memberBinding;
        }

        public override Method VisitMethod(Method method)
        {
            if (method == null)
            {
                return null;
            }
            Method currentMethod = base.CurrentMethod;
            TypeNode currentType = base.CurrentType;
            base.CurrentMethod = method;
            base.CurrentType = method.DeclaringType;
            method.Body = this.VisitBlock(method.Body);
            base.CurrentMethod = currentMethod;
            base.CurrentType = currentType;
            return method;
        }

        public override Expression VisitMethodCall(MethodCall call)
        {
            call = (MethodCall) base.VisitMethodCall(call);
            if (call == null)
            {
                return null;
            }
            MemberBinding callee = call.Callee as MemberBinding;
            if (callee != null)
            {
                Method boundMember = callee.BoundMember as Method;
                if (boundMember == null)
                {
                    return call;
                }
                ParameterList parameters = boundMember.Parameters;
                if (parameters == null)
                {
                    return call;
                }
                ExpressionList operands = call.Operands;
                int count = (operands == null) ? 0 : operands.Count;
                if (count > parameters.Count)
                {
                    count = parameters.Count;
                }
                for (int i = 0; i < count; i++)
                {
                    Expression expression = operands[i];
                    if (expression != null)
                    {
                        Parameter parameter = parameters[i];
                        if ((((parameter != null) && (expression.Type != null)) && ((parameter.Type != null) && expression.Type.IsValueType)) && !parameter.Type.IsValueType)
                        {
                            operands[i] = new BinaryExpression(expression, new MemberBinding(null, expression.Type), NodeType.Box, parameter.Type);
                        }
                    }
                }
                if (((boundMember.ReturnType != null) && (call.Type != null)) && (boundMember.ReturnType.IsValueType && !call.Type.IsValueType))
                {
                    return new BinaryExpression(call, new MemberBinding(null, boundMember.ReturnType), NodeType.Box, call.Type);
                }
            }
            return call;
        }

        public override Expression VisitParameter(Parameter parameter)
        {
            ParameterBinding binding = parameter as ParameterBinding;
            if ((binding != null) && (binding.BoundParameter != null))
            {
                binding.Type = binding.BoundParameter.Type;
            }
            return parameter;
        }

        public override Statement VisitReturn(Return Return)
        {
            Return = (Return) base.VisitReturn(Return);
            if (Return == null)
            {
                return null;
            }
            Expression expression = Return.Expression;
            if ((((expression != null) && (expression.Type != null)) && ((base.CurrentMethod != null) && (base.CurrentMethod.ReturnType != null))) && (expression.Type.IsValueType && !base.CurrentMethod.ReturnType.IsValueType))
            {
                Return.Expression = new BinaryExpression(expression, new MemberBinding(null, expression.Type), NodeType.Box, base.CurrentMethod.ReturnType);
            }
            return Return;
        }

        public override TypeNode VisitTypeNode(TypeNode typeNode)
        {
            if (typeNode == null)
            {
                return null;
            }
            TypeNode currentType = base.CurrentType;
            base.CurrentType = typeNode;
            MemberList members = typeNode.Members;
            int num = 0;
            int num2 = (members == null) ? 0 : members.Count;
            while (num < num2)
            {
                Member member = members[num];
                TypeNode node2 = member as TypeNode;
                if (node2 != null)
                {
                    this.VisitTypeNode(node2);
                }
                else
                {
                    Method method = member as Method;
                    if (method != null)
                    {
                        this.VisitMethod(method);
                    }
                }
                num++;
            }
            base.CurrentType = currentType;
            return typeNode;
        }

        public override Expression VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            if (unaryExpression == null)
            {
                return null;
            }
            return base.VisitUnaryExpression((UnaryExpression) unaryExpression.Clone());
        }
    }
}

