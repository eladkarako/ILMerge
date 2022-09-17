namespace System.Compiler
{
    using System;
    using System.Text;

    internal class FunctionType : DelegateNode
    {
        protected TypeNodeList structuralElementTypes;

        private FunctionType(Identifier name, TypeNode returnType, ParameterList parameters)
        {
            base.Flags = TypeFlags.Sealed | TypeFlags.Public;
            base.Namespace = StandardIds.StructuralTypes;
            base.Name = name;
            base.returnType = returnType;
            base.parameters = parameters;
        }

        public static FunctionType For(TypeNode returnType, ParameterList parameters, TypeNode referringType)
        {
            if ((returnType == null) || (referringType == null))
            {
                return null;
            }
            Module declaringModule = referringType.DeclaringModule;
            if (declaringModule == null)
            {
                return null;
            }
            TypeFlags visibilityIntersection = returnType.Flags & TypeFlags.NestedFamORAssem;
            StringBuilder builder = new StringBuilder();
            builder.Append("Function_");
            builder.Append(returnType.Name.ToString());
            int capacity = (parameters == null) ? 0 : parameters.Count;
            if (parameters != null)
            {
                for (int i = 0; i < capacity; i++)
                {
                    Parameter parameter = parameters[i];
                    if ((parameter != null) && (parameter.Type != null))
                    {
                        visibilityIntersection = TypeNode.GetVisibilityIntersection(visibilityIntersection, parameter.Type.Flags & TypeFlags.NestedFamORAssem);
                        builder.Append('_');
                        builder.Append(parameter.Type.Name.ToString());
                    }
                }
            }
            FunctionType element = null;
            int num3 = 0;
            string name = builder.ToString();
            Identifier id = Identifier.For(name);
            for (TypeNode node = declaringModule.GetStructurallyEquivalentType(StandardIds.StructuralTypes, id); node != null; node = declaringModule.GetStructurallyEquivalentType(StandardIds.StructuralTypes, id))
            {
                element = node as FunctionType;
                bool flag = (element != null) && (element.ReturnType == returnType);
                if (flag)
                {
                    ParameterList list = element.Parameters;
                    int num4 = (list == null) ? 0 : list.Count;
                    flag = capacity == num4;
                    if ((parameters != null) && (list != null))
                    {
                        for (int j = 0; (j < capacity) && flag; j++)
                        {
                            Parameter parameter2 = parameters[j];
                            Parameter parameter3 = list[j];
                            flag = ((parameter2 != null) && (parameter3 != null)) && (parameter2.Type == parameter3.Type);
                        }
                    }
                }
                if (flag)
                {
                    return element;
                }
                id = Identifier.For(name + ++num3.ToString());
            }
            if (parameters != null)
            {
                ParameterList list2 = new ParameterList(capacity);
                for (int k = 0; k < capacity; k++)
                {
                    Parameter parameter4 = parameters[k];
                    if (parameter4 != null)
                    {
                        parameter4 = (Parameter) parameter4.Clone();
                    }
                    list2.Add(parameter4);
                }
                parameters = list2;
            }
            element = new FunctionType(id, returnType, parameters) {
                DeclaringModule = declaringModule
            };
            switch (visibilityIntersection)
            {
                case TypeFlags.NestedPrivate:
                case TypeFlags.NestedFamily:
                case TypeFlags.NestedFamANDAssem:
                    referringType.Members.Add(element);
                    element.DeclaringType = referringType;
                    element.Flags &= ~TypeFlags.NestedFamORAssem;
                    element.Flags |= TypeFlags.NestedPrivate;
                    break;

                default:
                    declaringModule.Types.Add(element);
                    break;
            }
            declaringModule.StructurallyEquivalentType[element.Name.UniqueIdKey] = element;
            element.ProvideMembers();
            return element;
        }

        public override bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if (type == null)
            {
                return false;
            }
            if (this != type)
            {
                FunctionType type2 = type as FunctionType;
                if (type2 == null)
                {
                    return false;
                }
                if (this.Template != null)
                {
                    return base.IsStructurallyEquivalentTo(type2);
                }
                if (base.Flags != type2.Flags)
                {
                    return false;
                }
                if ((this.ReturnType == null) || (type2.ReturnType == null))
                {
                    return false;
                }
                if ((this.ReturnType != type2.ReturnType) && !this.ReturnType.IsStructurallyEquivalentTo(type2.ReturnType))
                {
                    return false;
                }
                if (this.Parameters == null)
                {
                    return (type2.Parameters == null);
                }
                if (type2.Parameters == null)
                {
                    return false;
                }
                int count = this.Parameters.Count;
                if (count != type2.Parameters.Count)
                {
                    return false;
                }
                for (int i = 0; i < count; i++)
                {
                    Parameter parameter = this.Parameters[i];
                    Parameter parameter2 = type2.Parameters[i];
                    if ((parameter == null) || (parameter2 == null))
                    {
                        return false;
                    }
                    if ((parameter.Type == null) || (parameter2.Type == null))
                    {
                        return false;
                    }
                    if ((parameter.Type != parameter2.Type) && !parameter.Type.IsStructurallyEquivalentTo(parameter2.Type))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool IsStructural =>
            true;

        public override TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList structuralElementTypes = this.structuralElementTypes;
                if (structuralElementTypes == null)
                {
                    this.structuralElementTypes = structuralElementTypes = new TypeNodeList();
                    structuralElementTypes.Add(this.ReturnType);
                    ParameterList parameters = this.Parameters;
                    int num = 0;
                    int num2 = (parameters == null) ? 0 : parameters.Count;
                    while (num < num2)
                    {
                        Parameter parameter = parameters[num];
                        if ((parameter != null) && (parameter.Type != null))
                        {
                            structuralElementTypes.Add(parameter.Type);
                        }
                        num++;
                    }
                }
                return structuralElementTypes;
            }
        }
    }
}

