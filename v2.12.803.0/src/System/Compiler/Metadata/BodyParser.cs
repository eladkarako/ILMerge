namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Runtime.InteropServices;

    internal sealed class BodyParser : ILParser
    {
        private int alignment;
        private readonly TrivialHashtable blockMap;
        private TypeNode constraint;
        private int ilOffset;
        private bool isReadOnly;
        private bool isTailCall;
        private bool isVolatile;
        private SourceContext lastSourceContext;
        private OpCode opCode;
        private readonly ExpressionStack operandStack;

        internal BodyParser(Reader reader, Method method, int methodIndex, int RVA) : base(reader, method, methodIndex, RVA)
        {
            this.operandStack = new ExpressionStack();
            this.blockMap = new TrivialHashtable(0x80);
            this.alignment = -1;
        }

        private void CreateBlocksForBranchTargets()
        {
            int position = base.bodyReader.Position;
            while (base.counter < base.size)
            {
                this.ProcessOneILInstruction();
            }
            base.counter = 0;
            base.bodyReader.Position = position;
        }

        private AssignmentStatement ParseArrayElementAssignment(OpCode opCode)
        {
            Expression source = this.PopOperand();
            ExpressionList arguments = new ExpressionList(1);
            arguments.Add(this.PopOperand());
            Expression expression2 = this.PopOperand();
            Indexer target = new Indexer(expression2, arguments);
            TypeNode intPtr = CoreSystemTypes.Object;
            switch (opCode)
            {
                case OpCode.Stelem_I:
                    intPtr = CoreSystemTypes.IntPtr;
                    break;

                case OpCode.Stelem_I1:
                    intPtr = CoreSystemTypes.Int8;
                    break;

                case OpCode.Stelem_I2:
                    intPtr = CoreSystemTypes.Int16;
                    break;

                case OpCode.Stelem_I4:
                    intPtr = CoreSystemTypes.Int32;
                    break;

                case OpCode.Stelem_I8:
                    intPtr = CoreSystemTypes.Int64;
                    break;

                case OpCode.Stelem_R4:
                    intPtr = CoreSystemTypes.Single;
                    break;

                case OpCode.Stelem_R8:
                    intPtr = CoreSystemTypes.Double;
                    break;

                case OpCode.Stelem:
                    intPtr = (TypeNode) base.GetMemberFromToken();
                    break;

                default:
                {
                    ArrayType type = expression2.Type as ArrayType;
                    if (type != null)
                    {
                        intPtr = type.ElementType;
                    }
                    break;
                }
            }
            target.ElementType = target.Type = intPtr;
            return new AssignmentStatement(target, source);
        }

        private Indexer ParseArrayElementLoad(OpCode opCode, TypeNode elementType)
        {
            ExpressionList arguments = new ExpressionList(1);
            arguments.Add(this.PopOperand());
            Expression expression = this.PopOperand();
            Indexer indexer = new Indexer(expression, arguments);
            TypeNode intPtr = elementType;
            switch (opCode)
            {
                case OpCode.Ldelem_I1:
                    intPtr = CoreSystemTypes.Int8;
                    break;

                case OpCode.Ldelem_U1:
                    intPtr = CoreSystemTypes.UInt8;
                    break;

                case OpCode.Ldelem_I2:
                    intPtr = CoreSystemTypes.Int16;
                    break;

                case OpCode.Ldelem_U2:
                    intPtr = CoreSystemTypes.UInt16;
                    break;

                case OpCode.Ldelem_I4:
                    intPtr = CoreSystemTypes.Int32;
                    break;

                case OpCode.Ldelem_U4:
                    intPtr = CoreSystemTypes.UInt32;
                    break;

                case OpCode.Ldelem_I8:
                    intPtr = CoreSystemTypes.Int64;
                    break;

                case OpCode.Ldelem_I:
                    intPtr = CoreSystemTypes.IntPtr;
                    break;

                case OpCode.Ldelem_R4:
                    intPtr = CoreSystemTypes.Single;
                    break;

                case OpCode.Ldelem_R8:
                    intPtr = CoreSystemTypes.Double;
                    break;

                case OpCode.Ldelem:
                    intPtr = (TypeNode) base.GetMemberFromToken();
                    break;

                default:
                    if (intPtr == null)
                    {
                        intPtr = CoreSystemTypes.Object;
                        ArrayType type = expression.Type as ArrayType;
                        if (type != null)
                        {
                            intPtr = type.ElementType;
                        }
                    }
                    break;
            }
            indexer.ElementType = indexer.Type = intPtr;
            return indexer;
        }

        private UnaryExpression ParseArrayElementLoadAddress()
        {
            TypeNode memberFromToken = (TypeNode) base.GetMemberFromToken();
            return new UnaryExpression(this.ParseArrayElementLoad(OpCode.Nop, memberFromToken), this.isReadOnly ? NodeType.ReadOnlyAddressOf : NodeType.AddressOf, memberFromToken.GetReferenceType());
        }

        private BinaryExpression ParseBinaryComparison(NodeType oper) => 
            new BinaryExpression(this.PopOperand(), this.PopOperand(), oper) { Type = CoreSystemTypes.Int32 };

        private BinaryExpression ParseBinaryOperation(NodeType oper)
        {
            Expression expression = this.PopOperand();
            Expression expression2 = this.PopOperand();
            BinaryExpression expression3 = new BinaryExpression(expression2, expression, oper) {
                Type = expression2.Type
            };
            if (expression3.Type == null)
            {
                expression3.Type = expression.Type;
            }
            return expression3;
        }

        private Branch ParseBranch(NodeType operatorType, int operandCount, bool shortOffset, bool unordered) => 
            this.ParseBranch(operatorType, operandCount, shortOffset, unordered, false);

        private Branch ParseBranch(NodeType operatorType, int operandCount, bool shortOffset, bool unordered, bool leavesExceptionBlock)
        {
            Expression expression = (operandCount > 1) ? this.PopOperand() : null;
            Expression expression2 = (operandCount > 0) ? this.PopOperand() : null;
            Expression condition = (operandCount > 1) ? new BinaryExpression(expression2, expression, operatorType) : ((operandCount > 0) ? ((operatorType == NodeType.Nop) ? expression2 : new UnaryExpression(expression2, operatorType)) : null);
            int num = shortOffset ? base.GetSByte() : base.GetInt32();
            Block target = (Block) this.blockMap[(num + base.counter) + 1];
            if ((num >= 0) && !base.reader.preserveShortBranches)
            {
                shortOffset = false;
            }
            return new Branch(condition, target, shortOffset, unordered, leavesExceptionBlock);
        }

        private MethodCall ParseCall(NodeType typeOfCall, out bool isStatement)
        {
            TypeNodeList list;
            Method memberFromToken = (Method) base.GetMemberFromToken(out list);
            int num = (list == null) ? 0 : list.Count;
            isStatement = TypeIsVoid(memberFromToken.ReturnType);
            int n = (memberFromToken.Parameters == null) ? 0 : memberFromToken.Parameters.Count;
            if (typeOfCall == NodeType.Jmp)
            {
                n = 0;
            }
            else
            {
                n += num;
            }
            Expression[] expressionArray = new Expression[n];
            ExpressionList arguments = new ExpressionList(n);
            for (int i = n - 1; i >= 0; i--)
            {
                expressionArray[i] = this.PopOperand();
            }
            for (int j = 0; j < n; j++)
            {
                arguments.Add(expressionArray[j]);
            }
            if (list != null)
            {
                int num5 = n - 1;
                int num6 = num;
                while (num6 > 0)
                {
                    Expression expression = arguments[num5];
                    TypeNode node = list[num6 - 1];
                    if ((expression != null) && (node != null))
                    {
                        expression.Type = node;
                    }
                    num6--;
                    num5--;
                }
            }
            Expression targetObject = memberFromToken.IsStatic ? null : this.PopOperand();
            MemberBinding callee = new MemberBinding(targetObject, memberFromToken);
            MethodCall call = new MethodCall(callee, arguments, typeOfCall) {
                Type = memberFromToken.ReturnType,
                IsTailCall = this.isTailCall
            };
            if (this.constraint != null)
            {
                call.Constraint = this.constraint;
                this.constraint = null;
            }
            return call;
        }

        private MethodCall ParseCalli(out bool isStatement)
        {
            FunctionPointer calliSignature = base.reader.GetCalliSignature(base.GetInt32());
            if (calliSignature == null)
            {
                throw new InvalidMetadataException(ExceptionStrings.BaddCalliSignature);
            }
            isStatement = TypeIsVoid(calliSignature.ReturnType);
            int count = calliSignature.ParameterTypes.Count;
            Expression[] expressionArray = new Expression[count + 1];
            ExpressionList arguments = new ExpressionList(count + 1);
            for (int i = count; i >= 0; i--)
            {
                expressionArray[i] = this.PopOperand();
            }
            for (int j = 0; j <= count; j++)
            {
                arguments.Add(expressionArray[j]);
            }
            Expression targetObject = calliSignature.IsStatic ? null : this.PopOperand();
            return new MethodCall(new MemberBinding(targetObject, calliSignature), arguments, NodeType.Calli) { 
                Type = calliSignature.ReturnType,
                IsTailCall = this.isTailCall
            };
        }

        private Construct ParseConstruct()
        {
            TypeNodeList list;
            Method memberFromToken = (Method) base.GetMemberFromToken(out list);
            int count = memberFromToken.Parameters.Count;
            Expression[] expressionArray = new Expression[count];
            ExpressionList arguments = new ExpressionList(count);
            for (int i = count - 1; i >= 0; i--)
            {
                expressionArray[i] = this.PopOperand();
            }
            for (int j = 0; j < count; j++)
            {
                arguments.Add(expressionArray[j]);
            }
            return new Construct(new MemberBinding(null, memberFromToken), arguments) { Type = memberFromToken.DeclaringType };
        }

        private AssignmentStatement ParseCopyObject()
        {
            TypeNode memberFromToken = (TypeNode) base.GetMemberFromToken();
            Expression address = this.PopOperand();
            return new AssignmentStatement(new AddressDereference(this.PopOperand(), memberFromToken, this.isVolatile, this.alignment), new AddressDereference(address, memberFromToken));
        }

        protected override void ParseExceptionHandlerEntry(bool smallSection)
        {
            int @byte = base.reader.tables.GetByte();
            int n = (ushort) base.reader.tables.GetInt16();
            if (smallSection)
            {
                n = @byte / 12;
            }
            else
            {
                n = (@byte + (n << 8)) / 0x18;
            }
            if (n < 0)
            {
                n = 0;
            }
            base.method.ExceptionHandlers = new ExceptionHandlerList(n);
            for (int i = 0; i < n; i++)
            {
                int num4;
                int num5;
                int num6;
                int num7;
                int num8;
                if (smallSection)
                {
                    num4 = base.reader.tables.GetInt16();
                    num5 = base.reader.tables.GetUInt16();
                    num6 = base.reader.tables.GetByte();
                    num7 = base.reader.tables.GetUInt16();
                    num8 = base.reader.tables.GetByte();
                }
                else
                {
                    num4 = base.reader.tables.GetInt32();
                    num5 = base.reader.tables.GetInt32();
                    num6 = base.reader.tables.GetInt32();
                    num7 = base.reader.tables.GetInt32();
                    num8 = base.reader.tables.GetInt32();
                }
                int tok = base.reader.tables.GetInt32();
                ExceptionHandler element = new ExceptionHandler();
                switch (num4)
                {
                    case 0:
                    {
                        element.HandlerType = NodeType.Catch;
                        int currentPosition = base.reader.tables.GetCurrentPosition();
                        element.FilterType = (TypeNode) base.reader.GetMemberFromToken(tok);
                        base.reader.tables.SetCurrentPosition(currentPosition);
                        break;
                    }
                    case 1:
                        element.HandlerType = NodeType.Filter;
                        element.FilterExpression = Reader.GetOrCreateBlock(this.blockMap, tok);
                        break;

                    case 2:
                        element.HandlerType = NodeType.Finally;
                        break;

                    case 4:
                        element.HandlerType = NodeType.FaultHandler;
                        break;

                    default:
                        throw new InvalidMetadataException(ExceptionStrings.BadExceptionHandlerType);
                }
                element.TryStartBlock = Reader.GetOrCreateBlock(this.blockMap, num5);
                element.BlockAfterTryEnd = Reader.GetOrCreateBlock(this.blockMap, num5 + num6);
                element.HandlerStartBlock = Reader.GetOrCreateBlock(this.blockMap, num7);
                element.BlockAfterHandlerEnd = Reader.GetOrCreateBlock(this.blockMap, num7 + num8);
                base.method.ExceptionHandlers.Add(element);
            }
        }

        private AssignmentStatement ParseInitObject()
        {
            TypeNode memberFromToken = (TypeNode) base.GetMemberFromToken();
            return new AssignmentStatement(new AddressDereference(this.PopOperand(), memberFromToken, this.isVolatile, this.alignment), new Literal(null, CoreSystemTypes.Object));
        }

        private UnaryExpression ParseLoadRuntimeMetadataToken()
        {
            Expression operand = null;
            TypeNode type = null;
            Member memberFromToken = base.GetMemberFromToken();
            TypeNode node2 = memberFromToken as TypeNode;
            if (node2 == null)
            {
                type = (memberFromToken.NodeType == NodeType.Field) ? CoreSystemTypes.RuntimeFieldHandle : CoreSystemTypes.RuntimeMethodHandle;
                operand = new MemberBinding(null, memberFromToken);
            }
            else
            {
                type = CoreSystemTypes.RuntimeTypeHandle;
                operand = new Literal(node2, CoreSystemTypes.Type);
            }
            return new UnaryExpression(operand, NodeType.Ldtoken, type);
        }

        private ConstructArray ParseNewArray()
        {
            TypeNode memberFromToken = (TypeNode) base.GetMemberFromToken();
            ExpressionList sizes = new ExpressionList(1);
            sizes.Add(this.PopOperand());
            return new ConstructArray(memberFromToken, sizes, null) { Type = memberFromToken.GetArrayType(1) };
        }

        private bool ParseStatement(Block block)
        {
            bool flag2;
            int num;
            StatementList statements = block.Statements;
            Expression e = null;
            Statement statement = null;
            bool flag = false;
            SourceContext lastSourceContext = new SourceContext();
            if (base.method.contextForOffset != null)
            {
                object obj2 = base.method.contextForOffset[base.counter + 1];
                if (obj2 != null)
                {
                    lastSourceContext = (SourceContext) obj2;
                }
                else
                {
                    lastSourceContext = this.lastSourceContext;
                }
            }
        Label_005C:
            flag2 = false;
            int counter = base.counter;
            this.ilOffset = base.counter;
            this.opCode = base.GetOpCode();
            switch (this.opCode)
            {
                case OpCode.Nop:
                    statement = new Statement(NodeType.Nop);
                    goto Label_1640;

                case OpCode.Break:
                    statement = new Statement(NodeType.DebugBreak);
                    goto Label_1640;

                case OpCode.Ldarg_0:
                    e = base.Parameters(0);
                    break;

                case OpCode.Ldarg_1:
                    e = base.Parameters(1);
                    break;

                case OpCode.Ldarg_2:
                    e = base.Parameters(2);
                    break;

                case OpCode.Ldarg_3:
                    e = base.Parameters(3);
                    break;

                case OpCode.Ldloc_0:
                    e = base.locals[0];
                    break;

                case OpCode.Ldloc_1:
                    e = base.locals[1];
                    break;

                case OpCode.Ldloc_2:
                    e = base.locals[2];
                    break;

                case OpCode.Ldloc_3:
                    e = base.locals[3];
                    break;

                case OpCode.Stloc_0:
                    statement = new AssignmentStatement(base.locals[0], this.PopOperand());
                    goto Label_1640;

                case OpCode.Stloc_1:
                    statement = new AssignmentStatement(base.locals[1], this.PopOperand());
                    goto Label_1640;

                case OpCode.Stloc_2:
                    statement = new AssignmentStatement(base.locals[2], this.PopOperand());
                    goto Label_1640;

                case OpCode.Stloc_3:
                    statement = new AssignmentStatement(base.locals[3], this.PopOperand());
                    goto Label_1640;

                case OpCode.Ldarg_S:
                    e = base.Parameters(base.GetByte());
                    break;

                case OpCode.Ldarga_S:
                    e = SetType(new UnaryExpression(base.Parameters(base.GetByte()), NodeType.AddressOf));
                    break;

                case OpCode.Starg_S:
                    statement = new AssignmentStatement(base.Parameters(base.GetByte()), this.PopOperand());
                    goto Label_1640;

                case OpCode.Ldloc_S:
                    e = base.locals[base.GetByte()];
                    break;

                case OpCode.Ldloca_S:
                    e = SetType(new UnaryExpression(base.locals[base.GetByte()], NodeType.AddressOf));
                    break;

                case OpCode.Stloc_S:
                    statement = new AssignmentStatement(base.locals[base.GetByte()], this.PopOperand());
                    goto Label_1640;

                case OpCode.Ldnull:
                    e = Literal.Null;
                    break;

                case OpCode.Ldc_I4_M1:
                    e = Literal.Int32MinusOne;
                    break;

                case OpCode.Ldc_I4_0:
                    e = Literal.Int32Zero;
                    break;

                case OpCode.Ldc_I4_1:
                    e = Literal.Int32One;
                    break;

                case OpCode.Ldc_I4_2:
                    e = Literal.Int32Two;
                    break;

                case OpCode.Ldc_I4_3:
                    e = new Literal(3, CoreSystemTypes.Int32);
                    break;

                case OpCode.Ldc_I4_4:
                    e = new Literal(4, CoreSystemTypes.Int32);
                    break;

                case OpCode.Ldc_I4_5:
                    e = new Literal(5, CoreSystemTypes.Int32);
                    break;

                case OpCode.Ldc_I4_6:
                    e = new Literal(6, CoreSystemTypes.Int32);
                    break;

                case OpCode.Ldc_I4_7:
                    e = new Literal(7, CoreSystemTypes.Int32);
                    break;

                case OpCode.Ldc_I4_8:
                    e = new Literal(8, CoreSystemTypes.Int32);
                    break;

                case OpCode.Ldc_I4_S:
                    e = new Literal((int) base.GetSByte(), CoreSystemTypes.Int32);
                    break;

                case OpCode.Ldc_I4:
                    e = new Literal(base.GetInt32(), CoreSystemTypes.Int32);
                    break;

                case OpCode.Ldc_I8:
                    e = new Literal(base.GetInt64(), CoreSystemTypes.Int64);
                    break;

                case OpCode.Ldc_R4:
                    e = new Literal(base.GetSingle(), CoreSystemTypes.Single);
                    break;

                case OpCode.Ldc_R8:
                    e = new Literal(base.GetDouble(), CoreSystemTypes.Double);
                    break;

                case OpCode.Dup:
                    statement = new ExpressionStatement(new Expression(NodeType.Dup));
                    goto Label_1640;

                case OpCode.Pop:
                    statement = new ExpressionStatement(new UnaryExpression(this.PopOperand(), NodeType.Pop));
                    goto Label_1640;

                case OpCode.Jmp:
                    e = this.ParseCall(NodeType.Jmp, out flag2);
                    if (!flag2)
                    {
                        break;
                    }
                    goto Label_1640;

                case OpCode.Call:
                    e = this.ParseCall(NodeType.Call, out flag2);
                    if (!flag2)
                    {
                        break;
                    }
                    goto Label_1640;

                case OpCode.Calli:
                    e = this.ParseCalli(out flag2);
                    if (!flag2)
                    {
                        break;
                    }
                    goto Label_1640;

                case OpCode.Ret:
                {
                    Expression expression = TypeIsVoid(base.method.ReturnType) ? null : this.PopOperand();
                    statement = new Return(expression);
                    flag = true;
                    goto Label_1640;
                }
                case OpCode.Br_S:
                    statement = this.ParseBranch(NodeType.Nop, 0, true, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Brfalse_S:
                    statement = this.ParseBranch(NodeType.LogicalNot, 1, true, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Brtrue_S:
                    statement = this.ParseBranch(NodeType.Nop, 1, true, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Beq_S:
                    statement = this.ParseBranch(NodeType.Eq, 2, true, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bge_S:
                    statement = this.ParseBranch(NodeType.Ge, 2, true, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bgt_S:
                    statement = this.ParseBranch(NodeType.Gt, 2, true, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Ble_S:
                    statement = this.ParseBranch(NodeType.Le, 2, true, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Blt_S:
                    statement = this.ParseBranch(NodeType.Lt, 2, true, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bne_Un_S:
                    statement = this.ParseBranch(NodeType.Ne, 2, true, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bge_Un_S:
                    statement = this.ParseBranch(NodeType.Ge, 2, true, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bgt_Un_S:
                    statement = this.ParseBranch(NodeType.Gt, 2, true, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Ble_Un_S:
                    statement = this.ParseBranch(NodeType.Le, 2, true, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Blt_Un_S:
                    statement = this.ParseBranch(NodeType.Lt, 2, true, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Br:
                    statement = this.ParseBranch(NodeType.Nop, 0, false, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Brfalse:
                    statement = this.ParseBranch(NodeType.LogicalNot, 1, false, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Brtrue:
                    statement = this.ParseBranch(NodeType.Nop, 1, false, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Beq:
                    statement = this.ParseBranch(NodeType.Eq, 2, false, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bge:
                    statement = this.ParseBranch(NodeType.Ge, 2, false, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bgt:
                    statement = this.ParseBranch(NodeType.Gt, 2, false, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Ble:
                    statement = this.ParseBranch(NodeType.Le, 2, false, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Blt:
                    statement = this.ParseBranch(NodeType.Lt, 2, false, false);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bne_Un:
                    statement = this.ParseBranch(NodeType.Ne, 2, false, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bge_Un:
                    statement = this.ParseBranch(NodeType.Ge, 2, false, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Bgt_Un:
                    statement = this.ParseBranch(NodeType.Gt, 2, false, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Ble_Un:
                    statement = this.ParseBranch(NodeType.Le, 2, false, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Blt_Un:
                    statement = this.ParseBranch(NodeType.Lt, 2, false, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Switch:
                    statement = this.ParseSwitchInstruction();
                    flag = true;
                    goto Label_1640;

                case OpCode.Ldind_I1:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.Int8, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_U1:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.UInt8, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_I2:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.Int16, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_U2:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.UInt16, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_I4:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.Int32, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_U4:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.UInt32, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_I8:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.Int64, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_I:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.IntPtr, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_R4:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.Single, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_R8:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.Double, this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldind_Ref:
                    e = new AddressDereference(this.PopOperand(), CoreSystemTypes.Object, this.isVolatile, this.alignment);
                    break;

                case OpCode.Stind_Ref:
                    statement = this.ParseStoreIndirect(CoreSystemTypes.Object);
                    goto Label_1640;

                case OpCode.Stind_I1:
                    statement = this.ParseStoreIndirect(CoreSystemTypes.Int8);
                    goto Label_1640;

                case OpCode.Stind_I2:
                    statement = this.ParseStoreIndirect(CoreSystemTypes.Int16);
                    goto Label_1640;

                case OpCode.Stind_I4:
                    statement = this.ParseStoreIndirect(CoreSystemTypes.Int32);
                    goto Label_1640;

                case OpCode.Stind_I8:
                    statement = this.ParseStoreIndirect(CoreSystemTypes.Int64);
                    goto Label_1640;

                case OpCode.Stind_R4:
                    statement = this.ParseStoreIndirect(CoreSystemTypes.Single);
                    goto Label_1640;

                case OpCode.Stind_R8:
                    statement = this.ParseStoreIndirect(CoreSystemTypes.Double);
                    goto Label_1640;

                case OpCode.Add:
                    e = this.ParseBinaryOperation(NodeType.Add);
                    break;

                case OpCode.Sub:
                    e = this.ParseBinaryOperation(NodeType.Sub);
                    break;

                case OpCode.Mul:
                    e = this.ParseBinaryOperation(NodeType.Mul);
                    break;

                case OpCode.Div:
                    e = this.ParseBinaryOperation(NodeType.Div);
                    break;

                case OpCode.Div_Un:
                    e = this.ParseBinaryOperation(NodeType.Div_Un);
                    break;

                case OpCode.Rem:
                    e = this.ParseBinaryOperation(NodeType.Rem);
                    break;

                case OpCode.Rem_Un:
                    e = this.ParseBinaryOperation(NodeType.Rem_Un);
                    break;

                case OpCode.And:
                    e = this.ParseBinaryOperation(NodeType.And);
                    break;

                case OpCode.Or:
                    e = this.ParseBinaryOperation(NodeType.Or);
                    break;

                case OpCode.Xor:
                    e = this.ParseBinaryOperation(NodeType.Xor);
                    break;

                case OpCode.Shl:
                    e = this.ParseBinaryOperation(NodeType.Shl);
                    break;

                case OpCode.Shr:
                    e = this.ParseBinaryOperation(NodeType.Shr);
                    break;

                case OpCode.Shr_Un:
                    e = this.ParseBinaryOperation(NodeType.Shr_Un);
                    break;

                case OpCode.Neg:
                    e = this.ParseUnaryOperation(NodeType.Neg);
                    break;

                case OpCode.Not:
                    e = this.ParseUnaryOperation(NodeType.Not);
                    break;

                case OpCode.Conv_I1:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_I1, CoreSystemTypes.Int8);
                    break;

                case OpCode.Conv_I2:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_I2, CoreSystemTypes.Int16);
                    break;

                case OpCode.Conv_I4:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_I4, CoreSystemTypes.Int32);
                    break;

                case OpCode.Conv_I8:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_I8, CoreSystemTypes.Int64);
                    break;

                case OpCode.Conv_R4:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_R4, CoreSystemTypes.Single);
                    break;

                case OpCode.Conv_R8:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_R8, CoreSystemTypes.Double);
                    break;

                case OpCode.Conv_U4:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_U4, CoreSystemTypes.UInt32);
                    break;

                case OpCode.Conv_U8:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_U8, CoreSystemTypes.UInt64);
                    break;

                case OpCode.Callvirt:
                    e = this.ParseCall(NodeType.Callvirt, out flag2);
                    if (!flag2)
                    {
                        break;
                    }
                    goto Label_1640;

                case OpCode.Cpobj:
                    statement = this.ParseCopyObject();
                    goto Label_1640;

                case OpCode.Ldobj:
                    e = new AddressDereference(this.PopOperand(), (TypeNode) base.GetMemberFromToken(), this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldstr:
                    e = new Literal(base.GetStringFromToken(), CoreSystemTypes.String);
                    break;

                case OpCode.Newobj:
                    e = this.ParseConstruct();
                    break;

                case OpCode.Castclass:
                    e = ParseTypeCheck(this.PopOperand(), (TypeNode) base.GetMemberFromToken(), NodeType.Castclass);
                    break;

                case OpCode.Isinst:
                    e = ParseTypeCheck(this.PopOperand(), (TypeNode) base.GetMemberFromToken(), NodeType.Isinst);
                    break;

                case OpCode.Conv_R_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_R_Un, CoreSystemTypes.Double);
                    break;

                case OpCode.Unbox:
                    e = ParseTypeCheck(this.PopOperand(), (TypeNode) base.GetMemberFromToken(), NodeType.Unbox);
                    break;

                case OpCode.Throw:
                    statement = new Throw(this.PopOperand());
                    flag = true;
                    goto Label_1640;

                case OpCode.Ldfld:
                    e = new MemberBinding(this.PopOperand(), base.GetMemberFromToken(), this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldflda:
                    e = SetType(new UnaryExpression(new MemberBinding(this.PopOperand(), base.GetMemberFromToken(), this.isVolatile, this.alignment), NodeType.AddressOf));
                    break;

                case OpCode.Stfld:
                    statement = this.ParseStoreField();
                    goto Label_1640;

                case OpCode.Ldsfld:
                    e = new MemberBinding(null, base.GetMemberFromToken(), this.isVolatile, this.alignment);
                    break;

                case OpCode.Ldsflda:
                    e = SetType(new UnaryExpression(new MemberBinding(null, base.GetMemberFromToken(), this.isVolatile, this.alignment), NodeType.AddressOf));
                    break;

                case OpCode.Stsfld:
                    statement = new AssignmentStatement(new MemberBinding(null, base.GetMemberFromToken(), this.isVolatile, this.alignment), this.PopOperand());
                    goto Label_1640;

                case OpCode.Stobj:
                    statement = this.ParseStoreIndirect((TypeNode) base.GetMemberFromToken());
                    goto Label_1640;

                case OpCode.Conv_Ovf_I1_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I1_Un, CoreSystemTypes.Int8);
                    break;

                case OpCode.Conv_Ovf_I2_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I2_Un, CoreSystemTypes.Int16);
                    break;

                case OpCode.Conv_Ovf_I4_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I4_Un, CoreSystemTypes.Int32);
                    break;

                case OpCode.Conv_Ovf_I8_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I8_Un, CoreSystemTypes.Int64);
                    break;

                case OpCode.Conv_Ovf_U1_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U1_Un, CoreSystemTypes.UInt8);
                    break;

                case OpCode.Conv_Ovf_U2_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U2_Un, CoreSystemTypes.UInt16);
                    break;

                case OpCode.Conv_Ovf_U4_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U4_Un, CoreSystemTypes.UInt32);
                    break;

                case OpCode.Conv_Ovf_U8_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U8_Un, CoreSystemTypes.UInt64);
                    break;

                case OpCode.Conv_Ovf_I_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I_Un, CoreSystemTypes.IntPtr);
                    break;

                case OpCode.Conv_Ovf_U_Un:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U_Un, CoreSystemTypes.UIntPtr);
                    break;

                case OpCode.Box:
                {
                    TypeNode memberFromToken = (TypeNode) base.GetMemberFromToken();
                    TypeNode resultType = (memberFromToken is EnumNode) ? CoreSystemTypes.Enum : CoreSystemTypes.ValueType;
                    e = new BinaryExpression(this.PopOperand(), new Literal(memberFromToken, CoreSystemTypes.Type), NodeType.Box, resultType);
                    break;
                }
                case OpCode.Newarr:
                    e = this.ParseNewArray();
                    break;

                case OpCode.Ldlen:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Ldlen, CoreSystemTypes.UIntPtr);
                    break;

                case OpCode.Ldelema:
                    e = this.ParseArrayElementLoadAddress();
                    break;

                case OpCode.Ldelem_I1:
                case OpCode.Ldelem_U1:
                case OpCode.Ldelem_I2:
                case OpCode.Ldelem_U2:
                case OpCode.Ldelem_I4:
                case OpCode.Ldelem_U4:
                case OpCode.Ldelem_I8:
                case OpCode.Ldelem_I:
                case OpCode.Ldelem_R4:
                case OpCode.Ldelem_R8:
                case OpCode.Ldelem_Ref:
                    e = this.ParseArrayElementLoad(this.opCode, null);
                    break;

                case OpCode.Stelem_I:
                case OpCode.Stelem_I1:
                case OpCode.Stelem_I2:
                case OpCode.Stelem_I4:
                case OpCode.Stelem_I8:
                case OpCode.Stelem_R4:
                case OpCode.Stelem_R8:
                case OpCode.Stelem_Ref:
                    statement = this.ParseArrayElementAssignment(this.opCode);
                    goto Label_1640;

                case OpCode.Ldelem:
                    e = this.ParseArrayElementLoad(this.opCode, null);
                    break;

                case OpCode.Stelem:
                    statement = this.ParseArrayElementAssignment(this.opCode);
                    goto Label_1640;

                case OpCode.Unbox_Any:
                    e = ParseTypeCheck(this.PopOperand(), (TypeNode) base.GetMemberFromToken(), NodeType.UnboxAny);
                    break;

                case OpCode.Conv_Ovf_I1:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I1, CoreSystemTypes.Int8);
                    break;

                case OpCode.Conv_Ovf_U1:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U1, CoreSystemTypes.UInt8);
                    break;

                case OpCode.Conv_Ovf_I2:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I2, CoreSystemTypes.Int16);
                    break;

                case OpCode.Conv_Ovf_U2:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U2, CoreSystemTypes.UInt16);
                    break;

                case OpCode.Conv_Ovf_I4:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I4, CoreSystemTypes.Int32);
                    break;

                case OpCode.Conv_Ovf_U4:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U4, CoreSystemTypes.UInt32);
                    break;

                case OpCode.Conv_Ovf_I8:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I8, CoreSystemTypes.Int64);
                    break;

                case OpCode.Conv_Ovf_U8:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U8, CoreSystemTypes.UInt64);
                    break;

                case OpCode.Refanyval:
                    e = new BinaryExpression(this.PopOperand(), new Literal(base.GetMemberFromToken(), CoreSystemTypes.Type), NodeType.Refanyval, CoreSystemTypes.IntPtr);
                    break;

                case OpCode.Ckfinite:
                    e = this.ParseUnaryOperation(NodeType.Ckfinite);
                    break;

                case OpCode.Mkrefany:
                    e = new BinaryExpression(this.PopOperand(), new Literal(base.GetMemberFromToken(), CoreSystemTypes.Type), NodeType.Mkrefany, CoreSystemTypes.DynamicallyTypedReference);
                    break;

                case OpCode.Ldtoken:
                    e = this.ParseLoadRuntimeMetadataToken();
                    break;

                case OpCode.Conv_U2:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_U2, CoreSystemTypes.UInt16);
                    break;

                case OpCode.Conv_U1:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_U1, CoreSystemTypes.UInt8);
                    break;

                case OpCode.Conv_I:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_I, CoreSystemTypes.IntPtr);
                    break;

                case OpCode.Conv_Ovf_I:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_I, CoreSystemTypes.IntPtr);
                    break;

                case OpCode.Conv_Ovf_U:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_Ovf_U, CoreSystemTypes.UIntPtr);
                    break;

                case OpCode.Add_Ovf:
                    e = this.ParseBinaryOperation(NodeType.Add_Ovf);
                    break;

                case OpCode.Add_Ovf_Un:
                    e = this.ParseBinaryOperation(NodeType.Add_Ovf_Un);
                    break;

                case OpCode.Mul_Ovf:
                    e = this.ParseBinaryOperation(NodeType.Mul_Ovf);
                    break;

                case OpCode.Mul_Ovf_Un:
                    e = this.ParseBinaryOperation(NodeType.Mul_Ovf_Un);
                    break;

                case OpCode.Sub_Ovf:
                    e = this.ParseBinaryOperation(NodeType.Sub_Ovf);
                    break;

                case OpCode.Sub_Ovf_Un:
                    e = this.ParseBinaryOperation(NodeType.Sub_Ovf_Un);
                    break;

                case OpCode.Endfinally:
                    statement = new EndFinally();
                    flag = true;
                    goto Label_1640;

                case OpCode.Leave:
                    statement = this.ParseBranch(NodeType.Nop, 0, false, false, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Leave_S:
                    statement = this.ParseBranch(NodeType.Nop, 0, true, false, true);
                    flag = true;
                    goto Label_1640;

                case OpCode.Stind_I:
                    statement = this.ParseStoreIndirect(CoreSystemTypes.IntPtr);
                    goto Label_1640;

                case OpCode.Conv_U:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Conv_U, CoreSystemTypes.UIntPtr);
                    break;

                case OpCode.Arglist:
                    e = new Expression(NodeType.Arglist, CoreSystemTypes.ArgIterator);
                    break;

                case OpCode.Ceq:
                    e = this.ParseBinaryComparison(NodeType.Ceq);
                    break;

                case OpCode.Cgt:
                    e = this.ParseBinaryComparison(NodeType.Cgt);
                    break;

                case OpCode.Cgt_Un:
                    e = this.ParseBinaryComparison(NodeType.Cgt_Un);
                    break;

                case OpCode.Clt:
                    e = this.ParseBinaryComparison(NodeType.Clt);
                    break;

                case OpCode.Clt_Un:
                    e = this.ParseBinaryComparison(NodeType.Clt_Un);
                    break;

                case OpCode.Ldftn:
                    e = new UnaryExpression(new MemberBinding(null, base.GetMemberFromToken()), NodeType.Ldftn, CoreSystemTypes.IntPtr);
                    break;

                case OpCode.Ldvirtftn:
                    e = new BinaryExpression(this.PopOperand(), new MemberBinding(null, base.GetMemberFromToken()), NodeType.Ldvirtftn, CoreSystemTypes.IntPtr);
                    break;

                case OpCode.Ldarg:
                    e = base.Parameters((ushort) base.GetInt16());
                    break;

                case OpCode.Ldarga:
                    e = SetType(new UnaryExpression(base.Parameters((ushort) base.GetInt16()), NodeType.AddressOf));
                    break;

                case OpCode.Starg:
                    statement = new AssignmentStatement(base.Parameters((ushort) base.GetInt16()), this.PopOperand());
                    goto Label_1640;

                case OpCode.Ldloc:
                    e = base.locals[(ushort) base.GetInt16()];
                    break;

                case OpCode.Ldloca:
                    e = SetType(new UnaryExpression(base.locals[(ushort) base.GetInt16()], NodeType.AddressOf));
                    break;

                case OpCode.Stloc:
                    statement = new AssignmentStatement(base.locals[(ushort) base.GetInt16()], this.PopOperand());
                    goto Label_1640;

                case OpCode.Localloc:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Localloc, CoreSystemTypes.Void);
                    break;

                case OpCode.Endfilter:
                    statement = new EndFilter(this.PopOperand());
                    flag = true;
                    goto Label_1640;

                case OpCode.Unaligned_:
                    this.alignment = base.GetByte();
                    goto Label_005C;

                case OpCode.Volatile_:
                    this.isVolatile = true;
                    goto Label_005C;

                case OpCode.Tail_:
                    this.isTailCall = true;
                    goto Label_005C;

                case OpCode.Initobj:
                    statement = this.ParseInitObject();
                    goto Label_1640;

                case OpCode.Constrained_:
                    this.constraint = base.GetMemberFromToken() as TypeNode;
                    goto Label_005C;

                case OpCode.Cpblk:
                    e = this.ParseTernaryOperation(NodeType.Cpblk);
                    goto Label_1640;

                case OpCode.Initblk:
                    e = this.ParseTernaryOperation(NodeType.Initblk);
                    goto Label_1640;

                case OpCode.Rethrow:
                    statement = new Throw(null) {
                        NodeType = NodeType.Rethrow
                    };
                    flag = true;
                    goto Label_1640;

                case OpCode.Sizeof:
                    e = new UnaryExpression(new Literal(base.GetMemberFromToken(), CoreSystemTypes.Type), NodeType.Sizeof, CoreSystemTypes.Int32);
                    break;

                case OpCode.Refanytype:
                    e = new UnaryExpression(this.PopOperand(), NodeType.Refanytype, CoreSystemTypes.RuntimeTypeHandle);
                    break;

                case OpCode.Readonly_:
                    this.isReadOnly = true;
                    goto Label_005C;

                default:
                    throw new InvalidMetadataException(ExceptionStrings.UnknownOpCode);
            }
            if (this.blockMap[base.counter + 1] != null)
            {
                flag = true;
            }
            else
            {
                e.ILOffset = this.ilOffset;
                this.operandStack.Push(e);
                this.isReadOnly = false;
                this.isVolatile = false;
                this.isTailCall = false;
                this.alignment = -1;
                goto Label_005C;
            }
        Label_1640:
            num = 0;
            while (num <= this.operandStack.top)
            {
                Expression expression3 = this.operandStack.elements[num];
                Statement statement2 = new ExpressionStatement(expression3) {
                    ILOffset = this.ilOffset,
                    SourceContext = lastSourceContext
                };
                statements.Add(statement2);
                num++;
            }
            this.operandStack.top = -1;
            if (statement == null)
            {
                statement = new ExpressionStatement(e);
                e.ILOffset = this.ilOffset;
            }
            statement.SourceContext = lastSourceContext;
            statement.ILOffset = this.ilOffset;
            this.lastSourceContext = lastSourceContext;
            statements.Add(statement);
            return (flag || (this.blockMap[base.counter + 1] != null));
        }

        internal StatementList ParseStatements()
        {
            base.ParseHeader();
            if (base.size == 0)
            {
                return new StatementList(0);
            }
            this.CreateBlocksForBranchTargets();
            StatementList list = new StatementList();
            Block statement = null;
            while (base.counter < base.size)
            {
                if (statement == null)
                {
                    statement = Reader.GetOrCreateBlock(this.blockMap, base.counter);
                    statement.SourceContext = this.lastSourceContext;
                    list.Add(statement);
                }
                if (this.ParseStatement(statement))
                {
                    statement = null;
                }
            }
            list.Add(Reader.GetOrCreateBlock(this.blockMap, base.counter));
            return list;
        }

        private AssignmentStatement ParseStoreField() => 
            new AssignmentStatement(new MemberBinding(this.PopOperand(), base.GetMemberFromToken(), this.isVolatile, this.alignment), this.PopOperand());

        private AssignmentStatement ParseStoreIndirect(TypeNode type) => 
            new AssignmentStatement(new AddressDereference(this.PopOperand(), type, this.isVolatile, this.alignment), this.PopOperand());

        private SwitchInstruction ParseSwitchInstruction()
        {
            int n = base.GetInt32();
            int num2 = base.counter + (n * 4);
            BlockList targets = new BlockList(n);
            for (int i = 0; i < n; i++)
            {
                int address = base.GetInt32() + num2;
                targets.Add(Reader.GetOrCreateBlock(this.blockMap, address));
            }
            return new SwitchInstruction(this.PopOperand(), targets);
        }

        private TernaryExpression ParseTernaryOperation(NodeType oper)
        {
            Expression expression = this.PopOperand();
            return new TernaryExpression(this.PopOperand(), this.PopOperand(), expression, oper, null);
        }

        private static Expression ParseTypeCheck(Expression operand, TypeNode type, NodeType typeOfCheck)
        {
            TypeNode resultType = type;
            if (typeOfCheck == NodeType.Unbox)
            {
                resultType = type.GetReferenceType();
            }
            return new BinaryExpression(operand, new Literal(type, CoreSystemTypes.Type), typeOfCheck, resultType);
        }

        private UnaryExpression ParseUnaryOperation(NodeType oper)
        {
            Expression operand = this.PopOperand();
            return new UnaryExpression(operand, oper, operand.Type);
        }

        private Expression PopOperand() => 
            this.operandStack.Pop();

        private void ProcessOneILInstruction()
        {
            OpCode opCode = base.GetOpCode();
            if (opCode <= OpCode.Refanyval)
            {
                switch (opCode)
                {
                    case OpCode.Ldelem:
                    case OpCode.Stelem:
                    case OpCode.Unbox_Any:
                    case OpCode.Refanyval:
                    case OpCode.Ldc_I4:
                    case OpCode.Jmp:
                    case OpCode.Call:
                    case OpCode.Calli:
                    case OpCode.Callvirt:
                    case OpCode.Cpobj:
                    case OpCode.Ldobj:
                    case OpCode.Ldstr:
                    case OpCode.Newobj:
                    case OpCode.Castclass:
                    case OpCode.Isinst:
                    case OpCode.Unbox:
                    case OpCode.Ldfld:
                    case OpCode.Ldflda:
                    case OpCode.Stfld:
                    case OpCode.Ldsfld:
                    case OpCode.Ldsflda:
                    case OpCode.Stsfld:
                    case OpCode.Stobj:
                    case OpCode.Box:
                    case OpCode.Newarr:
                    case OpCode.Ldelema:
                        goto Label_0261;

                    case OpCode.Ldarg_S:
                    case OpCode.Ldarga_S:
                    case OpCode.Starg_S:
                    case OpCode.Ldloc_S:
                    case OpCode.Ldloca_S:
                    case OpCode.Stloc_S:
                    case OpCode.Ldc_I4_S:
                        base.GetByte();
                        return;

                    case OpCode.Ldnull:
                    case OpCode.Ldc_I4_M1:
                    case OpCode.Ldc_I4_0:
                    case OpCode.Ldc_I4_1:
                    case OpCode.Ldc_I4_2:
                    case OpCode.Ldc_I4_3:
                    case OpCode.Ldc_I4_4:
                    case OpCode.Ldc_I4_5:
                    case OpCode.Ldc_I4_6:
                    case OpCode.Ldc_I4_7:
                    case OpCode.Ldc_I4_8:
                    case (OpCode.Ldc_I4 | OpCode.Ldarg_2):
                    case OpCode.Dup:
                    case OpCode.Pop:
                    case OpCode.Ret:
                    case OpCode.Conv_R_Un:
                    case (OpCode.Conv_R_Un | OpCode.Break):
                    case (OpCode.Cpobj | OpCode.Ldloc_2):
                    case OpCode.Throw:
                    case OpCode.Conv_Ovf_I1_Un:
                    case OpCode.Conv_Ovf_I2_Un:
                    case OpCode.Conv_Ovf_I4_Un:
                    case OpCode.Conv_Ovf_I8_Un:
                    case OpCode.Conv_Ovf_U1_Un:
                    case OpCode.Conv_Ovf_U2_Un:
                    case OpCode.Conv_Ovf_U4_Un:
                    case OpCode.Conv_Ovf_U8_Un:
                    case OpCode.Conv_Ovf_I_Un:
                    case OpCode.Conv_Ovf_U_Un:
                    case OpCode.Ldlen:
                        return;

                    case OpCode.Ldc_I8:
                        base.GetInt64();
                        return;

                    case OpCode.Ldc_R4:
                        base.GetSingle();
                        return;

                    case OpCode.Ldc_R8:
                        base.GetDouble();
                        return;

                    case OpCode.Br_S:
                    case OpCode.Brfalse_S:
                    case OpCode.Brtrue_S:
                    case OpCode.Beq_S:
                    case OpCode.Bge_S:
                    case OpCode.Bgt_S:
                    case OpCode.Ble_S:
                    case OpCode.Blt_S:
                    case OpCode.Bne_Un_S:
                    case OpCode.Bge_Un_S:
                    case OpCode.Bgt_Un_S:
                    case OpCode.Ble_Un_S:
                    case OpCode.Blt_Un_S:
                        goto Label_0281;

                    case OpCode.Br:
                    case OpCode.Brfalse:
                    case OpCode.Brtrue:
                    case OpCode.Beq:
                    case OpCode.Bge:
                    case OpCode.Bgt:
                    case OpCode.Ble:
                    case OpCode.Blt:
                    case OpCode.Bne_Un:
                    case OpCode.Bge_Un:
                    case OpCode.Bgt_Un:
                    case OpCode.Ble_Un:
                    case OpCode.Blt_Un:
                        goto Label_0289;

                    case OpCode.Switch:
                        this.SkipSwitch();
                        return;
                }
                return;
            }
            if (opCode <= OpCode.Ldtoken)
            {
                if ((opCode != OpCode.Mkrefany) && (opCode != OpCode.Ldtoken))
                {
                    return;
                }
            }
            else
            {
                switch (opCode)
                {
                    case OpCode.Leave:
                        goto Label_0289;

                    case OpCode.Leave_S:
                        goto Label_0281;

                    case OpCode.Ldftn:
                    case OpCode.Ldvirtftn:
                    case OpCode.Initobj:
                    case OpCode.Constrained_:
                    case OpCode.Sizeof:
                        base.GetInt32();
                        return;

                    case (OpCode.Arglist | OpCode.Ldloc_2):
                    case OpCode.Localloc:
                    case (OpCode.Arglist | OpCode.Starg_S):
                    case OpCode.Endfilter:
                    case OpCode.Volatile_:
                    case OpCode.Tail_:
                    case OpCode.Cpblk:
                    case OpCode.Initblk:
                    case (OpCode.Initblk | OpCode.Break):
                    case OpCode.Rethrow:
                    case (OpCode.Rethrow | OpCode.Break):
                        return;

                    case OpCode.Ldarg:
                    case OpCode.Ldarga:
                    case OpCode.Starg:
                    case OpCode.Ldloc:
                    case OpCode.Ldloca:
                    case OpCode.Stloc:
                        base.GetInt16();
                        return;

                    case OpCode.Unaligned_:
                        base.GetByte();
                        return;
                }
                return;
            }
        Label_0261:
            base.GetInt32();
            return;
        Label_0281:
            this.SkipBranch(true);
            return;
        Label_0289:
            this.SkipBranch(false);
        }

        private static UnaryExpression SetType(UnaryExpression uex)
        {
            if ((uex != null) && (uex.Operand != null))
            {
                TypeNode type = uex.Operand.Type;
                if (type != null)
                {
                    uex.Type = type.GetReferenceType();
                }
            }
            return uex;
        }

        private void SkipBranch(bool shortOffset)
        {
            int num = shortOffset ? base.GetSByte() : base.GetInt32();
            Reader.GetOrCreateBlock(this.blockMap, base.counter + num);
        }

        private void SkipSwitch()
        {
            int num = base.GetInt32();
            int num2 = base.counter + (num * 4);
            for (int i = 0; i < num; i++)
            {
                int address = base.GetInt32() + num2;
                Reader.GetOrCreateBlock(this.blockMap, address);
            }
        }

        private static bool TypeIsVoid(TypeNode t)
        {
            if (t == null)
            {
                return false;
            }
            while (true)
            {
                NodeType nodeType = t.NodeType;
                if ((nodeType != NodeType.OptionalModifier) && (nodeType != NodeType.RequiredModifier))
                {
                    return (t == CoreSystemTypes.Void);
                }
                t = ((TypeModifier) t).ModifiedType;
            }
        }
    }
}

