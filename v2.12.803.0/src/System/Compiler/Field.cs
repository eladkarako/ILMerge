namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Text;

    internal class Field : Member
    {
        private Literal defaultValue;
        protected FieldInfo fieldInfo;
        private FieldFlags flags;
        public Event ForEvent;
        protected string fullName;
        public bool HasOutOfBandContract;
        public InterfaceList ImplementedInterfaceExpressions;
        public InterfaceList ImplementedInterfaces;
        private byte[] initialData;
        public Expression Initializer;
        public bool IsModelfield;
        private bool isVolatile;
        private System.Compiler.MarshallingInformation marshallingInformation;
        private int offset;
        internal PESection section;
        private TypeNode type;
        public TypeNode TypeExpression;

        public Field() : base(NodeType.Field)
        {
        }

        public Field(Identifier name) : base(NodeType.Field)
        {
            base.Name = name;
        }

        public Field(TypeNode declaringType, AttributeList attributes, FieldFlags flags, Identifier name, TypeNode type, Literal defaultValue) : base(declaringType, attributes, name, NodeType.Field)
        {
            this.defaultValue = defaultValue;
            this.flags = flags;
            this.type = type;
        }

        protected override Identifier GetDocumentationId()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("F:");
            if (this.DeclaringType == null)
            {
                return Identifier.Empty;
            }
            builder.Append(this.DeclaringType.FullName);
            builder.Append(".");
            if (base.Name == null)
            {
                return Identifier.Empty;
            }
            builder.Append(base.Name.Name);
            return Identifier.For(builder.ToString());
        }

        public static Field GetField(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                return null;
            }
            return TypeNode.GetTypeNode(fieldInfo.DeclaringType)?.GetField(Identifier.For(fieldInfo.Name));
        }

        public virtual FieldInfo GetFieldInfo()
        {
            if (this.fieldInfo == null)
            {
                TypeNode declaringType = this.DeclaringType;
                if (declaringType == null)
                {
                    return null;
                }
                System.Type runtimeType = declaringType.GetRuntimeType();
                if (runtimeType == null)
                {
                    return null;
                }
                BindingFlags declaredOnly = BindingFlags.DeclaredOnly;
                if (this.IsPublic)
                {
                    declaredOnly |= BindingFlags.Public;
                }
                else
                {
                    declaredOnly |= BindingFlags.NonPublic;
                }
                if (this.IsStatic)
                {
                    declaredOnly |= BindingFlags.Static;
                }
                else
                {
                    declaredOnly |= BindingFlags.Instance;
                }
                this.fieldInfo = runtimeType.GetField(base.Name.ToString(), declaredOnly);
            }
            return this.fieldInfo;
        }

        public virtual Literal GetValue(Literal targetObject) => 
            new Literal(this.GetValue(targetObject.Value));

        public virtual object GetValue(object targetObject) => 
            this.GetFieldInfo()?.GetValue(targetObject);

        public virtual void SetValue(Literal targetObject, Literal value)
        {
            this.SetValue(targetObject.Value, value.Value);
        }

        public virtual void SetValue(object targetObject, object value)
        {
            FieldInfo fieldInfo = this.GetFieldInfo();
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(targetObject, value);
            }
        }

        public override string ToString() => 
            (this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "." + base.Name);

        public Literal DefaultValue
        {
            get => 
                this.defaultValue;
            set
            {
                this.defaultValue = value;
            }
        }

        public FieldFlags Flags
        {
            get => 
                this.flags;
            set
            {
                this.flags = value;
            }
        }

        public override string FullName
        {
            get
            {
                string fullName = this.fullName;
                if (fullName == null)
                {
                    this.fullName = fullName = this.DeclaringType.FullName + "." + ((base.Name == null) ? "" : base.Name.ToString());
                }
                return fullName;
            }
        }

        public byte[] InitialData
        {
            get => 
                this.initialData;
            set
            {
                this.initialData = value;
            }
        }

        public override bool IsAssembly =>
            ((this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.Assembly);

        public override bool IsCompilerControlled =>
            ((this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.CompilerControlled);

        public override bool IsFamily =>
            ((this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.Family);

        public override bool IsFamilyAndAssembly =>
            ((this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.FamANDAssem);

        public override bool IsFamilyOrAssembly =>
            ((this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.FamORAssem);

        public virtual bool IsInitOnly =>
            ((this.Flags & FieldFlags.InitOnly) != FieldFlags.CompilerControlled);

        public virtual bool IsLiteral =>
            ((this.Flags & FieldFlags.Literal) != FieldFlags.CompilerControlled);

        public override bool IsPrivate =>
            ((this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.Private);

        public override bool IsPublic =>
            ((this.Flags & FieldFlags.FieldAccessMask) == FieldFlags.Public);

        public override bool IsSpecialName =>
            ((this.Flags & FieldFlags.SpecialName) != FieldFlags.CompilerControlled);

        public override bool IsStatic =>
            ((this.Flags & FieldFlags.Static) != FieldFlags.CompilerControlled);

        public override bool IsVisibleOutsideAssembly
        {
            get
            {
                if ((this.DeclaringType == null) || this.DeclaringType.IsVisibleOutsideAssembly)
                {
                    switch ((this.Flags & FieldFlags.FieldAccessMask))
                    {
                        case FieldFlags.Family:
                        case FieldFlags.FamORAssem:
                            if (this.DeclaringType == null)
                            {
                                return false;
                            }
                            return !this.DeclaringType.IsSealed;

                        case FieldFlags.Public:
                            return true;
                    }
                }
                return false;
            }
        }

        public bool IsVolatile
        {
            get => 
                this.isVolatile;
            set
            {
                this.isVolatile = value;
            }
        }

        public System.Compiler.MarshallingInformation MarshallingInformation
        {
            get => 
                this.marshallingInformation;
            set
            {
                this.marshallingInformation = value;
            }
        }

        public int Offset
        {
            get => 
                this.offset;
            set
            {
                this.offset = value;
            }
        }

        public PESection Section
        {
            get => 
                this.section;
            set
            {
                this.section = value;
            }
        }

        public TypeNode Type
        {
            get => 
                this.type;
            set
            {
                this.type = value;
            }
        }
    }
}

