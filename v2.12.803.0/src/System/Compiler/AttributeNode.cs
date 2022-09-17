namespace System.Compiler
{
    using System;
    using System.Compiler.Metadata;
    using System.Reflection;

    internal class AttributeNode : Node
    {
        private bool allowMultiple;
        private Expression constructor;
        public static readonly AttributeNode DoesNotExist = new AttributeNode();
        private ExpressionList expressions;
        private bool inherited;
        public bool IsPseudoAttribute;
        private AttributeTargets target;
        private TypeNode type;
        private AttributeNode usageAttribute;
        private AttributeTargets validOn;

        public AttributeNode() : base(NodeType.Attribute)
        {
        }

        public AttributeNode(Expression constructor, ExpressionList expressions) : base(NodeType.Attribute)
        {
            this.constructor = constructor;
            this.expressions = expressions;
            this.target = AttributeTargets.All;
        }

        public AttributeNode(Expression constructor, ExpressionList expressions, AttributeTargets target) : base(NodeType.Attribute)
        {
            this.constructor = constructor;
            this.expressions = expressions;
            this.target = target;
        }

        private static Attribute ConstructAttribute(InstanceInitializer constr, object[] argumentValues)
        {
            ConstructorInfo constructorInfo = constr.GetConstructorInfo();
            if (constructorInfo != null)
            {
                try
                {
                    return (constructorInfo.Invoke(argumentValues) as Attribute);
                }
                catch
                {
                }
            }
            return null;
        }

        protected Array GetCoercedArrayLiteral(ArrayType arrayType, Array arrayValue)
        {
            if (arrayType == null)
            {
                return null;
            }
            if (arrayValue == null)
            {
                return null;
            }
            if (arrayValue.Rank != 1)
            {
                return null;
            }
            TypeNode elementType = arrayType.ElementType;
            if ((elementType.typeCode != ElementType.ValueType) && (elementType.typeCode != ElementType.Class))
            {
                return arrayValue;
            }
            int length = arrayValue.GetLength(0);
            System.Type runtimeType = elementType.GetRuntimeType();
            if (runtimeType == null)
            {
                return null;
            }
            Array array = Array.CreateInstance(runtimeType, length);
            for (int i = 0; i < length; i++)
            {
                array.SetValue(this.GetCoercedLiteralValue(elementType, arrayValue.GetValue(i)), i);
            }
            return array;
        }

        protected object GetCoercedLiteralValue(TypeNode type, object value)
        {
            if ((type == null) || (value == null))
            {
                return null;
            }
            switch (type.typeCode)
            {
                case ElementType.ValueType:
                    return Enum.ToObject(type.GetRuntimeType(), value);

                case ElementType.Class:
                    return ((TypeNode) value).GetRuntimeType();

                case ElementType.SzArray:
                    return this.GetCoercedArrayLiteral((ArrayType) type, (Array) value);
            }
            Literal literal = value as Literal;
            if (((literal != null) && (type == CoreSystemTypes.Object)) && (literal.Type is EnumNode))
            {
                return this.GetCoercedLiteralValue(literal.Type, literal.Value);
            }
            return value;
        }

        public Expression GetNamedArgument(Identifier name)
        {
            if ((name != null) && (this.Expressions != null))
            {
                ExpressionList.Enumerator enumerator = this.Expressions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Expression current = enumerator.Current;
                    NamedArgument argument = current as NamedArgument;
                    if (((argument != null) && (argument.Name != null)) && (argument.Name.UniqueIdKey == name.UniqueIdKey))
                    {
                        return argument.Value;
                    }
                }
            }
            return null;
        }

        public Expression GetPositionalArgument(int position)
        {
            if ((this.Expressions == null) || (this.Expressions.Count <= position))
            {
                return null;
            }
            Expression expression = this.Expressions[position];
            if (expression is NamedArgument)
            {
                return null;
            }
            return expression;
        }

        public virtual Attribute GetRuntimeAttribute()
        {
            MemberBinding constructor = this.Constructor as MemberBinding;
            if (constructor == null)
            {
                return null;
            }
            InstanceInitializer boundMember = constructor.BoundMember as InstanceInitializer;
            if (boundMember == null)
            {
                return null;
            }
            ParameterList parameters = boundMember.Parameters;
            int num = (parameters == null) ? 0 : parameters.Count;
            object[] argumentValues = new object[num];
            ExpressionList expressions = this.Expressions;
            int num2 = (expressions == null) ? 0 : expressions.Count;
            int index = 0;
            int num4 = 0;
            while (index < num)
            {
                if (num4 >= num2)
                {
                    return null;
                }
                Expression expression = expressions[num4++];
                Literal literal = expression as Literal;
                if (literal != null)
                {
                    argumentValues[index] = this.GetCoercedLiteralValue(literal.Type, literal.Value);
                }
                index++;
            }
            Attribute attr = ConstructAttribute(boundMember, argumentValues);
            if (attr == null)
            {
                return null;
            }
            for (int i = 0; i < num2; i++)
            {
                NamedArgument argument = expressions[i] as NamedArgument;
                if ((argument != null) && (argument.Name != null))
                {
                    Literal literal2 = argument.Value as Literal;
                    if (literal2 != null)
                    {
                        object coercedLiteralValue = this.GetCoercedLiteralValue(literal2.Type, literal2.Value);
                        if (argument.IsCustomAttributeProperty)
                        {
                            TypeNode declaringType = boundMember.DeclaringType;
                            while (declaringType != null)
                            {
                                Property prop = declaringType.GetProperty(argument.Name, new TypeNode[0]);
                                if (prop != null)
                                {
                                    SetAttributeProperty(prop, attr, coercedLiteralValue);
                                    declaringType = null;
                                }
                                else
                                {
                                    declaringType = declaringType.BaseType;
                                }
                            }
                        }
                        else
                        {
                            TypeNode baseType = boundMember.DeclaringType;
                            while (baseType != null)
                            {
                                Field field = boundMember.DeclaringType.GetField(argument.Name);
                                if (field != null)
                                {
                                    FieldInfo fieldInfo = field.GetFieldInfo();
                                    if (fieldInfo != null)
                                    {
                                        fieldInfo.SetValue(attr, coercedLiteralValue);
                                    }
                                    baseType = null;
                                }
                                else
                                {
                                    baseType = baseType.BaseType;
                                }
                            }
                        }
                    }
                }
            }
            return attr;
        }

        private void GetUsageInformation()
        {
            AttributeNode attribute = null;
            TypeNode type = this.Type;
            while (type != null)
            {
                attribute = type.GetAttribute(SystemTypes.AttributeUsageAttribute);
                if (attribute != null)
                {
                    break;
                }
                type = type.BaseType;
            }
            if (attribute == null)
            {
                this.usageAttribute = DoesNotExist;
            }
            else
            {
                ExpressionList expressions = attribute.Expressions;
                if ((expressions != null) && (expressions.Count >= 1))
                {
                    Literal literal = expressions[0] as Literal;
                    if ((literal != null) && (literal.Value is int))
                    {
                        this.validOn = (AttributeTargets) ((int) literal.Value);
                        int num = 1;
                        int count = expressions.Count;
                        while (num < count)
                        {
                            NamedArgument argument = expressions[num] as NamedArgument;
                            if ((argument != null) && (argument.Name != null))
                            {
                                literal = argument.Value as Literal;
                                if (literal != null)
                                {
                                    if (argument.Name.UniqueIdKey == StandardIds.AllowMultiple.UniqueIdKey)
                                    {
                                        if ((literal.Value != null) && (literal.Value is bool))
                                        {
                                            this.allowMultiple = (bool) literal.Value;
                                        }
                                    }
                                    else if (((argument.Name.UniqueIdKey == StandardIds.Inherited.UniqueIdKey) && (literal.Value != null)) && (literal.Value is bool))
                                    {
                                        this.inherited = (bool) literal.Value;
                                    }
                                }
                            }
                            num++;
                        }
                        if (!this.allowMultiple)
                        {
                            int num3 = type.Attributes.Count;
                            for (int i = 0; i < num3; i++)
                            {
                                AttributeNode node3 = type.Attributes[i];
                                if ((((node3 != null) && (node3.Type != null)) && ((node3.Type.Name != null) && (node3.Type.Name.UniqueIdKey == StandardIds.AllowMultipleAttribute.UniqueIdKey))) && ((node3.Type.Namespace != null) && (node3.Type.Namespace.UniqueIdKey == StandardIds.WindowsFoundation.UniqueIdKey)))
                                {
                                    this.allowMultiple = true;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void SetAttributeProperty(Property prop, Attribute attr, object val)
        {
            PropertyInfo propertyInfo = prop.GetPropertyInfo();
            if (propertyInfo != null)
            {
                try
                {
                    propertyInfo.SetValue(attr, val, null);
                }
                catch
                {
                }
            }
        }

        public virtual bool AllowMultiple
        {
            get
            {
                if (this.usageAttribute == null)
                {
                    this.GetUsageInformation();
                }
                return this.allowMultiple;
            }
            set
            {
                this.allowMultiple = value;
            }
        }

        public Expression Constructor
        {
            get => 
                this.constructor;
            set
            {
                this.constructor = value;
            }
        }

        public ExpressionList Expressions
        {
            get => 
                this.expressions;
            set
            {
                this.expressions = value;
            }
        }

        public virtual bool Inherited
        {
            get
            {
                if (this.usageAttribute == null)
                {
                    this.GetUsageInformation();
                }
                return this.inherited;
            }
            set
            {
                this.inherited = value;
            }
        }

        public AttributeTargets Target
        {
            get => 
                this.target;
            set
            {
                this.target = value;
            }
        }

        public virtual TypeNode Type
        {
            get
            {
                if (this.type == null)
                {
                    MemberBinding constructor = this.Constructor as MemberBinding;
                    Member boundMember = constructor?.BoundMember;
                    this.type = boundMember?.DeclaringType;
                }
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public virtual AttributeTargets ValidOn
        {
            get
            {
                if (this.usageAttribute == null)
                {
                    this.GetUsageInformation();
                }
                return this.validOn;
            }
            set
            {
                this.validOn = value;
            }
        }
    }
}

