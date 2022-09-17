namespace System.Compiler.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Compiler;
    using System.Runtime.InteropServices;

    internal abstract class ILParser
    {
        protected MemoryCursor bodyReader;
        internal int counter;
        protected LocalList locals = new LocalList();
        protected Method method;
        protected int methodIndex;
        protected Reader reader;
        protected int RVA;
        internal int size;

        internal ILParser(Reader reader, Method method, int methodIndex, int RVA)
        {
            this.reader = reader;
            this.bodyReader = reader.tables.GetNewCursor();
            this.method = method;
            this.method.LocalList = this.locals;
            this.methodIndex = methodIndex;
            this.RVA = RVA;
        }

        protected byte GetByte()
        {
            this.counter++;
            return this.bodyReader.ReadByte();
        }

        protected double GetDouble()
        {
            this.counter += 8;
            return this.bodyReader.ReadDouble();
        }

        protected short GetInt16()
        {
            this.counter += 2;
            return this.bodyReader.ReadInt16();
        }

        protected int GetInt32()
        {
            this.counter += 4;
            return this.bodyReader.ReadInt32();
        }

        protected long GetInt64()
        {
            this.counter += 8;
            return this.bodyReader.ReadInt64();
        }

        protected Member GetMemberFromToken() => 
            this.reader.GetMemberFromToken(this.GetInt32());

        protected Member GetMemberFromToken(out TypeNodeList varArgTypes) => 
            this.reader.GetMemberFromToken(this.GetInt32(), out varArgTypes);

        protected OpCode GetOpCode()
        {
            int @byte = this.GetByte();
            if (@byte == 0xfe)
            {
                @byte = (@byte << 8) | this.GetByte();
            }
            return (OpCode) @byte;
        }

        protected sbyte GetSByte()
        {
            this.counter++;
            return this.bodyReader.ReadSByte();
        }

        protected float GetSingle()
        {
            this.counter += 4;
            return this.bodyReader.ReadSingle();
        }

        protected string GetStringFromToken()
        {
            int num = this.GetInt32();
            return this.reader.tables.GetUserString(num & 0xffffff);
        }

        protected Expression Parameters(int i)
        {
            if (this.method.IsStatic)
            {
                return this.method.Parameters[i];
            }
            if (i == 0)
            {
                return this.method.ThisParameter;
            }
            return this.method.Parameters[i - 1];
        }

        protected abstract void ParseExceptionHandlerEntry(bool smallSection);
        protected void ParseHeader()
        {
            byte methodBodyHeaderByte = this.reader.tables.GetMethodBodyHeaderByte(this.RVA);
            if ((methodBodyHeaderByte & 3) == 2)
            {
                this.size = methodBodyHeaderByte >> 2;
                this.bodyReader = this.reader.tables.GetNewCursor();
                this.reader.tables.Skip(this.size);
            }
            else
            {
                this.method.InitLocals = (methodBodyHeaderByte & 0x10) != 0;
                int num3 = this.reader.tables.GetByte() >> 4;
                if (num3 != 2)
                {
                    if (num3 != 3)
                    {
                        throw new InvalidMetadataException(ExceptionStrings.InvalidFatMethodHeader);
                    }
                    this.reader.tables.Skip(2);
                    this.size = this.reader.tables.GetInt32();
                    int localIndex = this.reader.tables.GetInt32();
                    this.bodyReader = this.reader.tables.GetNewCursor();
                    this.reader.tables.Skip(this.size);
                    this.reader.tables.AlignTo32BitBoundary();
                    while ((methodBodyHeaderByte & 8) != 0)
                    {
                        methodBodyHeaderByte = this.reader.tables.GetByte();
                        if ((methodBodyHeaderByte & 3) != 1)
                        {
                            throw new InvalidMetadataException(ExceptionStrings.BadMethodHeaderSection);
                        }
                        if ((methodBodyHeaderByte & 0x80) != 0)
                        {
                            throw new InvalidMetadataException(ExceptionStrings.TooManyMethodHeaderSections);
                        }
                        this.ParseExceptionHandlerEntry((methodBodyHeaderByte & 0x40) == 0);
                    }
                    Dictionary<int, LocalInfo> localSourceNames = new Dictionary<int, LocalInfo>();
                    if (this.reader.getDebugSymbols && (this.reader.debugReader != null))
                    {
                        ISymUnmanagedMethod method = null;
                        try
                        {
                            this.reader.debugReader.GetMethod((uint) (0x6000000 | this.methodIndex), ref method);
                            if (method != null)
                            {
                                ISymUnmanagedScope rootScope = method.GetRootScope();
                                try
                                {
                                    this.reader.GetLocalSourceNames(rootScope, localSourceNames);
                                }
                                finally
                                {
                                    if (rootScope != null)
                                    {
                                        Marshal.ReleaseComObject(rootScope);
                                    }
                                }
                            }
                        }
                        catch (COMException)
                        {
                        }
                        catch (InvalidCastException)
                        {
                        }
                        catch (InvalidComObjectException)
                        {
                        }
                        finally
                        {
                            if (method != null)
                            {
                                Marshal.ReleaseComObject(method);
                            }
                        }
                    }
                    this.reader.GetLocals(localIndex, this.locals, localSourceNames);
                }
            }
        }
    }
}

