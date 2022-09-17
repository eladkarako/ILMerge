namespace System.Compiler
{
    using System;
    using System.Compiler.Metadata;
    using System.Text;

    internal class ArrayType : TypeNode
    {
        private MemberList addressList;
        private MemberList ctorList;
        private TypeNode elementType;
        private MemberList getterList;
        private int[] lowerBounds;
        private int rank;
        private MemberList setterList;
        private int[] sizes;
        protected TypeNodeList structuralElementTypes;

        internal ArrayType() : base(NodeType.ArrayType)
        {
        }

        internal ArrayType(TypeNode elementType, int rank) : this(elementType, rank, new int[0], new int[0])
        {
            if (rank == 1)
            {
                base.typeCode = System.Compiler.Metadata.ElementType.SzArray;
            }
            else
            {
                base.typeCode = System.Compiler.Metadata.ElementType.Array;
            }
        }

        internal ArrayType(TypeNode elementType, int rank, int[] sizes) : this(elementType, rank, sizes, new int[0])
        {
        }

        internal ArrayType(TypeNode elementType, int rank, int[] sizes, int[] lowerBounds) : base(null, null, null, elementType.Flags, null, null, null, null, NodeType.ArrayType)
        {
            this.rank = rank;
            this.elementType = elementType;
            base.DeclaringModule = elementType.DeclaringModule;
            this.lowerBounds = lowerBounds;
            this.sizes = sizes;
            if (rank == 1)
            {
                base.typeCode = System.Compiler.Metadata.ElementType.SzArray;
            }
            else
            {
                base.typeCode = System.Compiler.Metadata.ElementType.Array;
            }
            if ((elementType != null) && (elementType.Name != null))
            {
                StringBuilder builder = new StringBuilder(this.ElementType.Name.ToString());
                builder.Append('[');
                int num = (this.Sizes == null) ? 0 : this.Sizes.Length;
                int num2 = (this.LowerBounds == null) ? 0 : this.LowerBounds.Length;
                int index = 0;
                int num4 = this.Rank;
                while (index < num4)
                {
                    if ((index < num) && (this.Sizes[index] != 0))
                    {
                        if ((index < num2) && (this.LowerBounds[index] != 0))
                        {
                            builder.Append(this.LowerBounds[index]);
                            builder.Append(':');
                        }
                        builder.Append(this.Sizes[index]);
                    }
                    if (index < (num4 - 1))
                    {
                        builder.Append(',');
                    }
                    index++;
                }
                builder.Append(']');
                base.Name = Identifier.For(builder.ToString());
                base.Namespace = elementType.Namespace;
            }
        }

        internal override void AppendDocumentIdMangledName(StringBuilder sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if (this.ElementType != null)
            {
                this.ElementType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
                sb.Append('[');
                int num = (this.Sizes == null) ? 0 : this.Sizes.Length;
                int num2 = (this.LowerBounds == null) ? 0 : this.LowerBounds.Length;
                int index = 0;
                int rank = this.Rank;
                while (index < rank)
                {
                    if ((index < num) && (this.Sizes[index] != 0))
                    {
                        if ((index < num2) && (this.LowerBounds[index] != 0))
                        {
                            sb.Append(this.LowerBounds[index]);
                            sb.Append(':');
                        }
                        sb.Append(this.Sizes[index]);
                    }
                    if (index < (rank - 1))
                    {
                        sb.Append(',');
                    }
                    index++;
                }
                sb.Append(']');
            }
        }

        public virtual int GetLowerBound(int dimension)
        {
            if ((this.LowerBounds != null) && (this.LowerBounds.Length > dimension))
            {
                return this.LowerBounds[dimension];
            }
            return 0;
        }

        public override MemberList GetMembersNamed(Identifier name)
        {
            if (name == null)
            {
                return new MemberList(0);
            }
            if (name.UniqueIdKey == StandardIds.Get.UniqueIdKey)
            {
                if ((this.getterList == null) && (this.Getter != null))
                {
                }
                return this.getterList;
            }
            if (name.UniqueIdKey == StandardIds.Set.UniqueIdKey)
            {
                if ((this.setterList == null) && (this.Setter != null))
                {
                }
                return this.setterList;
            }
            if (name.UniqueIdKey == StandardIds.Ctor.UniqueIdKey)
            {
                if ((this.ctorList == null) && (this.Constructor != null))
                {
                }
                return this.ctorList;
            }
            if (name.UniqueIdKey != StandardIds.Address.UniqueIdKey)
            {
                return new MemberList(0);
            }
            if ((this.addressList == null) && (this.Address != null))
            {
            }
            return this.addressList;
        }

        public override Type GetRuntimeType()
        {
            if (base.runtimeType == null)
            {
                if (this.ElementType == null)
                {
                    return null;
                }
                Type runtimeType = this.ElementType.GetRuntimeType();
                if (runtimeType == null)
                {
                    return null;
                }
                if (this.IsSzArray())
                {
                    base.runtimeType = runtimeType.MakeArrayType();
                }
                else
                {
                    base.runtimeType = runtimeType.MakeArrayType(this.Rank);
                }
            }
            return base.runtimeType;
        }

        public virtual int GetSize(int dimension)
        {
            if ((this.Sizes != null) && (this.Sizes.Length > dimension))
            {
                return this.Sizes[dimension];
            }
            return 0;
        }

        public override bool IsAssignableTo(TypeNode targetType)
        {
            if (targetType == null)
            {
                return false;
            }
            if (((targetType == this) || (targetType == CoreSystemTypes.Object)) || ((targetType == CoreSystemTypes.Array) || (targetType == SystemTypes.ICloneable)))
            {
                return true;
            }
            if (CoreSystemTypes.Array.IsAssignableTo(targetType))
            {
                return true;
            }
            if ((((targetType.Template != null) && (SystemTypes.GenericIEnumerable != null)) && (SystemTypes.GenericIEnumerable.DeclaringModule == CoreSystemTypes.SystemAssembly)) && (((targetType.Template == SystemTypes.GenericIEnumerable) || (targetType.Template == SystemTypes.GenericICollection)) || (targetType.Template == SystemTypes.GenericIList)))
            {
                if ((targetType.TemplateArguments == null) || (targetType.TemplateArguments.Count != 1))
                {
                    return false;
                }
                TypeNode node = targetType.TemplateArguments[0];
                if (this.ElementType == node)
                {
                    return true;
                }
                if (this.ElementType.IsValueType)
                {
                    return false;
                }
                return this.ElementType.IsAssignableTo(node);
            }
            ArrayType type = targetType as ArrayType;
            if (type == null)
            {
                return false;
            }
            if ((this.Rank != 1) || (type.Rank != 1))
            {
                return false;
            }
            TypeNode elementType = this.ElementType;
            if (elementType == null)
            {
                return false;
            }
            if (elementType == type.ElementType)
            {
                return true;
            }
            if (elementType.IsValueType)
            {
                return false;
            }
            return elementType.IsAssignableTo(type.ElementType);
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if (type == null)
            {
                return false;
            }
            if (this != type)
            {
                ArrayType type2 = type as ArrayType;
                if (type2 == null)
                {
                    return false;
                }
                if (this.Rank != type2.Rank)
                {
                    return false;
                }
                if ((this.ElementType == null) || (type2.ElementType == null))
                {
                    return false;
                }
                if ((this.ElementType != type2.ElementType) && !this.ElementType.IsStructurallyEquivalentTo(type2.ElementType))
                {
                    return false;
                }
                if (this.Sizes == null)
                {
                    return (type2.Sizes == null);
                }
                if (type2.Sizes == null)
                {
                    return false;
                }
                int length = this.Sizes.Length;
                if (length != type2.Sizes.Length)
                {
                    return false;
                }
                for (int i = 0; i < length; i++)
                {
                    if (this.Sizes[i] != type2.Sizes[i])
                    {
                        return false;
                    }
                }
                if (this.LowerBounds == null)
                {
                    return (type2.LowerBounds == null);
                }
                if (type2.LowerBounds == null)
                {
                    return false;
                }
                length = this.LowerBounds.Length;
                if (length != type2.LowerBounds.Length)
                {
                    return false;
                }
                for (int j = 0; j < length; j++)
                {
                    if (this.LowerBounds[j] != type2.LowerBounds[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsSzArray() => 
            (base.typeCode == System.Compiler.Metadata.ElementType.SzArray);

        public virtual void SetLowerBoundToUnknown()
        {
            base.typeCode = System.Compiler.Metadata.ElementType.Array;
        }

        public Method Address
        {
            get
            {
                if (this.addressList == null)
                {
                    lock (this)
                    {
                        if (this.addressList == null)
                        {
                            Method element = new Method {
                                Name = StandardIds.Address,
                                DeclaringType = this,
                                CallingConvention = CallingConventionFlags.HasThis,
                                Flags = MethodFlags.Public,
                                Parameters = new ParameterList()
                            };
                            int num = 0;
                            int rank = this.Rank;
                            while (num < rank)
                            {
                                Parameter parameter = new Parameter {
                                    Type = CoreSystemTypes.Int32,
                                    DeclaringMethod = element
                                };
                                element.Parameters.Add(parameter);
                                num++;
                            }
                            element.ReturnType = this.ElementType.GetReferenceType();
                            this.addressList = new MemberList();
                            this.addressList.Add(element);
                        }
                    }
                }
                return (Method) this.addressList[0];
            }
        }

        public Method Constructor
        {
            get
            {
                if (this.ctorList == null)
                {
                    lock (this)
                    {
                        if (this.ctorList == null)
                        {
                            InstanceInitializer element = new InstanceInitializer {
                                DeclaringType = this
                            };
                            element.Flags |= MethodFlags.Public;
                            int rank = this.Rank;
                            element.Parameters = new ParameterList(rank);
                            for (int i = 0; i < rank; i++)
                            {
                                Parameter parameter = new Parameter {
                                    DeclaringMethod = element,
                                    Type = CoreSystemTypes.Int32
                                };
                                element.Parameters.Add(parameter);
                            }
                            this.ctorList = new MemberList(2);
                            this.ctorList.Add(element);
                            element = new InstanceInitializer {
                                DeclaringType = this
                            };
                            element.Flags |= MethodFlags.Public;
                            element.Parameters = new ParameterList(rank *= 2);
                            for (int j = 0; j < rank; j++)
                            {
                                Parameter parameter2 = new Parameter {
                                    Type = CoreSystemTypes.Int32,
                                    DeclaringMethod = element
                                };
                                element.Parameters.Add(parameter2);
                            }
                            this.ctorList.Add(element);
                        }
                    }
                }
                return (Method) this.ctorList[0];
            }
        }

        public TypeNode ElementType
        {
            get => 
                this.elementType;
            set
            {
                this.elementType = value;
            }
        }

        public override string FullName
        {
            get
            {
                if ((this.ElementType != null) && (this.ElementType.DeclaringType != null))
                {
                    return (this.ElementType.DeclaringType.FullName + "+" + ((base.Name == null) ? "" : base.Name.ToString()));
                }
                if ((base.Namespace != null) && (base.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey))
                {
                    return (base.Namespace.ToString() + "." + ((base.Name == null) ? "" : base.Name.ToString()));
                }
                if (base.Name != null)
                {
                    return base.Name.ToString();
                }
                return "";
            }
        }

        public Method Getter
        {
            get
            {
                if (this.getterList == null)
                {
                    lock (this)
                    {
                        if (this.getterList == null)
                        {
                            Method element = new Method {
                                Name = StandardIds.Get,
                                DeclaringType = this,
                                CallingConvention = CallingConventionFlags.HasThis,
                                Flags = MethodFlags.Public,
                                Parameters = new ParameterList()
                            };
                            int num = 0;
                            int rank = this.Rank;
                            while (num < rank)
                            {
                                Parameter parameter = new Parameter {
                                    Type = CoreSystemTypes.Int32,
                                    DeclaringMethod = element
                                };
                                element.Parameters.Add(parameter);
                                num++;
                            }
                            element.ReturnType = this.ElementType;
                            this.getterList = new MemberList();
                            this.getterList.Add(element);
                        }
                    }
                }
                return (Method) this.getterList[0];
            }
        }

        public override InterfaceList Interfaces
        {
            get
            {
                if (base.interfaces == null)
                {
                    InterfaceList list = new InterfaceList(new Interface[] { SystemTypes.ICloneable, SystemTypes.IList, SystemTypes.ICollection, SystemTypes.IEnumerable });
                    if (((this.Rank == 1) && (SystemTypes.GenericIEnumerable != null)) && (SystemTypes.GenericIEnumerable.DeclaringModule == CoreSystemTypes.SystemAssembly))
                    {
                        list.Add((Interface) SystemTypes.GenericIEnumerable.GetTemplateInstance(this, new TypeNode[] { this.elementType }));
                        if (SystemTypes.GenericICollection != null)
                        {
                            list.Add((Interface) SystemTypes.GenericICollection.GetTemplateInstance(this, new TypeNode[] { this.elementType }));
                        }
                        if (SystemTypes.GenericIList != null)
                        {
                            list.Add((Interface) SystemTypes.GenericIList.GetTemplateInstance(this, new TypeNode[] { this.elementType }));
                        }
                    }
                    base.interfaces = list;
                }
                return base.interfaces;
            }
            set
            {
                base.interfaces = value;
            }
        }

        public override bool IsStructural =>
            true;

        public int[] LowerBounds
        {
            get => 
                this.lowerBounds;
            set
            {
                this.lowerBounds = value;
            }
        }

        public override MemberList Members
        {
            get
            {
                if ((base.members == null) || base.membersBeingPopulated)
                {
                    lock (this)
                    {
                        if (base.members == null)
                        {
                            base.membersBeingPopulated = true;
                            MemberList list2 = base.members = new MemberList(5);
                            list2.Add(this.Constructor);
                            list2.Add(this.ctorList[1]);
                            list2.Add(this.Getter);
                            list2.Add(this.Setter);
                            list2.Add(this.Address);
                            base.membersBeingPopulated = false;
                        }
                    }
                }
                return base.members;
            }
            set
            {
                base.members = value;
            }
        }

        public int Rank
        {
            get => 
                this.rank;
            set
            {
                this.rank = value;
            }
        }

        public Method Setter
        {
            get
            {
                if (this.setterList == null)
                {
                    lock (this)
                    {
                        if (this.setterList == null)
                        {
                            Parameter parameter;
                            Method element = new Method {
                                Name = StandardIds.Set,
                                DeclaringType = this,
                                CallingConvention = CallingConventionFlags.HasThis,
                                Flags = MethodFlags.Public,
                                Parameters = new ParameterList()
                            };
                            int num = 0;
                            int rank = this.Rank;
                            while (num < rank)
                            {
                                parameter = new Parameter {
                                    Type = CoreSystemTypes.Int32,
                                    DeclaringMethod = element
                                };
                                element.Parameters.Add(parameter);
                                num++;
                            }
                            parameter = new Parameter {
                                Type = this.ElementType,
                                DeclaringMethod = element
                            };
                            element.Parameters.Add(parameter);
                            element.ReturnType = CoreSystemTypes.Void;
                            this.setterList = new MemberList();
                            this.setterList.Add(element);
                        }
                    }
                }
                return (Method) this.setterList[0];
            }
        }

        public int[] Sizes
        {
            get => 
                this.sizes;
            set
            {
                this.sizes = value;
            }
        }

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList structuralElementTypes = this.structuralElementTypes;
                if (structuralElementTypes == null)
                {
                    this.structuralElementTypes = structuralElementTypes = new TypeNodeList(1);
                    structuralElementTypes.Add(this.ElementType);
                }
                return structuralElementTypes;
            }
        }
    }
}

