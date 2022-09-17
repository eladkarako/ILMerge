namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Globalization;

    internal class InstructionParser : ILParser
    {
        private readonly TrivialHashtable ehMap;
        private SourceContext sourceContext;

        internal InstructionParser(Reader reader, Method method, int methodIndex, int RVA) : base(reader, method, methodIndex, RVA)
        {
            this.sourceContext = new SourceContext();
            this.ehMap = new TrivialHashtable();
        }

        private Instruction AddInstruction(OpCode opCode, int offset) => 
            this.AddInstruction(opCode, offset, null);

        private Instruction AddInstruction(OpCode opCode, int offset, object value)
        {
            Instruction element = new Instruction(opCode, offset, value);
            InstructionList list = (InstructionList) this.ehMap[offset + 1];
            if (list == null)
            {
                this.ehMap[offset + 1] = list = new InstructionList(2);
            }
            list.Add(element);
            if (base.method.contextForOffset != null)
            {
                object obj2 = base.method.contextForOffset[offset + 1];
                if (obj2 != null)
                {
                    element.SourceContext = (SourceContext) obj2;
                }
            }
            return element;
        }

        protected override void ParseExceptionHandlerEntry(bool smallSection)
        {
            TrivialHashtable hashtable = new TrivialHashtable();
            int @byte = base.reader.tables.GetByte();
            int num2 = (ushort) base.reader.tables.GetInt16();
            if (smallSection)
            {
                num2 = @byte / 12;
            }
            else
            {
                num2 = (@byte + (num2 << 8)) / 0x18;
            }
            for (int i = 0; i < num2; i++)
            {
                int num4;
                int num5;
                int num6;
                int num7;
                int num8;
                Instruction instruction;
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
                if (hashtable[num5 + num6] == null)
                {
                    instruction = this.AddInstruction(OpCode._Try, num5);
                    this.AddInstruction(OpCode._EndTry, num5 + num6, instruction);
                    hashtable[num5 + num6] = string.Empty;
                }
                switch (num4)
                {
                    case 0:
                    {
                        int currentPosition = base.reader.tables.GetCurrentPosition();
                        TypeNode memberFromToken = (TypeNode) base.reader.GetMemberFromToken(tok);
                        base.reader.tables.SetCurrentPosition(currentPosition);
                        instruction = this.AddInstruction(OpCode._Catch, num7, memberFromToken);
                        this.AddInstruction(OpCode._EndHandler, num7 + num8, instruction);
                        continue;
                    }
                    case 1:
                    {
                        instruction = this.AddInstruction(OpCode._Filter, tok);
                        this.AddInstruction(OpCode._EndFilter, num7, instruction);
                        instruction = this.AddInstruction(OpCode._Catch, num7);
                        this.AddInstruction(OpCode._EndHandler, num7 + num8, instruction);
                        continue;
                    }
                    case 2:
                    {
                        instruction = this.AddInstruction(OpCode._Finally, num7);
                        this.AddInstruction(OpCode._EndHandler, num7 + num8, instruction);
                        continue;
                    }
                    case 4:
                    {
                        instruction = this.AddInstruction(OpCode._Fault, num7);
                        this.AddInstruction(OpCode._EndHandler, num7 + num8, instruction);
                        continue;
                    }
                }
                throw new InvalidMetadataException(ExceptionStrings.BadExceptionHandlerType);
            }
        }

        internal Instruction ParseInstruction()
        {
            if (base.counter >= base.size)
            {
                return null;
            }
            int counter = base.counter;
            if (base.method.contextForOffset != null)
            {
                object obj2 = base.method.contextForOffset[counter + 1];
                if (obj2 != null)
                {
                    this.sourceContext = (SourceContext) obj2;
                }
            }
            object sByte = null;
            OpCode opCode = base.GetOpCode();
            switch (opCode)
            {
                case OpCode.Nop:
                case OpCode.Break:
                case OpCode.Ldnull:
                case OpCode.Dup:
                case OpCode.Pop:
                case OpCode.Ret:
                case OpCode.Ldind_I1:
                case OpCode.Ldind_U1:
                case OpCode.Ldind_I2:
                case OpCode.Ldind_U2:
                case OpCode.Ldind_I4:
                case OpCode.Ldind_U4:
                case OpCode.Ldind_I8:
                case OpCode.Ldind_I:
                case OpCode.Ldind_R4:
                case OpCode.Ldind_R8:
                case OpCode.Ldind_Ref:
                case OpCode.Stind_Ref:
                case OpCode.Stind_I1:
                case OpCode.Stind_I2:
                case OpCode.Stind_I4:
                case OpCode.Stind_I8:
                case OpCode.Stind_R4:
                case OpCode.Stind_R8:
                case OpCode.Add:
                case OpCode.Sub:
                case OpCode.Mul:
                case OpCode.Div:
                case OpCode.Div_Un:
                case OpCode.Rem:
                case OpCode.Rem_Un:
                case OpCode.And:
                case OpCode.Or:
                case OpCode.Xor:
                case OpCode.Shl:
                case OpCode.Shr:
                case OpCode.Shr_Un:
                case OpCode.Neg:
                case OpCode.Not:
                case OpCode.Conv_I1:
                case OpCode.Conv_I2:
                case OpCode.Conv_I4:
                case OpCode.Conv_I8:
                case OpCode.Conv_R4:
                case OpCode.Conv_R8:
                case OpCode.Conv_U4:
                case OpCode.Conv_U8:
                case OpCode.Conv_R_Un:
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
                case OpCode.Stelem_I:
                case OpCode.Stelem_I1:
                case OpCode.Stelem_I2:
                case OpCode.Stelem_I4:
                case OpCode.Stelem_I8:
                case OpCode.Stelem_R4:
                case OpCode.Stelem_R8:
                case OpCode.Stelem_Ref:
                case OpCode.Conv_Ovf_I1:
                case OpCode.Conv_Ovf_U1:
                case OpCode.Conv_Ovf_I2:
                case OpCode.Conv_Ovf_U2:
                case OpCode.Conv_Ovf_I4:
                case OpCode.Conv_Ovf_U4:
                case OpCode.Conv_Ovf_I8:
                case OpCode.Conv_Ovf_U8:
                case OpCode.Ckfinite:
                case OpCode.Conv_U2:
                case OpCode.Conv_U1:
                case OpCode.Conv_I:
                case OpCode.Conv_Ovf_I:
                case OpCode.Conv_Ovf_U:
                case OpCode.Add_Ovf:
                case OpCode.Add_Ovf_Un:
                case OpCode.Mul_Ovf:
                case OpCode.Mul_Ovf_Un:
                case OpCode.Sub_Ovf:
                case OpCode.Sub_Ovf_Un:
                case OpCode.Endfinally:
                case OpCode.Stind_I:
                case OpCode.Conv_U:
                case OpCode.Prefix7:
                case OpCode.Prefix6:
                case OpCode.Prefix5:
                case OpCode.Prefix4:
                case OpCode.Prefix3:
                case OpCode.Prefix2:
                case OpCode.Prefix1:
                case OpCode.Arglist:
                case OpCode.Ceq:
                case OpCode.Cgt:
                case OpCode.Cgt_Un:
                case OpCode.Clt:
                case OpCode.Clt_Un:
                case OpCode.Localloc:
                case OpCode.Endfilter:
                case OpCode.Volatile_:
                case OpCode.Tail_:
                case OpCode.Cpblk:
                case OpCode.Initblk:
                case OpCode.Rethrow:
                case OpCode.Refanytype:
                case OpCode.Readonly_:
                    break;

                case OpCode.Ldarg_0:
                    sByte = base.Parameters(0);
                    break;

                case OpCode.Ldarg_1:
                    sByte = base.Parameters(1);
                    break;

                case OpCode.Ldarg_2:
                    sByte = base.Parameters(2);
                    break;

                case OpCode.Ldarg_3:
                    sByte = base.Parameters(3);
                    break;

                case OpCode.Ldloc_0:
                    sByte = base.locals[0];
                    break;

                case OpCode.Ldloc_1:
                    sByte = base.locals[1];
                    break;

                case OpCode.Ldloc_2:
                    sByte = base.locals[2];
                    break;

                case OpCode.Ldloc_3:
                    sByte = base.locals[3];
                    break;

                case OpCode.Stloc_0:
                    sByte = base.locals[0];
                    break;

                case OpCode.Stloc_1:
                    sByte = base.locals[1];
                    break;

                case OpCode.Stloc_2:
                    sByte = base.locals[2];
                    break;

                case OpCode.Stloc_3:
                    sByte = base.locals[3];
                    break;

                case OpCode.Ldarg_S:
                case OpCode.Ldarga_S:
                case OpCode.Starg_S:
                    sByte = base.Parameters(base.GetByte());
                    break;

                case OpCode.Ldloc_S:
                case OpCode.Ldloca_S:
                case OpCode.Stloc_S:
                    sByte = base.locals[base.GetByte()];
                    break;

                case OpCode.Ldc_I4_M1:
                    sByte = -1;
                    break;

                case OpCode.Ldc_I4_0:
                    sByte = 0;
                    break;

                case OpCode.Ldc_I4_1:
                    sByte = 1;
                    break;

                case OpCode.Ldc_I4_2:
                    sByte = 2;
                    break;

                case OpCode.Ldc_I4_3:
                    sByte = 3;
                    break;

                case OpCode.Ldc_I4_4:
                    sByte = 4;
                    break;

                case OpCode.Ldc_I4_5:
                    sByte = 5;
                    break;

                case OpCode.Ldc_I4_6:
                    sByte = 6;
                    break;

                case OpCode.Ldc_I4_7:
                    sByte = 7;
                    break;

                case OpCode.Ldc_I4_8:
                    sByte = 8;
                    break;

                case OpCode.Ldc_I4_S:
                    sByte = base.GetSByte();
                    break;

                case OpCode.Ldc_I4:
                    sByte = base.GetInt32();
                    break;

                case OpCode.Ldc_I8:
                    sByte = base.GetInt64();
                    break;

                case OpCode.Ldc_R4:
                    sByte = base.GetSingle();
                    break;

                case OpCode.Ldc_R8:
                    sByte = base.GetDouble();
                    break;

                case OpCode.Jmp:
                case OpCode.Call:
                    sByte = (Method) base.GetMemberFromToken();
                    break;

                case OpCode.Calli:
                    sByte = base.reader.GetCalliSignature(base.GetInt32());
                    break;

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
                    sByte = (base.counter + 1) + base.GetSByte();
                    break;

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
                    sByte = (base.counter + 4) + base.GetInt32();
                    break;

                case OpCode.Switch:
                    sByte = this.ParseSwitchInstruction();
                    break;

                case OpCode.Callvirt:
                    sByte = (Method) base.GetMemberFromToken();
                    break;

                case OpCode.Cpobj:
                case OpCode.Ldobj:
                    sByte = (TypeNode) base.GetMemberFromToken();
                    break;

                case OpCode.Ldstr:
                    sByte = base.GetStringFromToken();
                    break;

                case OpCode.Newobj:
                    sByte = (Method) base.GetMemberFromToken();
                    break;

                case OpCode.Castclass:
                case OpCode.Isinst:
                    sByte = (TypeNode) base.GetMemberFromToken();
                    break;

                case OpCode.Unbox:
                    sByte = (TypeNode) base.GetMemberFromToken();
                    break;

                case OpCode.Ldfld:
                case OpCode.Ldflda:
                case OpCode.Stfld:
                case OpCode.Ldsfld:
                case OpCode.Ldsflda:
                case OpCode.Stsfld:
                case OpCode.Stobj:
                    sByte = base.GetMemberFromToken();
                    break;

                case OpCode.Box:
                case OpCode.Newarr:
                    sByte = (TypeNode) base.GetMemberFromToken();
                    break;

                case OpCode.Ldelema:
                    sByte = (TypeNode) base.GetMemberFromToken();
                    break;

                case OpCode.Ldelem:
                    sByte = (TypeNode) base.GetMemberFromToken();
                    break;

                case OpCode.Stelem:
                    sByte = (TypeNode) base.GetMemberFromToken();
                    break;

                case OpCode.Unbox_Any:
                    sByte = base.GetMemberFromToken();
                    break;

                case OpCode.Refanyval:
                    sByte = base.GetMemberFromToken();
                    break;

                case OpCode.Mkrefany:
                    sByte = base.GetMemberFromToken();
                    break;

                case OpCode.Ldtoken:
                    sByte = base.GetMemberFromToken();
                    break;

                case OpCode.Leave:
                    sByte = (base.counter + 4) + base.GetInt32();
                    break;

                case OpCode.Leave_S:
                    sByte = (base.counter + 1) + base.GetSByte();
                    break;

                case OpCode.Ldftn:
                case OpCode.Ldvirtftn:
                    sByte = base.GetMemberFromToken();
                    break;

                case OpCode.Ldarg:
                case OpCode.Ldarga:
                case OpCode.Starg:
                    sByte = base.Parameters(base.GetInt16());
                    break;

                case OpCode.Ldloc:
                case OpCode.Ldloca:
                case OpCode.Stloc:
                    sByte = base.locals[base.GetInt16()];
                    break;

                case OpCode.Unaligned_:
                    sByte = base.GetByte();
                    break;

                case OpCode.Initobj:
                    sByte = (TypeNode) base.GetMemberFromToken();
                    break;

                case OpCode.Constrained_:
                    sByte = base.GetMemberFromToken() as TypeNode;
                    break;

                case OpCode.Sizeof:
                    sByte = base.GetMemberFromToken();
                    break;

                default:
                    throw new InvalidMetadataException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.UnknownOpCodeEncountered, new object[] { opCode.ToString("x") }));
            }
            return new Instruction(opCode, counter, sByte) { SourceContext = this.sourceContext };
        }

        internal InstructionList ParseInstructions()
        {
            base.ParseHeader();
            if (base.size == 0)
            {
                return new InstructionList(0);
            }
            InstructionList list = new InstructionList();
            list.Add(new Instruction(OpCode._Locals, 0, base.locals));
            while (base.counter <= base.size)
            {
                InstructionList list2 = (InstructionList) this.ehMap[base.counter + 1];
                if (list2 != null)
                {
                    for (int i = 0; i < list2.Count; i++)
                    {
                        list.Add(list2[i]);
                    }
                }
                if (base.counter >= base.size)
                {
                    return list;
                }
                list.Add(this.ParseInstruction());
            }
            return list;
        }

        private Int32List ParseSwitchInstruction()
        {
            int capacity = base.GetInt32();
            Int32List list = new Int32List(capacity);
            int num2 = base.counter + (capacity * 4);
            for (int i = 0; i < capacity; i++)
            {
                int element = base.GetInt32() + num2;
                list.Add(element);
            }
            return list;
        }
    }
}

