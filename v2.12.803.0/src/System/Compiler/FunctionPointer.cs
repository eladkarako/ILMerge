namespace System.Compiler
{
    using System;
    using System.Compiler.Metadata;
    using System.Text;

    internal class FunctionPointer : TypeNode
    {
        private CallingConventionFlags callingConvention;
        private TypeNodeList parameterTypes;
        private TypeNode returnType;
        protected TypeNodeList structuralElementTypes;
        private int varArgStart;

        internal FunctionPointer(TypeNodeList parameterTypes, TypeNode returnType, Identifier name) : base(NodeType.FunctionPointer)
        {
            base.Name = name;
            base.Namespace = returnType.Namespace;
            this.parameterTypes = parameterTypes;
            this.returnType = returnType;
            base.typeCode = ElementType.FunctionPointer;
            this.varArgStart = 0x7fffffff;
        }

        public static FunctionPointer For(TypeNodeList parameterTypes, TypeNode returnType)
        {
            Module declaringModule = returnType.DeclaringModule;
            if (declaringModule == null)
            {
                declaringModule = new Module();
            }
            StringBuilder builder = new StringBuilder("function pointer ");
            builder.Append(returnType.FullName);
            builder.Append('(');
            int num = 0;
            int num2 = (parameterTypes == null) ? 0 : parameterTypes.Count;
            while (num < num2)
            {
                TypeNode node = parameterTypes[num];
                if (node != null)
                {
                    if (num != 0)
                    {
                        builder.Append(',');
                    }
                    builder.Append(node.FullName);
                }
                num++;
            }
            builder.Append(')');
            Identifier id = Identifier.For(builder.ToString());
            TypeNode structurallyEquivalentType = declaringModule.GetStructurallyEquivalentType(returnType.Namespace, id);
            int num3 = 1;
            while (structurallyEquivalentType != null)
            {
                FunctionPointer pointer = structurallyEquivalentType as FunctionPointer;
                if (((pointer != null) && (pointer.ReturnType == returnType)) && ParameterTypesAreEquivalent(pointer.ParameterTypes, parameterTypes))
                {
                    return pointer;
                }
                id = Identifier.For(id.ToString() + num3++);
                structurallyEquivalentType = declaringModule.GetStructurallyEquivalentType(returnType.Namespace, id);
            }
            FunctionPointer pointer2 = structurallyEquivalentType as FunctionPointer;
            if (pointer2 == null)
            {
                pointer2 = new FunctionPointer(parameterTypes, returnType, id) {
                    DeclaringModule = declaringModule
                };
                declaringModule.StructurallyEquivalentType[id.UniqueIdKey] = pointer2;
            }
            return pointer2;
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if (type == null)
            {
                return false;
            }
            if (this == type)
            {
                return true;
            }
            FunctionPointer pointer = type as FunctionPointer;
            if (pointer == null)
            {
                return false;
            }
            if (((base.Flags != pointer.Flags) || (this.CallingConvention != pointer.CallingConvention)) || (this.VarArgStart != pointer.VarArgStart))
            {
                return false;
            }
            if ((this.ReturnType == null) || (pointer.ReturnType == null))
            {
                return false;
            }
            if ((this.ReturnType != pointer.ReturnType) && !this.ReturnType.IsStructurallyEquivalentTo(pointer.ReturnType))
            {
                return false;
            }
            return this.IsStructurallyEquivalentList(this.ParameterTypes, pointer.ParameterTypes);
        }

        private static bool ParameterTypesAreEquivalent(TypeNodeList list1, TypeNodeList list2)
        {
            if ((list1 == null) || (list2 == null))
            {
                return (list1 == list2);
            }
            int count = list1.Count;
            if (count != list2.Count)
            {
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                if (list1[i] != list2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public CallingConventionFlags CallingConvention
        {
            get => 
                this.callingConvention;
            set
            {
                this.callingConvention = value;
            }
        }

        public override bool IsStatic =>
            ((this.CallingConvention & CallingConventionFlags.HasThis) == CallingConventionFlags.Default);

        public override bool IsStructural =>
            true;

        public TypeNodeList ParameterTypes
        {
            get => 
                this.parameterTypes;
            set
            {
                this.parameterTypes = value;
            }
        }

        public TypeNode ReturnType
        {
            get => 
                this.returnType;
            set
            {
                this.returnType = value;
            }
        }

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList structuralElementTypes = this.structuralElementTypes;
                if (structuralElementTypes == null)
                {
                    this.structuralElementTypes = structuralElementTypes = new TypeNodeList();
                    structuralElementTypes.Add(this.ReturnType);
                    TypeNodeList parameterTypes = this.ParameterTypes;
                    int num = 0;
                    int num2 = (parameterTypes == null) ? 0 : parameterTypes.Count;
                    while (num < num2)
                    {
                        TypeNode element = parameterTypes[num];
                        if (element != null)
                        {
                            structuralElementTypes.Add(element);
                        }
                        num++;
                    }
                }
                return structuralElementTypes;
            }
        }

        public int VarArgStart
        {
            get => 
                this.varArgStart;
            set
            {
                this.varArgStart = value;
            }
        }
    }
}

