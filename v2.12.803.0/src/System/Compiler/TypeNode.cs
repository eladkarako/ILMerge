namespace System.Compiler
{
    using System;
    using System.Collections;
    using System.Compiler.Metadata;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal abstract class TypeNode : Member, IEquatable<TypeNode>
    {
        protected internal TrivialHashtable arrayTypes;
        protected static System.Compiler.Module cachingModuleForGenericInstances = new CachingModuleForGenericsInstances();
        private int classSize;
        internal TypeNodeList consolidatedTemplateArguments;
        protected internal TypeNodeList consolidatedTemplateParameters;
        private Identifier constructorName;
        protected internal MemberList constructors;
        private TypeContract contract;
        private System.Compiler.Module declaringModule;
        protected internal MemberList defaultMembers;
        protected internal TrivialHashtable explicitCoercionFromTable;
        protected internal MemberList explicitCoercionMethods;
        protected internal TrivialHashtable explicitCoercionToTable;
        private TrivialHashtable explicitInterfaceImplementations;
        private TypeNodeList extensions;
        private bool extensionsExamined;
        private TypeFlags flags;
        protected string fullName;
        protected internal TrivialHashtable implicitCoercionFromTable;
        protected internal MemberList implicitCoercionMethods;
        protected internal TrivialHashtable implicitCoercionToTable;
        public InterfaceList InterfaceExpressions;
        protected InterfaceList interfaces;
        private bool isCheckingInheritedFrom;
        public TypeNodeList IsDefinedBy;
        protected bool isGeneric;
        protected bool isNormalized;
        public bool IsNotFullySpecialized;
        protected static readonly char[] MangleChars = new char[] { '!', '>' };
        protected internal int memberCount;
        protected internal MemberList members;
        protected internal volatile bool membersBeingPopulated;
        protected internal TrivialHashtable memberTable;
        protected static readonly System.Compiler.Method MethodDoesNotExist = new System.Compiler.Method();
        internal TrivialHashtable modifierTable;
        private Identifier @namespace;
        protected internal TypeNodeList nestedTypes;
        public bool NewTemplateInstanceIsRecursive;
        private static readonly TypeNode NotSpecified = new Class();
        protected System.Compiler.Method opFalse;
        protected System.Compiler.Method opTrue;
        private int packingSize;
        public TypeNode PartiallyDefines;
        protected internal System.Compiler.Pointer pointerType;
        public NestedTypeProvider ProvideNestedTypes;
        public object ProviderHandle;
        public TypeAttributeProvider ProvideTypeAttributes;
        public TypeMemberProvider ProvideTypeMembers;
        public TypeSignatureProvider ProvideTypeSignature;
        protected internal Reference referenceType;
        protected internal Type runtimeType;
        protected SecurityAttributeList securityAttributes;
        protected internal TrivialHashtableUsingWeakReferences structurallyEquivalentMethod;
        protected internal TrivialHashtable szArrayTypes;
        protected TypeNode template;
        public TypeNodeList TemplateArgumentExpressions;
        protected TypeNodeList templateArguments;
        public TypeNode TemplateExpression;
        private TypeNodeList templateInstances;
        internal TypeNodeList templateParameters;
        internal ElementType typeCode;
        private static Hashtable typeMap;

        internal TypeNode(NodeType nodeType) : base(nodeType)
        {
            this.typeCode = ElementType.Class;
        }

        internal TypeNode(NodeType nodeType, NestedTypeProvider provideNestedTypes, TypeAttributeProvider provideAttributes, TypeMemberProvider provideMembers, object handle) : base(nodeType)
        {
            this.typeCode = ElementType.Class;
            this.ProvideNestedTypes = provideNestedTypes;
            this.ProvideTypeAttributes = provideAttributes;
            this.ProvideTypeMembers = provideMembers;
            this.ProviderHandle = handle;
            this.isNormalized = true;
        }

        internal TypeNode(System.Compiler.Module declaringModule, TypeNode declaringType, AttributeList attributes, TypeFlags flags, Identifier Namespace, Identifier name, InterfaceList interfaces, MemberList members, NodeType nodeType) : base(null, attributes, name, nodeType)
        {
            this.typeCode = ElementType.Class;
            this.DeclaringModule = declaringModule;
            this.DeclaringType = declaringType;
            this.Flags = flags;
            this.Interfaces = interfaces;
            this.members = members;
            this.Namespace = Namespace;
        }

        private void AddTemplateParametersFromAttributeEncoding(TypeNodeList result)
        {
        }

        private void AppendAssemblyQualifierIfNecessary(StringBuilder sb, TypeNode type, out bool isAssemQualified)
        {
            isAssemQualified = false;
            if (type != null)
            {
                AssemblyNode declaringModule = type.DeclaringModule as AssemblyNode;
                if (declaringModule != null)
                {
                    sb.Append(", ");
                    sb.Append(declaringModule.StrongName);
                    isAssemQualified = true;
                }
            }
        }

        internal virtual void AppendDocumentIdMangledName(StringBuilder sb, TypeNodeList methodTypeParameters, TypeNodeList typeParameters)
        {
            if (this.DeclaringType != null)
            {
                this.DeclaringType.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
                sb.Append('.');
                sb.Append(this.GetUnmangledNameWithoutTypeParameters());
            }
            else
            {
                sb.Append(this.GetFullUnmangledNameWithoutTypeParameters());
            }
            TypeNodeList templateArguments = this.TemplateArguments;
            int num = (templateArguments == null) ? 0 : templateArguments.Count;
            if (num != 0)
            {
                sb.Append('{');
                for (int i = 0; i < num; i++)
                {
                    TypeNode node = templateArguments[i];
                    if (node != null)
                    {
                        node.AppendDocumentIdMangledName(sb, methodTypeParameters, typeParameters);
                        if (i < (num - 1))
                        {
                            sb.Append(',');
                        }
                    }
                }
                sb.Append('}');
            }
        }

        private void AppendSerializedTypeName(StringBuilder sb, TypeNode type, ref bool isAssemQualified)
        {
            if (type != null)
            {
                string serializedTypeName = this.GetSerializedTypeName(type, ref isAssemQualified);
                if (isAssemQualified)
                {
                    sb.Append('[');
                }
                sb.Append(serializedTypeName);
                if (isAssemQualified)
                {
                    sb.Append(']');
                }
            }
        }

        public virtual void ClearMemberTable()
        {
            lock (this)
            {
                this.memberTable = null;
                this.memberCount = 0;
            }
        }

        public static TypeNode DeepStripModifiers(TypeNode type)
        {
            for (TypeModifier modifier = type as TypeModifier; modifier != null; modifier = type as TypeModifier)
            {
                type = modifier.ModifiedType;
            }
            ArrayType type2 = type as ArrayType;
            if (type2 != null)
            {
                return DeepStripModifiers(type2.ElementType).GetArrayType(1);
            }
            Reference reference = type as Reference;
            if (reference != null)
            {
                return DeepStripModifiers(reference.ElementType).GetReferenceType();
            }
            return type;
        }

        public static TypeNode DeepStripModifiers(TypeNode type, params TypeNode[] modifiers)
        {
            OptionalModifier modifier = type as OptionalModifier;
            if (modifier != null)
            {
                TypeNode modified = DeepStripModifiers(modifier.ModifiedType, modifiers);
                for (int j = 0; j < modifiers.Length; j++)
                {
                    if (modifier.Modifier == modifiers[j])
                    {
                        return modified;
                    }
                }
                return OptionalModifier.For(modifier.Modifier, modified);
            }
            RequiredModifier modifier2 = type as RequiredModifier;
            if (modifier2 != null)
            {
                TypeNode node2 = DeepStripModifiers(modifier2.ModifiedType, modifiers);
                for (int k = 0; k < modifiers.Length; k++)
                {
                    if (modifier2.Modifier == modifiers[k])
                    {
                        return node2;
                    }
                }
                return RequiredModifier.For(modifier2.Modifier, node2);
            }
            ArrayType type2 = type as ArrayType;
            if (type2 != null)
            {
                return DeepStripModifiers(type2.ElementType, modifiers).GetArrayType(1);
            }
            Reference reference = type as Reference;
            if (reference != null)
            {
                return DeepStripModifiers(reference.ElementType, modifiers).GetReferenceType();
            }
            if (((type.Template == null) || (type.TemplateArguments == null)) || (type.TemplateArguments.Count <= 0))
            {
                return type;
            }
            TypeNodeList templateArguments = new TypeNodeList(type.TemplateArguments.Count);
            for (int i = 0; i < type.TemplateArguments.Count; i++)
            {
                templateArguments.Add(DeepStripModifiers(type.TemplateArguments[i], modifiers));
            }
            return type.Template.GetTemplateInstance(type, templateArguments);
        }

        public static TypeNode DeepStripModifiers(TypeNode type, TypeNode templateType, params TypeNode[] modifiers)
        {
            if (templateType == null)
            {
                return DeepStripModifiers(type, modifiers);
            }
            if (templateType is ITypeParameter)
            {
                return type;
            }
            OptionalModifier modifier = type as OptionalModifier;
            if (modifier != null)
            {
                OptionalModifier modifier2 = (OptionalModifier) templateType;
                TypeNode modified = DeepStripModifiers(modifier.ModifiedType, modifier2.ModifiedType, modifiers);
                for (int j = 0; j < modifiers.Length; j++)
                {
                    if (modifier.Modifier == modifiers[j])
                    {
                        return modified;
                    }
                }
                return OptionalModifier.For(modifier.Modifier, modified);
            }
            RequiredModifier modifier3 = type as RequiredModifier;
            if (modifier3 != null)
            {
                RequiredModifier modifier4 = (RequiredModifier) templateType;
                TypeNode node2 = DeepStripModifiers(modifier3.ModifiedType, modifier4.ModifiedType, modifiers);
                for (int k = 0; k < modifiers.Length; k++)
                {
                    if (modifier3.Modifier == modifiers[k])
                    {
                        return node2;
                    }
                }
                return RequiredModifier.For(modifier3.Modifier, node2);
            }
            ArrayType type2 = type as ArrayType;
            if (type2 != null)
            {
                ArrayType type3 = (ArrayType) templateType;
                return DeepStripModifiers(type2.ElementType, type3.ElementType, modifiers).GetArrayType(1);
            }
            Reference reference = type as Reference;
            if (reference != null)
            {
                Reference reference2 = (Reference) templateType;
                return DeepStripModifiers(reference.ElementType, reference2.ElementType, modifiers).GetReferenceType();
            }
            if (((type.Template == null) || (type.TemplateArguments == null)) || (type.TemplateArguments.Count <= 0))
            {
                return type;
            }
            TypeNodeList templateArguments = new TypeNodeList(type.TemplateArguments.Count);
            for (int i = 0; i < type.TemplateArguments.Count; i++)
            {
                TypeNodeList list2 = (templateType.TemplateArguments != null) ? templateType.TemplateArguments : templateType.TemplateParameters;
                templateArguments.Add(DeepStripModifiers(type.TemplateArguments[i], list2[i], modifiers));
            }
            return type.Template.GetTemplateInstance(type, templateArguments);
        }

        public void DuplicateExtensions(TypeNode source, TypeNodeList newExtensions)
        {
            if (source != null)
            {
                this.extensions = newExtensions;
                this.extensionsExamined = source.extensionsExamined;
            }
        }

        public bool Equals(TypeNode other) => 
            (this == other);

        public override bool Equals(object other) => 
            (this == (other as TypeNode));

        public System.Compiler.Method ExplicitImplementation(System.Compiler.Method method)
        {
            if (this.ImplementsExplicitly(method))
            {
                return (System.Compiler.Method) this.explicitInterfaceImplementations[method.UniqueKey];
            }
            return null;
        }

        public virtual void GetAbstractMethods(MethodList result)
        {
            if (this.IsAbstract)
            {
                InterfaceList interfaces = this.Interfaces;
                int num = 0;
                int num2 = (interfaces == null) ? 0 : interfaces.Count;
                while (num < num2)
                {
                    Interface interface2 = interfaces[num];
                    if (interface2 != null)
                    {
                        MemberList members = interface2.Members;
                        int num3 = 0;
                        int num4 = (members == null) ? 0 : members.Count;
                        while (num3 < num4)
                        {
                            System.Compiler.Method method = members[num3] as System.Compiler.Method;
                            if (((method != null) && !this.ImplementsExplicitly(method)) && !this.ImplementsMethod(method, true))
                            {
                                result.Add(method);
                            }
                            num3++;
                        }
                    }
                    num++;
                }
            }
        }

        public virtual ArrayType GetArrayType(int rank) => 
            this.GetArrayType(rank, false);

        public virtual ArrayType GetArrayType(int rank, bool lowerBoundIsUnknown)
        {
            if ((rank > 1) || lowerBoundIsUnknown)
            {
                return this.GetArrayType(rank, 0, 0, new int[0], new int[0]);
            }
            if (this.szArrayTypes == null)
            {
                this.szArrayTypes = new TrivialHashtable();
            }
            ArrayType type = (ArrayType) this.szArrayTypes[rank];
            if (type != null)
            {
                return type;
            }
            lock (this)
            {
                type = (ArrayType) this.szArrayTypes[rank];
                if (type == null)
                {
                    this.szArrayTypes[rank] = type = new ArrayType(this, rank);
                    type.Flags &= ~TypeFlags.NestedFamORAssem;
                    type.Flags |= this.Flags & TypeFlags.NestedFamORAssem;
                }
                return type;
            }
        }

        public virtual ArrayType GetArrayType(int rank, int[] sizes, int[] loBounds) => 
            this.GetArrayType(rank, (sizes == null) ? 0 : sizes.Length, (loBounds == null) ? 0 : loBounds.Length, (sizes == null) ? new int[0] : sizes, (loBounds == null) ? new int[0] : loBounds);

        internal ArrayType GetArrayType(int rank, int numSizes, int numLoBounds, int[] sizes, int[] loBounds)
        {
            if (this.arrayTypes == null)
            {
                this.arrayTypes = new TrivialHashtable();
            }
            StringBuilder builder = new StringBuilder(rank * 5);
            for (int i = 0; i < rank; i++)
            {
                if (i < numLoBounds)
                {
                    builder.Append(loBounds[i]);
                }
                else
                {
                    builder.Append('0');
                }
                if (i < numSizes)
                {
                    builder.Append(':');
                    builder.Append(sizes[i]);
                }
                builder.Append(',');
            }
            Identifier identifier = Identifier.For(builder.ToString());
            ArrayType type = (ArrayType) this.arrayTypes[identifier.UniqueIdKey];
            if (type != null)
            {
                return type;
            }
            lock (this)
            {
                type = (ArrayType) this.arrayTypes[identifier.UniqueIdKey];
                if (type == null)
                {
                    if (loBounds == null)
                    {
                        loBounds = new int[0];
                    }
                    this.arrayTypes[identifier.UniqueIdKey] = type = new ArrayType(this, rank, sizes, loBounds);
                    type.Flags &= ~TypeFlags.NestedFamORAssem;
                    type.Flags |= this.Flags & TypeFlags.NestedFamORAssem;
                }
                return type;
            }
        }

        protected virtual TypeNodeList GetConsolidatedTemplateArguments()
        {
            TypeNodeList templateArguments = this.TemplateArguments;
            if (this.DeclaringType == null)
            {
                return templateArguments;
            }
            TypeNodeList consolidatedTemplateArguments = this.DeclaringType.ConsolidatedTemplateArguments;
            if (consolidatedTemplateArguments == null)
            {
                if (this.DeclaringType.IsGeneric && (this.DeclaringType.Template == null))
                {
                    consolidatedTemplateArguments = this.DeclaringType.ConsolidatedTemplateParameters;
                }
                if (consolidatedTemplateArguments == null)
                {
                    return templateArguments;
                }
            }
            int num = (templateArguments == null) ? 0 : templateArguments.Count;
            if (num != 0)
            {
                consolidatedTemplateArguments = consolidatedTemplateArguments.Clone();
                for (int i = 0; i < num; i++)
                {
                    consolidatedTemplateArguments.Add(templateArguments[i]);
                }
            }
            return consolidatedTemplateArguments;
        }

        protected virtual TypeNodeList GetConsolidatedTemplateArguments(TypeNodeList typeArgs)
        {
            TypeNodeList consolidatedTemplateArguments = this.ConsolidatedTemplateArguments;
            if ((consolidatedTemplateArguments == null) || (consolidatedTemplateArguments.Count == 0))
            {
                if (!this.IsGeneric || (this.Template != null))
                {
                    return typeArgs;
                }
                consolidatedTemplateArguments = this.ConsolidatedTemplateParameters;
            }
            int num = (typeArgs == null) ? 0 : typeArgs.Count;
            if (num != 0)
            {
                consolidatedTemplateArguments = consolidatedTemplateArguments.Clone();
                for (int i = 0; i < num; i++)
                {
                    consolidatedTemplateArguments.Add(typeArgs[i]);
                }
            }
            return consolidatedTemplateArguments;
        }

        public virtual TypeNode GetConsolidatedTemplateInstance(System.Compiler.Module module, TypeNode referringType, TypeNode declaringType, TypeNodeList templateArguments, TypeNodeList consolidatedTemplateArguments)
        {
            TypeNodeList templateParameters = this.TemplateParameters;
            if ((module == null) || ((declaringType == null) && ((templateParameters == null) || (templateParameters.Count == 0))))
            {
                return this;
            }
            if (this.IsGeneric)
            {
                referringType = null;
                module = cachingModuleForGenericInstances;
            }
            Identifier uniqueMangledTemplateInstanceName = this.GetUniqueMangledTemplateInstanceName(consolidatedTemplateArguments);
            TypeNode node = this.TryToFindExistingInstance(module, uniqueMangledTemplateInstanceName);
            if (node != null)
            {
                return node;
            }
            if (this.NewTemplateInstanceIsRecursive)
            {
                return this;
            }
            lock (System.Compiler.Module.GlobalLock)
            {
                bool flag;
                node = this.TryToFindExistingInstance(module, uniqueMangledTemplateInstanceName);
                if (node != null)
                {
                    return node;
                }
                Identifier mangledTemplateInstanceName = this.GetMangledTemplateInstanceName(templateArguments, out flag);
                return this.GetConsolidatedTemplateInstance(module, referringType, declaringType, templateArguments, uniqueMangledTemplateInstanceName, flag, mangledTemplateInstanceName, consolidatedTemplateArguments);
            }
        }

        private TypeNode GetConsolidatedTemplateInstance(System.Compiler.Module module, TypeNode referringType, TypeNode declaringType, TypeNodeList templateArguments, Identifier uniqueMangledName, bool notFullySpecialized, Identifier unusedMangledName, TypeNodeList consolidatedTemplateArguments)
        {
            TypeNode element = new Duplicator(module, declaringType) { 
                RecordOriginalAsTemplate = true,
                SkipBodies = true,
                TypesToBeDuplicated = { [this.UniqueKey] = this }
            }.VisitTypeNode(this, uniqueMangledName, templateArguments, this, true);
            if (module == this.DeclaringModule)
            {
                if (this.TemplateInstances == null)
                {
                    this.TemplateInstances = new TypeNodeList();
                }
                this.TemplateInstances.Add(element);
            }
            element.Name = unusedMangledName;
            element.Name.SourceContext = base.Name.SourceContext;
            element.fullName = null;
            if (this.IsGeneric)
            {
                element.DeclaringModule = this.DeclaringModule;
            }
            element.DeclaringType = (this.IsGeneric || (referringType == null)) ? declaringType : referringType;
            element.Template = this;
            element.templateParameters = null;
            element.consolidatedTemplateParameters = null;
            element.templateArguments = templateArguments;
            element.consolidatedTemplateArguments = consolidatedTemplateArguments;
            element.IsNotFullySpecialized = notFullySpecialized || ((declaringType != null) && TypeIsNotFullySpecialized(declaringType));
            module.StructurallyEquivalentType[unusedMangledName.UniqueIdKey] = element;
            module.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey] = element;
            new Specializer(module, this.ConsolidatedTemplateParameters, consolidatedTemplateArguments).VisitTypeNode(element);
            TypeFlags visibilityIntersection = this.Flags & TypeFlags.NestedFamORAssem;
            int num = 0;
            int count = templateArguments.Count;
            while (num < count)
            {
                TypeNode node2 = templateArguments[num];
                if (node2 != null)
                {
                    visibilityIntersection = GetVisibilityIntersection(visibilityIntersection, node2.Flags & TypeFlags.NestedFamORAssem);
                }
                num++;
            }
            element.Flags &= ~TypeFlags.NestedFamORAssem;
            element.Flags |= visibilityIntersection;
            return element;
        }

        protected virtual TypeNodeList GetConsolidatedTemplateParameters()
        {
            TypeNodeList templateParameters = this.TemplateParameters;
            TypeNode declaringType = this.DeclaringType;
            if (declaringType != null)
            {
                while (declaringType.Template != null)
                {
                    declaringType = declaringType.Template;
                }
                TypeNodeList consolidatedTemplateParameters = declaringType.ConsolidatedTemplateParameters;
                if (consolidatedTemplateParameters == null)
                {
                    return templateParameters;
                }
                int num = (templateParameters == null) ? 0 : templateParameters.Count;
                if (num != 0)
                {
                    consolidatedTemplateParameters = consolidatedTemplateParameters.Clone();
                    for (int i = 0; i < num; i++)
                    {
                        consolidatedTemplateParameters.Add(templateParameters[i]);
                    }
                }
                return consolidatedTemplateParameters;
            }
            return templateParameters;
        }

        public virtual InstanceInitializer GetConstructor(params TypeNode[] types) => 
            ((InstanceInitializer) GetFirstMethod(this.GetConstructors(), types));

        public virtual MemberList GetConstructors()
        {
            if (this.Members.Count != this.memberCount)
            {
                this.constructors = null;
            }
            if (this.constructors != null)
            {
                return this.constructors;
            }
            lock (this)
            {
                if (this.constructors != null)
                {
                    return this.constructors;
                }
                return (this.constructors = WeedOutNonSpecialMethods(this.GetMembersNamed(StandardIds.Ctor), MethodFlags.RTSpecialName));
            }
        }

        protected override Identifier GetDocumentationId()
        {
            if (this.DeclaringType == null)
            {
                return Identifier.For("T:" + this.FullName);
            }
            return Identifier.For(this.DeclaringType.DocumentationId + "." + base.Name);
        }

        public virtual Event GetEvent(Identifier name)
        {
            MemberList membersNamed = this.GetMembersNamed(name);
            int num = 0;
            int count = membersNamed.Count;
            while (num < count)
            {
                Event event2 = membersNamed[num] as Event;
                if (event2 != null)
                {
                    return event2;
                }
                num++;
            }
            return null;
        }

        public System.Compiler.Method GetExactMatchingMethod(System.Compiler.Method method)
        {
            if ((method != null) && (method.Name != null))
            {
                int num = (method.TemplateParameters == null) ? 0 : method.TemplateParameters.Count;
                MemberList membersNamed = this.GetMembersNamed(method.Name);
                int num2 = 0;
                int num3 = (membersNamed == null) ? 0 : membersNamed.Count;
                while (num2 < num3)
                {
                    System.Compiler.Method method2 = membersNamed[num2] as System.Compiler.Method;
                    if (method2 != null)
                    {
                        int num4 = (method2.TemplateParameters == null) ? 0 : method2.TemplateParameters.Count;
                        if (((num4 == num) && method2.ReturnType.IsStructurallyEquivalentTo(method.ReturnType)) && method2.ParametersMatchStructurally(method.Parameters))
                        {
                            return method2;
                        }
                    }
                    num2++;
                }
            }
            return null;
        }

        public virtual System.Compiler.Method GetExplicitCoercionFromMethod(TypeNode sourceType)
        {
            if (sourceType == null)
            {
                return null;
            }
            System.Compiler.Method method = null;
            if (this.explicitCoercionFromTable != null)
            {
                method = (System.Compiler.Method) this.explicitCoercionFromTable[sourceType.UniqueKey];
            }
            if (method == MethodDoesNotExist)
            {
                return null;
            }
            if (method != null)
            {
                return method;
            }
            lock (this)
            {
                if (this.explicitCoercionFromTable != null)
                {
                    method = (System.Compiler.Method) this.explicitCoercionFromTable[sourceType.UniqueKey];
                }
                if (method == MethodDoesNotExist)
                {
                    return null;
                }
                if (method == null)
                {
                    MemberList explicitCoercionMethods = this.ExplicitCoercionMethods;
                    int num = 0;
                    int count = explicitCoercionMethods.Count;
                    while (num < count)
                    {
                        System.Compiler.Method method3 = (System.Compiler.Method) explicitCoercionMethods[num];
                        if (sourceType == method3.Parameters[0].Type)
                        {
                            method = method3;
                            break;
                        }
                        num++;
                    }
                    if (this.explicitCoercionFromTable == null)
                    {
                        this.explicitCoercionFromTable = new TrivialHashtable();
                    }
                    if (method == null)
                    {
                        this.explicitCoercionFromTable[sourceType.UniqueKey] = MethodDoesNotExist;
                    }
                    else
                    {
                        this.explicitCoercionFromTable[sourceType.UniqueKey] = method;
                    }
                }
                return method;
            }
        }

        public virtual System.Compiler.Method GetExplicitCoercionToMethod(TypeNode targetType)
        {
            if (targetType == null)
            {
                return null;
            }
            System.Compiler.Method method = null;
            if (this.explicitCoercionToTable != null)
            {
                method = (System.Compiler.Method) this.explicitCoercionToTable[targetType.UniqueKey];
            }
            if (method == MethodDoesNotExist)
            {
                return null;
            }
            if (method == null)
            {
                lock (this)
                {
                    if (this.explicitCoercionToTable != null)
                    {
                        method = (System.Compiler.Method) this.explicitCoercionToTable[targetType.UniqueKey];
                    }
                    if (method == MethodDoesNotExist)
                    {
                        return null;
                    }
                    if (method != null)
                    {
                        return method;
                    }
                    MemberList explicitCoercionMethods = this.ExplicitCoercionMethods;
                    int num = 0;
                    int count = explicitCoercionMethods.Count;
                    while (num < count)
                    {
                        System.Compiler.Method method3 = (System.Compiler.Method) explicitCoercionMethods[num];
                        if (method3.ReturnType == targetType)
                        {
                            method = method3;
                            break;
                        }
                        num++;
                    }
                    if (this.explicitCoercionToTable == null)
                    {
                        this.explicitCoercionToTable = new TrivialHashtable();
                    }
                    if (method == null)
                    {
                        this.explicitCoercionToTable[targetType.UniqueKey] = MethodDoesNotExist;
                        return method;
                    }
                    this.explicitCoercionToTable[targetType.UniqueKey] = method;
                }
            }
            return method;
        }

        public virtual Field GetField(Identifier name)
        {
            MemberList membersNamed = this.GetMembersNamed(name);
            int num = 0;
            int count = membersNamed.Count;
            while (num < count)
            {
                Field field = membersNamed[num] as Field;
                if (field != null)
                {
                    return field;
                }
                num++;
            }
            return null;
        }

        private static System.Compiler.Method GetFirstMethod(MemberList members, params TypeNode[] types)
        {
            if (members != null)
            {
                TypeNodeList argumentTypes = (((types == null) ? 0 : types.Length) == 0) ? null : new TypeNodeList(types);
                int num2 = 0;
                int count = members.Count;
                while (num2 < count)
                {
                    System.Compiler.Method method = members[num2] as System.Compiler.Method;
                    if ((method != null) && method.ParameterTypesMatchStructurally(argumentTypes))
                    {
                        return method;
                    }
                    num2++;
                }
            }
            return null;
        }

        public virtual string GetFullUnmangledNameWithoutTypeParameters()
        {
            if (this.DeclaringType != null)
            {
                return (this.DeclaringType.GetFullUnmangledNameWithoutTypeParameters() + "+" + this.GetUnmangledNameWithoutTypeParameters());
            }
            if ((this.Namespace != null) && (this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey))
            {
                return (this.Namespace.ToString() + "." + this.GetUnmangledNameWithoutTypeParameters());
            }
            return this.GetUnmangledNameWithoutTypeParameters();
        }

        public virtual string GetFullUnmangledNameWithTypeParameters()
        {
            if (this.DeclaringType != null)
            {
                return (this.DeclaringType.GetFullUnmangledNameWithTypeParameters() + "+" + this.GetUnmangledNameWithTypeParameters(true));
            }
            if ((this.Namespace != null) && (this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey))
            {
                return (this.Namespace.ToString() + "." + this.GetUnmangledNameWithTypeParameters(true));
            }
            return this.GetUnmangledNameWithTypeParameters(true);
        }

        public virtual TypeNode GetGenericTemplateInstance(System.Compiler.Module module, TypeNodeList consolidatedArguments)
        {
            if (this.DeclaringType == null)
            {
                return this.GetTemplateInstance(module, null, null, consolidatedArguments);
            }
            TypeNodeList ownTemplateArguments = this.GetOwnTemplateArguments(consolidatedArguments);
            if (ownTemplateArguments == consolidatedArguments)
            {
                return this.GetTemplateInstance(module, null, this.DeclaringType, consolidatedArguments);
            }
            int count = consolidatedArguments.Count;
            int num2 = (ownTemplateArguments == null) ? 0 : ownTemplateArguments.Count;
            int capacity = count - num2;
            TypeNodeList list2 = new TypeNodeList(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list2.Add(consolidatedArguments[i]);
            }
            TypeNode genericTemplateInstance = this.DeclaringType.GetGenericTemplateInstance(module, list2);
            return this.GetConsolidatedTemplateInstance(module, null, genericTemplateInstance, ownTemplateArguments, consolidatedArguments);
        }

        public override int GetHashCode()
        {
            TypeNode effectiveTypeNode = this.EffectiveTypeNode;
            if (effectiveTypeNode == this)
            {
                return base.GetHashCode();
            }
            return effectiveTypeNode.GetHashCode();
        }

        public System.Compiler.Method GetImplementingMethod(System.Compiler.Method meth, bool checkPublic)
        {
            if (meth != null)
            {
                MemberList membersNamed = this.GetMembersNamed(meth.Name);
                int num = 0;
                int num2 = (membersNamed == null) ? 0 : membersNamed.Count;
                while (num < num2)
                {
                    System.Compiler.Method method = membersNamed[num] as System.Compiler.Method;
                    if (((((method != null) && method.IsVirtual) && (!checkPublic || method.IsPublic)) && ((method.ReturnType == meth.ReturnType) || ((method.ReturnType != null) && method.ReturnType.IsStructurallyEquivalentTo(meth.ReturnType)))) && method.ParametersMatchStructurally(meth.Parameters))
                    {
                        return method;
                    }
                    num++;
                }
                if (checkPublic && (this.BaseType != null))
                {
                    return this.BaseType.GetImplementingMethod(meth, true);
                }
            }
            return null;
        }

        public virtual System.Compiler.Method GetImplicitCoercionFromMethod(TypeNode sourceType)
        {
            if (sourceType == null)
            {
                return null;
            }
            System.Compiler.Method method = null;
            if (this.implicitCoercionFromTable != null)
            {
                method = (System.Compiler.Method) this.implicitCoercionFromTable[sourceType.UniqueKey];
            }
            if (method == MethodDoesNotExist)
            {
                return null;
            }
            if (method != null)
            {
                return method;
            }
            lock (this)
            {
                if (this.implicitCoercionFromTable != null)
                {
                    method = (System.Compiler.Method) this.implicitCoercionFromTable[sourceType.UniqueKey];
                }
                if (method == MethodDoesNotExist)
                {
                    return null;
                }
                if (method == null)
                {
                    MemberList implicitCoercionMethods = this.ImplicitCoercionMethods;
                    int num = 0;
                    int count = implicitCoercionMethods.Count;
                    while (num < count)
                    {
                        System.Compiler.Method method3 = (System.Compiler.Method) implicitCoercionMethods[num];
                        if (sourceType.IsStructurallyEquivalentTo(StripModifiers(method3.Parameters[0].Type)))
                        {
                            method = method3;
                            break;
                        }
                        num++;
                    }
                    if (this.implicitCoercionFromTable == null)
                    {
                        this.implicitCoercionFromTable = new TrivialHashtable();
                    }
                    if (method == null)
                    {
                        this.implicitCoercionFromTable[sourceType.UniqueKey] = MethodDoesNotExist;
                    }
                    else
                    {
                        this.implicitCoercionFromTable[sourceType.UniqueKey] = method;
                    }
                }
                return method;
            }
        }

        public virtual System.Compiler.Method GetImplicitCoercionToMethod(TypeNode targetType)
        {
            if (targetType == null)
            {
                return null;
            }
            System.Compiler.Method method = null;
            if (this.implicitCoercionToTable != null)
            {
                method = (System.Compiler.Method) this.implicitCoercionToTable[targetType.UniqueKey];
            }
            if (method == MethodDoesNotExist)
            {
                return null;
            }
            if (method != null)
            {
                return method;
            }
            lock (this)
            {
                if (this.implicitCoercionToTable != null)
                {
                    method = (System.Compiler.Method) this.implicitCoercionToTable[targetType.UniqueKey];
                }
                if (method == MethodDoesNotExist)
                {
                    return null;
                }
                if (method == null)
                {
                    MemberList implicitCoercionMethods = this.ImplicitCoercionMethods;
                    int num = 0;
                    int count = implicitCoercionMethods.Count;
                    while (num < count)
                    {
                        System.Compiler.Method method3 = (System.Compiler.Method) implicitCoercionMethods[num];
                        if (method3.ReturnType == targetType)
                        {
                            method = method3;
                            break;
                        }
                        num++;
                    }
                    if (this.implicitCoercionToTable == null)
                    {
                        this.implicitCoercionToTable = new TrivialHashtable();
                    }
                    if (method == null)
                    {
                        this.implicitCoercionToTable[targetType.UniqueKey] = MethodDoesNotExist;
                    }
                    else
                    {
                        this.implicitCoercionToTable[targetType.UniqueKey] = method;
                    }
                }
                return method;
            }
        }

        public virtual Identifier GetMangledTemplateInstanceName(TypeNodeList templateArguments, out bool notFullySpecialized)
        {
            StringBuilder builder = new StringBuilder(base.Name.ToString());
            notFullySpecialized = false;
            int num = 0;
            int count = templateArguments.Count;
            while (num < count)
            {
                if (num == 0)
                {
                    builder.Append('<');
                }
                TypeNode node = templateArguments[num];
                if ((node != null) && (node.Name != null))
                {
                    builder.Append(node.FullName);
                    if (num < (count - 1))
                    {
                        builder.Append(',');
                    }
                    else
                    {
                        builder.Append('>');
                    }
                }
                num++;
            }
            return Identifier.For(builder.ToString());
        }

        public System.Compiler.Method GetMatchingMethod(System.Compiler.Method method)
        {
            if ((method != null) && (method.Name != null))
            {
                MemberList membersNamed = this.GetMembersNamed(method.Name);
                int num = 0;
                int num2 = (membersNamed == null) ? 0 : membersNamed.Count;
                while (num < num2)
                {
                    System.Compiler.Method method2 = membersNamed[num] as System.Compiler.Method;
                    if ((method2 != null) && method2.ParametersMatchStructurally(method.Parameters))
                    {
                        return method2;
                    }
                    num++;
                }
            }
            return null;
        }

        public virtual MemberList GetMembersNamed(Identifier name)
        {
            if (name == null)
            {
                return new MemberList(0);
            }
            MemberList members = this.Members;
            int range = (members == null) ? 0 : members.Count;
            if ((range != this.memberCount) || (this.memberTable == null))
            {
                this.UpdateMemberTable(range);
            }
            MemberList list2 = (MemberList) this.memberTable[name.UniqueIdKey];
            if (list2 == null)
            {
                lock (this)
                {
                    list2 = (MemberList) this.memberTable[name.UniqueIdKey];
                    if (list2 != null)
                    {
                        return list2;
                    }
                    this.memberTable[name.UniqueIdKey] = list2 = new MemberList();
                }
            }
            return list2;
        }

        public virtual System.Compiler.Method GetMethod(Identifier name, params TypeNode[] types) => 
            GetFirstMethod(this.GetMembersNamed(name), types);

        public virtual MethodList GetMethods(Identifier name, params TypeNode[] types) => 
            GetMethods(this.GetMembersNamed(name), types);

        private static MethodList GetMethods(MemberList members, params TypeNode[] types)
        {
            if (members == null)
            {
                return null;
            }
            int num = (types == null) ? 0 : types.Length;
            MethodList list = new MethodList();
            TypeNodeList argumentTypes = (num == 0) ? null : new TypeNodeList(types);
            int num2 = 0;
            int count = members.Count;
            while (num2 < count)
            {
                System.Compiler.Method element = members[num2] as System.Compiler.Method;
                if ((element != null) && element.ParameterTypesMatchStructurally(argumentTypes))
                {
                    list.Add(element);
                }
                num2++;
            }
            return list;
        }

        internal TypeNode GetModified(TypeNode modifierType, bool optionalModifier)
        {
            if (this.modifierTable == null)
            {
                this.modifierTable = new TrivialHashtable();
            }
            TypeNode node = (TypeNode) this.modifierTable[modifierType.UniqueKey];
            if (node == null)
            {
                node = optionalModifier ? ((TypeNode) new OptionalModifier(modifierType, this)) : ((TypeNode) new RequiredModifier(modifierType, this));
                this.modifierTable[modifierType.UniqueKey] = node;
            }
            return node;
        }

        public virtual TypeNode GetNestedType(Identifier name)
        {
            if (name != null)
            {
                if (this.template != null)
                {
                    throw new InvalidOperationException();
                }
                if (this.members != null)
                {
                    MemberList membersNamed = this.GetMembersNamed(name);
                    int num = 0;
                    int count = membersNamed.Count;
                    while (num < count)
                    {
                        TypeNode node = membersNamed[num] as TypeNode;
                        if (node != null)
                        {
                            return node;
                        }
                        num++;
                    }
                    return null;
                }
                TypeNodeList nestedTypes = this.NestedTypes;
                int num3 = 0;
                int num4 = (nestedTypes == null) ? 0 : nestedTypes.Count;
                while (num3 < num4)
                {
                    TypeNode node2 = nestedTypes[num3];
                    if ((node2 != null) && (node2.Name.UniqueIdKey == name.UniqueIdKey))
                    {
                        return node2;
                    }
                    num3++;
                }
            }
            return null;
        }

        public virtual System.Compiler.Method GetOpFalse()
        {
            System.Compiler.Method opFalse = this.opFalse;
            if (opFalse == MethodDoesNotExist)
            {
                return null;
            }
            if (opFalse != null)
            {
                return opFalse;
            }
            if (this.Members != null)
            {
            }
            lock (this)
            {
                opFalse = this.opFalse;
                if (opFalse != MethodDoesNotExist)
                {
                    if (opFalse != null)
                    {
                        return opFalse;
                    }
                    for (TypeNode node2 = this; node2 != null; node2 = node2.BaseType)
                    {
                        MemberList membersNamed = node2.GetMembersNamed(StandardIds.opFalse);
                        if (membersNamed != null)
                        {
                            int num = 0;
                            int count = membersNamed.Count;
                            while (num < count)
                            {
                                System.Compiler.Method method3 = membersNamed[num] as System.Compiler.Method;
                                if ((((method3 != null) && method3.IsSpecialName) && (method3.IsStatic && method3.IsPublic)) && (((method3.ReturnType == CoreSystemTypes.Boolean) && (method3.Parameters != null)) && (method3.Parameters.Count == 1)))
                                {
                                    return (this.opFalse = method3);
                                }
                                num++;
                            }
                        }
                    }
                    this.opFalse = MethodDoesNotExist;
                }
                return null;
            }
        }

        public virtual System.Compiler.Method GetOpTrue()
        {
            System.Compiler.Method opTrue = this.opTrue;
            if (opTrue == MethodDoesNotExist)
            {
                return null;
            }
            if (opTrue != null)
            {
                return opTrue;
            }
            if (this.Members != null)
            {
            }
            lock (this)
            {
                opTrue = this.opTrue;
                if (opTrue != MethodDoesNotExist)
                {
                    if (opTrue != null)
                    {
                        return opTrue;
                    }
                    for (TypeNode node2 = this; node2 != null; node2 = node2.BaseType)
                    {
                        MemberList membersNamed = node2.GetMembersNamed(StandardIds.opTrue);
                        if (membersNamed != null)
                        {
                            int num = 0;
                            int count = membersNamed.Count;
                            while (num < count)
                            {
                                System.Compiler.Method method3 = membersNamed[num] as System.Compiler.Method;
                                if ((((method3 != null) && method3.IsSpecialName) && (method3.IsStatic && method3.IsPublic)) && (((method3.ReturnType == CoreSystemTypes.Boolean) && (method3.Parameters != null)) && (method3.Parameters.Count == 1)))
                                {
                                    return (this.opTrue = method3);
                                }
                                num++;
                            }
                        }
                    }
                    this.opTrue = MethodDoesNotExist;
                }
                return null;
            }
        }

        protected virtual TypeNodeList GetOwnTemplateArguments(TypeNodeList consolidatedTemplateArguments)
        {
            int capacity = (this.TemplateParameters == null) ? 0 : this.TemplateParameters.Count;
            int num2 = (consolidatedTemplateArguments == null) ? 0 : consolidatedTemplateArguments.Count;
            int num3 = num2 - capacity;
            if (num3 <= 0)
            {
                return consolidatedTemplateArguments;
            }
            TypeNodeList list = new TypeNodeList(capacity);
            if (consolidatedTemplateArguments != null)
            {
                for (int i = 0; i < capacity; i++)
                {
                    list.Add(consolidatedTemplateArguments[i + num3]);
                }
            }
            return list;
        }

        public virtual System.Compiler.Pointer GetPointerType()
        {
            System.Compiler.Pointer pointerType = this.pointerType;
            if (pointerType == null)
            {
                lock (this)
                {
                    if (this.pointerType != null)
                    {
                        return this.pointerType;
                    }
                    pointerType = this.pointerType = new System.Compiler.Pointer(this);
                    pointerType.Flags &= ~TypeFlags.NestedFamORAssem;
                    pointerType.Flags |= this.Flags & TypeFlags.NestedFamORAssem;
                    pointerType.DeclaringModule = this.DeclaringModule;
                }
            }
            return pointerType;
        }

        public virtual Property GetProperty(Identifier name, params TypeNode[] types) => 
            GetProperty(this.GetMembersNamed(name), types);

        private static Property GetProperty(MemberList members, params TypeNode[] types)
        {
            if (members != null)
            {
                TypeNodeList argumentTypes = (((types == null) ? 0 : types.Length) == 0) ? null : new TypeNodeList(types);
                int num2 = 0;
                int count = members.Count;
                while (num2 < count)
                {
                    Property property = members[num2] as Property;
                    if ((property != null) && property.ParameterTypesMatch(argumentTypes))
                    {
                        return property;
                    }
                    num2++;
                }
            }
            return null;
        }

        public virtual Reference GetReferenceType()
        {
            Reference referenceType = this.referenceType;
            if (referenceType == null)
            {
                lock (this)
                {
                    if (this.referenceType != null)
                    {
                        return this.referenceType;
                    }
                    referenceType = this.referenceType = new Reference(this);
                    referenceType.Flags &= ~TypeFlags.NestedFamORAssem;
                    referenceType.Flags |= this.Flags & TypeFlags.NestedFamORAssem;
                    referenceType.DeclaringModule = this.DeclaringModule;
                }
            }
            return referenceType;
        }

        public virtual Type GetRuntimeType()
        {
            if (this.runtimeType == null)
            {
                lock (this)
                {
                    if (this.runtimeType != null)
                    {
                        return this.runtimeType;
                    }
                    if (this.IsGeneric && (this.Template != null))
                    {
                        try
                        {
                            TypeNode template = this.Template;
                            while (template.Template != null)
                            {
                                template = template.Template;
                            }
                            Type runtimeType = template.GetRuntimeType();
                            if (runtimeType == null)
                            {
                                return null;
                            }
                            TypeNodeList consolidatedTemplateArguments = this.ConsolidatedTemplateArguments;
                            Type[] typeArguments = new Type[consolidatedTemplateArguments.Count];
                            for (int i = 0; i < consolidatedTemplateArguments.Count; i++)
                            {
                                typeArguments[i] = consolidatedTemplateArguments[i].GetRuntimeType();
                            }
                            return runtimeType.MakeGenericType(typeArguments);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    if (this.DeclaringType != null)
                    {
                        Type type3 = this.DeclaringType.GetRuntimeType();
                        if (type3 != null)
                        {
                            BindingFlags declaredOnly = BindingFlags.DeclaredOnly;
                            if (this.IsPublic)
                            {
                                declaredOnly |= BindingFlags.Public;
                            }
                            else
                            {
                                declaredOnly |= BindingFlags.NonPublic;
                            }
                            this.runtimeType = type3.GetNestedType(base.Name.ToString(), declaredOnly);
                        }
                    }
                    else if (((this.DeclaringModule != null) && this.DeclaringModule.IsNormalized) && (this.DeclaringModule.ContainingAssembly != null))
                    {
                        Assembly runtimeAssembly = this.DeclaringModule.ContainingAssembly.GetRuntimeAssembly();
                        if (runtimeAssembly != null)
                        {
                            this.runtimeType = runtimeAssembly.GetType(this.FullName, false);
                        }
                    }
                }
            }
            return this.runtimeType;
        }

        public virtual string GetSerializedTypeName()
        {
            bool isAssemblyQualified = true;
            return this.GetSerializedTypeName(this, ref isAssemblyQualified);
        }

        private string GetSerializedTypeName(TypeNode type, ref bool isAssemblyQualified)
        {
            if (type == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            TypeModifier modifier = type as TypeModifier;
            if (modifier != null)
            {
                type = modifier.ModifiedType;
            }
            ArrayType type2 = type as ArrayType;
            if (type2 != null)
            {
                type = type2.ElementType;
                bool isAssemQualified = false;
                this.AppendSerializedTypeName(sb, type2.ElementType, ref isAssemQualified);
                if (type2.IsSzArray())
                {
                    sb.Append("[]");
                }
                else
                {
                    sb.Append('[');
                    if (type2.Rank == 1)
                    {
                        sb.Append('*');
                    }
                    for (int i = 1; i < type2.Rank; i++)
                    {
                        sb.Append(',');
                    }
                    sb.Append(']');
                }
            }
            else
            {
                System.Compiler.Pointer pointer = type as System.Compiler.Pointer;
                if (pointer != null)
                {
                    type = pointer.ElementType;
                    bool flag2 = false;
                    this.AppendSerializedTypeName(sb, pointer.ElementType, ref flag2);
                    sb.Append('*');
                }
                else
                {
                    Reference reference = type as Reference;
                    if (reference != null)
                    {
                        type = reference.ElementType;
                        bool flag3 = false;
                        this.AppendSerializedTypeName(sb, reference.ElementType, ref flag3);
                        sb.Append('&');
                    }
                    else if (type.Template == null)
                    {
                        sb.Append(type.FullName);
                    }
                    else
                    {
                        sb.Append(type.Template.FullName);
                        sb.Append('[');
                        int num2 = 0;
                        int num3 = (type.TemplateArguments == null) ? 0 : type.TemplateArguments.Count;
                        while (num2 < num3)
                        {
                            bool flag4 = true;
                            this.AppendSerializedTypeName(sb, type.TemplateArguments[num2], ref flag4);
                            if (num2 < (num3 - 1))
                            {
                                sb.Append(',');
                            }
                            num2++;
                        }
                        sb.Append(']');
                    }
                }
            }
            if (isAssemblyQualified)
            {
                this.AppendAssemblyQualifierIfNecessary(sb, type, out isAssemblyQualified);
            }
            return sb.ToString();
        }

        public virtual TypeNode GetTemplateInstance(System.Compiler.Module module, params TypeNode[] typeArguments) => 
            this.GetTemplateInstance(module, null, null, new TypeNodeList(typeArguments));

        public virtual TypeNode GetTemplateInstance(TypeNode referringType, params TypeNode[] templateArguments) => 
            this.GetTemplateInstance(referringType, new TypeNodeList(templateArguments));

        public virtual TypeNode GetTemplateInstance(TypeNode referringType, TypeNodeList templateArguments)
        {
            if (referringType == null)
            {
                return this;
            }
            System.Compiler.Module declaringModule = referringType.DeclaringModule;
            return this.GetTemplateInstance(declaringModule, referringType, this.DeclaringType, templateArguments);
        }

        public virtual TypeNode GetTemplateInstance(System.Compiler.Module module, TypeNode referringType, TypeNode declaringType, TypeNodeList templateArguments)
        {
            TypeNodeList templateParameters = this.TemplateParameters;
            if ((templateArguments == null) || ((declaringType == null) && ((templateParameters == null) || (templateParameters.Count == 0))))
            {
                return this;
            }
            module = cachingModuleForGenericInstances;
            Identifier uniqueMangledTemplateInstanceName = this.GetUniqueMangledTemplateInstanceName(templateArguments);
            TypeNode node = this.TryToFindExistingInstance(module, uniqueMangledTemplateInstanceName);
            if (node != null)
            {
                return node;
            }
            if (this.NewTemplateInstanceIsRecursive)
            {
                return this;
            }
            lock (System.Compiler.Module.GlobalLock)
            {
                node = this.TryToFindExistingInstance(module, uniqueMangledTemplateInstanceName);
                if (node == null)
                {
                    bool flag;
                    Identifier mangledTemplateInstanceName = this.GetMangledTemplateInstanceName(templateArguments, out flag);
                    TypeNodeList consolidatedTemplateArguments = templateArguments;
                    node = this.GetConsolidatedTemplateInstance(module, referringType, declaringType, templateArguments, uniqueMangledTemplateInstanceName, flag, mangledTemplateInstanceName, consolidatedTemplateArguments);
                    if (this.IsGeneric)
                    {
                        return node;
                    }
                }
                return node;
            }
        }

        public static TypeNode GetTypeNode(Type type)
        {
            if (type == null)
            {
                return null;
            }
            Hashtable typeMap = TypeNode.typeMap;
            if (typeMap == null)
            {
                TypeNode.typeMap = typeMap = Hashtable.Synchronized(new Hashtable());
            }
            TypeNode target = null;
            WeakReference reference = (WeakReference) typeMap[type];
            if (reference != null)
            {
                target = reference.Target as TypeNode;
                if (target == Class.DoesNotExist)
                {
                    return null;
                }
                if (target != null)
                {
                    return target;
                }
            }
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                try
                {
                    TypeNode typeNode = GetTypeNode(type.GetGenericTypeDefinition());
                    if (typeNode == null)
                    {
                        return null;
                    }
                    TypeNodeList consolidatedArguments = new TypeNodeList();
                    foreach (Type type2 in type.GetGenericArguments())
                    {
                        consolidatedArguments.Add(GetTypeNode(type2));
                    }
                    return typeNode.GetGenericTemplateInstance(typeNode.DeclaringModule, consolidatedArguments);
                }
                catch
                {
                    return null;
                }
            }
            if (type.IsGenericParameter)
            {
                try
                {
                    int genericParameterPosition = type.GenericParameterPosition;
                    System.Reflection.MethodInfo declaringMethod = type.DeclaringMethod as System.Reflection.MethodInfo;
                    if (declaringMethod != null)
                    {
                        System.Compiler.Method method = System.Compiler.Method.GetMethod(declaringMethod);
                        if (method == null)
                        {
                            return null;
                        }
                        if ((method.TemplateParameters != null) && (method.TemplateParameters.Count > genericParameterPosition))
                        {
                            return method.TemplateParameters[genericParameterPosition];
                        }
                    }
                    else
                    {
                        TypeNode node4 = GetTypeNode(type.DeclaringType);
                        if (node4 == null)
                        {
                            return null;
                        }
                        if ((node4.TemplateParameters != null) && (node4.TemplateParameters.Count > genericParameterPosition))
                        {
                            return node4.TemplateParameters[genericParameterPosition];
                        }
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
            if (type.HasElementType)
            {
                TypeNode node5 = GetTypeNode(type.GetElementType());
                if (node5 == null)
                {
                    return null;
                }
                if (type.IsArray)
                {
                    target = node5.GetArrayType(type.GetArrayRank());
                }
                else if (type.IsByRef)
                {
                    target = node5.GetReferenceType();
                }
                else if (type.IsPointer)
                {
                    target = node5.GetPointerType();
                }
                else
                {
                    target = null;
                }
            }
            else if (type.DeclaringType != null)
            {
                TypeNode node6 = GetTypeNode(type.DeclaringType);
                if (node6 == null)
                {
                    return null;
                }
                target = node6.GetNestedType(Identifier.For(type.Name));
            }
            else
            {
                AssemblyNode assembly = AssemblyNode.GetAssembly(type.Assembly);
                if (assembly != null)
                {
                    target = assembly.GetType(Identifier.For(type.Namespace), Identifier.For(type.Name));
                }
            }
            if (target == null)
            {
                typeMap[type] = new WeakReference(Class.DoesNotExist);
                return target;
            }
            typeMap[type] = new WeakReference(target);
            return target;
        }

        private Identifier GetUniqueMangledTemplateInstanceName(TypeNodeList templateArguments) => 
            GetUniqueMangledTemplateInstanceName(this.UniqueKey, templateArguments);

        internal static Identifier GetUniqueMangledTemplateInstanceName(int templateId, TypeNodeList templateArguments)
        {
            StringBuilder builder = new StringBuilder(templateId.ToString());
            int num = 0;
            int count = templateArguments.Count;
            while (num < count)
            {
                TypeNode node = templateArguments[num];
                if ((node != null) && (node.Name != null))
                {
                    builder.Append(':');
                    builder.Append(node.UniqueKey);
                }
                num++;
            }
            return Identifier.For(builder.ToString());
        }

        public virtual string GetUnmangledNameWithoutTypeParameters()
        {
            MangleChars[0] = TargetPlatform.GenericTypeNamesMangleChar;
            if (this.Template != null)
            {
                return this.Template.GetUnmangledNameWithoutTypeParameters();
            }
            if (base.Name == null)
            {
                return "";
            }
            string str = base.Name.ToString();
            if ((this.TemplateParameters == null) || (this.TemplateParameters.Count <= 0))
            {
                return str;
            }
            int length = str.LastIndexOfAny(MangleChars);
            if (length < 0)
            {
                return str;
            }
            if (str[length] == '>')
            {
                length++;
            }
            return str.Substring(0, length);
        }

        public virtual string GetUnmangledNameWithTypeParameters() => 
            this.GetUnmangledNameWithTypeParameters(false);

        private string GetUnmangledNameWithTypeParameters(bool fullNamesForTypeParameters)
        {
            StringBuilder builder = new StringBuilder(this.GetUnmangledNameWithoutTypeParameters());
            TypeNodeList templateParameters = this.TemplateParameters;
            if (this.Template != null)
            {
                templateParameters = this.TemplateArguments;
            }
            int num = 0;
            int num2 = (templateParameters == null) ? 0 : templateParameters.Count;
            while (num < num2)
            {
                TypeNode node = templateParameters[num];
                if (node != null)
                {
                    if (num == 0)
                    {
                        builder.Append('<');
                    }
                    else
                    {
                        builder.Append(',');
                    }
                    if (node.Name != null)
                    {
                        if (fullNamesForTypeParameters)
                        {
                            builder.Append(node.GetFullUnmangledNameWithTypeParameters());
                        }
                        else
                        {
                            builder.Append(node.GetUnmangledNameWithTypeParameters());
                        }
                    }
                    if (num == (num2 - 1))
                    {
                        builder.Append('>');
                    }
                }
                num++;
            }
            return builder.ToString();
        }

        public static TypeFlags GetVisibilityIntersection(TypeFlags vis1, TypeFlags vis2)
        {
            switch (vis2)
            {
                case TypeFlags.AnsiClass:
                case TypeFlags.NestedAssembly:
                    switch (vis1)
                    {
                        case TypeFlags.Public:
                            return vis2;

                        case TypeFlags.NestedPublic:
                        case TypeFlags.NestedFamORAssem:
                            return TypeFlags.NestedAssembly;

                        case TypeFlags.NestedPrivate:
                        case TypeFlags.NestedAssembly:
                        case TypeFlags.NestedFamANDAssem:
                            return vis1;

                        case TypeFlags.NestedFamily:
                            return TypeFlags.NestedFamANDAssem;
                    }
                    return vis1;

                case TypeFlags.Public:
                case TypeFlags.NestedPublic:
                    return vis1;

                case TypeFlags.NestedFamily:
                    switch (vis1)
                    {
                        case TypeFlags.Public:
                        case TypeFlags.NestedPublic:
                        case TypeFlags.NestedFamORAssem:
                            return TypeFlags.NestedFamily;

                        case TypeFlags.NestedPrivate:
                        case TypeFlags.NestedFamily:
                        case TypeFlags.NestedFamANDAssem:
                            return vis1;

                        case TypeFlags.NestedAssembly:
                            return TypeFlags.NestedFamANDAssem;
                    }
                    return vis1;

                case TypeFlags.NestedFamANDAssem:
                    switch (vis1)
                    {
                        case TypeFlags.Public:
                        case TypeFlags.NestedPublic:
                        case TypeFlags.NestedFamily:
                        case TypeFlags.NestedFamORAssem:
                            return TypeFlags.NestedFamANDAssem;

                        case TypeFlags.NestedPrivate:
                        case TypeFlags.NestedAssembly:
                        case TypeFlags.NestedFamANDAssem:
                            return vis1;
                    }
                    return vis1;

                case TypeFlags.NestedFamORAssem:
                    switch (vis1)
                    {
                        case TypeFlags.Public:
                        case TypeFlags.NestedPublic:
                            return TypeFlags.NestedFamORAssem;
                    }
                    return vis1;
            }
            return TypeFlags.NestedPrivate;
        }

        public static bool HasModifier(TypeNode type, TypeNode modifier)
        {
            TypeModifier modifier2 = type as TypeModifier;
            return ((modifier2?.Modifier == modifier) || HasModifier(modifier2.ModifiedType, modifier));
        }

        public bool ImplementsExplicitly(System.Compiler.Method method)
        {
            if (method == null)
            {
                return false;
            }
            TrivialHashtable explicitInterfaceImplementations = this.explicitInterfaceImplementations;
            return (explicitInterfaceImplementations?[method.UniqueKey] != null);
        }

        internal bool ImplementsMethod(System.Compiler.Method meth, bool checkPublic) => 
            (this.GetImplementingMethod(meth, checkPublic) != null);

        public virtual bool IsAssignableTo(TypeNode targetType)
        {
            if (this != CoreSystemTypes.Void)
            {
                if (targetType == this)
                {
                    return true;
                }
                if (this == CoreSystemTypes.Object)
                {
                    return false;
                }
                if (((targetType == CoreSystemTypes.Object) || this.IsStructurallyEquivalentTo(targetType)) || ((this.BaseType != null) && this.BaseType.IsAssignableTo(targetType)))
                {
                    return true;
                }
                if (((this.BaseType != null) && (this.ConsolidatedTemplateParameters != null)) && ((this.BaseType.Template != null) && this.BaseType.Template.IsAssignableTo(targetType)))
                {
                    return true;
                }
                InterfaceList interfaces = this.Interfaces;
                if (interfaces != null)
                {
                    int num = 0;
                    int count = interfaces.Count;
                    while (num < count)
                    {
                        Interface interface2 = interfaces[num];
                        if (interface2 != null)
                        {
                            if (interface2.IsAssignableTo(targetType))
                            {
                                return true;
                            }
                            if (((interface2.Template != null) && (this.ConsolidatedTemplateParameters != null)) && interface2.Template.IsAssignableTo(targetType))
                            {
                                return true;
                            }
                        }
                        num++;
                    }
                }
            }
            return false;
        }

        public virtual bool IsAssignableToInstanceOf(TypeNode targetTemplate)
        {
            if ((this != CoreSystemTypes.Void) && (targetTemplate != null))
            {
                if (targetTemplate.IsStructurallyEquivalentTo((this.Template == null) ? this : this.Template) || ((this.BaseType != null) && (this.BaseType.IsAssignableToInstanceOf(targetTemplate) || ((this.BaseType.Template != null) && this.BaseType.Template.IsAssignableToInstanceOf(targetTemplate)))))
                {
                    return true;
                }
                InterfaceList interfaces = this.Interfaces;
                if (interfaces != null)
                {
                    int num = 0;
                    int count = interfaces.Count;
                    while (num < count)
                    {
                        Interface interface2 = interfaces[num];
                        if ((interface2 != null) && interface2.IsAssignableToInstanceOf(targetTemplate))
                        {
                            return true;
                        }
                        num++;
                    }
                }
            }
            return false;
        }

        public virtual bool IsAssignableToInstanceOf(TypeNode targetTemplate, out TypeNodeList templateArguments)
        {
            templateArguments = null;
            if ((this != CoreSystemTypes.Void) && (targetTemplate != null))
            {
                if (targetTemplate == this.Template)
                {
                    templateArguments = this.TemplateArguments;
                    return true;
                }
                if (((this != CoreSystemTypes.Object) && (this.BaseType != null)) && this.BaseType.IsAssignableToInstanceOf(targetTemplate, out templateArguments))
                {
                    return true;
                }
                InterfaceList interfaces = this.Interfaces;
                if (interfaces != null)
                {
                    int num = 0;
                    int count = interfaces.Count;
                    while (num < count)
                    {
                        Interface interface2 = interfaces[num];
                        if ((interface2 != null) && interface2.IsAssignableToInstanceOf(targetTemplate, out templateArguments))
                        {
                            return true;
                        }
                        num++;
                    }
                }
            }
            return false;
        }

        public static bool IsCompleteTemplate(TypeNode t)
        {
            if (t == null)
            {
                return true;
            }
            if (t.template != null)
            {
                return false;
            }
            return IsCompleteTemplate(t.DeclaringType);
        }

        public virtual bool IsDerivedFrom(TypeNode otherType)
        {
            if (otherType != null)
            {
                for (TypeNode node = this.BaseType; node != null; node = node.BaseType)
                {
                    if (node == otherType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual bool IsInheritedFrom(TypeNode otherType)
        {
            if (otherType == null)
            {
                return false;
            }
            if (this == otherType)
            {
                return true;
            }
            bool flag = false;
            if (!this.isCheckingInheritedFrom)
            {
                this.isCheckingInheritedFrom = true;
                if (this.Template != null)
                {
                    flag = this.Template.IsInheritedFrom(otherType);
                }
                else
                {
                    if (otherType.Template != null)
                    {
                        otherType = otherType.Template;
                    }
                    TypeNode baseType = this.BaseType;
                    if ((baseType != null) && baseType.IsInheritedFrom(otherType))
                    {
                        flag = true;
                    }
                    else
                    {
                        InterfaceList interfaces = this.Interfaces;
                        if (interfaces != null)
                        {
                            int num = 0;
                            int count = interfaces.Count;
                            while (num < count)
                            {
                                Interface interface2 = interfaces[num];
                                if ((interface2 != null) && interface2.IsInheritedFrom(otherType))
                                {
                                    flag = true;
                                    break;
                                }
                                num++;
                            }
                        }
                    }
                }
            }
            this.isCheckingInheritedFrom = false;
            return flag;
        }

        public virtual bool IsNestedIn(TypeNode type)
        {
            for (TypeNode node = this.DeclaringType; node != null; node = node.DeclaringType)
            {
                if (node == type)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool IsStructurallyEquivalentList(TypeNodeList list1, TypeNodeList list2)
        {
            if (list1 == null)
            {
                return (list2 == null);
            }
            if (list2 == null)
            {
                return false;
            }
            int count = list1.Count;
            if (list2.Count != count)
            {
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                TypeNode node = list1[i];
                TypeNode type = list2[i];
                if ((node == null) || (type == null))
                {
                    return false;
                }
                if (!(node == type) && !node.IsStructurallyEquivalentTo(type))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool IsStructurallyEquivalentTo(TypeNode type)
        {
            if (type == null)
            {
                return false;
            }
            if (this != type)
            {
                if ((this.Template == null) || (type.Template == null))
                {
                    if ((this == type.Template) || (this.Template == type))
                    {
                        return true;
                    }
                    Identifier identifier = (this.Template == null) ? base.Name : this.Template.Name;
                    Identifier identifier2 = (type.Template == null) ? type.Name : type.Template.Name;
                    if (((identifier == null) || (identifier2 == null)) || (identifier.UniqueIdKey != identifier2.UniqueIdKey))
                    {
                        return false;
                    }
                    if (base.NodeType != type.NodeType)
                    {
                        return false;
                    }
                    if ((this.DeclaringType == null) || (type.DeclaringType == null))
                    {
                        return false;
                    }
                }
                if ((this.TemplateArguments == null) || (type.TemplateArguments == null))
                {
                    return ((((this.DeclaringType != null) && ((this.TemplateArguments == null) || (this.TemplateArguments.Count == 0))) && ((type.TemplateArguments == null) || (type.TemplateArguments.Count == 0))) && this.DeclaringType.IsStructurallyEquivalentTo(type.DeclaringType));
                }
                int count = this.TemplateArguments.Count;
                if (count != type.TemplateArguments.Count)
                {
                    return false;
                }
                if ((this.Template != type.Template) && !this.Template.IsStructurallyEquivalentTo(type.Template))
                {
                    return false;
                }
                for (int i = 0; i < count; i++)
                {
                    TypeNode node = this.TemplateArguments[i];
                    TypeNode node2 = type.TemplateArguments[i];
                    if ((node == null) || (node2 == null))
                    {
                        return false;
                    }
                    if (!(node == node2) && !node.IsStructurallyEquivalentTo(node2))
                    {
                        return false;
                    }
                }
                if (this.DeclaringType != null)
                {
                    return this.DeclaringType.IsStructurallyEquivalentTo(type.DeclaringType);
                }
            }
            return true;
        }

        public virtual TypeNode OldGetGenericTemplateInstance(System.Compiler.Module module, TypeNodeList consolidatedArguments)
        {
            if (this.DeclaringType == null)
            {
                return this.GetTemplateInstance(module, null, null, consolidatedArguments);
            }
            TypeNodeList ownTemplateArguments = this.GetOwnTemplateArguments(consolidatedArguments);
            if (ownTemplateArguments == consolidatedArguments)
            {
                return this.GetTemplateInstance(module, null, this.DeclaringType, consolidatedArguments);
            }
            int count = consolidatedArguments.Count;
            int num2 = (ownTemplateArguments == null) ? 0 : ownTemplateArguments.Count;
            int capacity = count - num2;
            TypeNodeList list2 = new TypeNodeList(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list2.Add(consolidatedArguments[i]);
            }
            TypeNode genericTemplateInstance = this.DeclaringType.GetGenericTemplateInstance(module, list2);
            TypeNode nestedType = genericTemplateInstance.GetNestedType(base.Name);
            if (nestedType == null)
            {
                TypeNode template = genericTemplateInstance.Template;
                template.GetNestedType(base.Name);
                nestedType = new Duplicator(module, null) { 
                    RecordOriginalAsTemplate = true,
                    SkipBodies = true,
                    TypesToBeDuplicated = { [this.UniqueKey] = this }
                }.VisitTypeNode(this, null, null, null, true);
                new Specializer(module, template.ConsolidatedTemplateParameters, genericTemplateInstance.ConsolidatedTemplateArguments).VisitTypeNode(nestedType);
                nestedType.DeclaringType = genericTemplateInstance;
                genericTemplateInstance.NestedTypes.Add(nestedType);
            }
            if (num2 == 0)
            {
                return nestedType;
            }
            return nestedType.GetTemplateInstance(module, null, genericTemplateInstance, ownTemplateArguments);
        }

        public static bool operator ==(TypeNode t1, TypeNode t2)
        {
            if (t1 == null)
            {
                return (t2 == null);
            }
            return ((t2 != null) && (t1.EffectiveTypeNode == t2.EffectiveTypeNode));
        }

        public static bool operator !=(TypeNode t1, TypeNode t2) => 
            !(t1 == t2);

        public void RecordExtension(TypeNode extension)
        {
            if (this.extensions == null)
            {
                this.extensions = new TypeNodeList();
            }
            this.extensions.Add(extension);
        }

        public TypeNode SelfInstantiation()
        {
            if (this.Template == null)
            {
                TypeNodeList consolidatedTemplateParameters = this.ConsolidatedTemplateParameters;
                if ((consolidatedTemplateParameters != null) && (consolidatedTemplateParameters.Count != 0))
                {
                    return this.GetGenericTemplateInstance(this.declaringModule, consolidatedTemplateParameters);
                }
            }
            return this;
        }

        public static TypeNode StripModifier(TypeNode type, TypeNode modifier)
        {
            TypeModifier modifier2 = type as TypeModifier;
            if (modifier2 == null)
            {
                return type;
            }
            TypeNode modified = StripModifier(modifier2.ModifiedType, modifier);
            if (modifier2.Modifier == modifier)
            {
                return modified;
            }
            if (modified == modifier2.ModifiedType)
            {
                return modifier2;
            }
            if (modifier2 is OptionalModifier)
            {
                return OptionalModifier.For(modifier2.Modifier, modified);
            }
            return RequiredModifier.For(modifier2.Modifier, modified);
        }

        public static TypeNode StripModifiers(TypeNode type)
        {
            for (TypeModifier modifier = type as TypeModifier; modifier != null; modifier = type as TypeModifier)
            {
                type = modifier.ModifiedType;
            }
            return type;
        }

        public override string ToString() => 
            this.GetFullUnmangledNameWithTypeParameters();

        protected virtual TypeNode TryToFindExistingInstance(System.Compiler.Module module, Identifier uniqueMangledName) => 
            module.TryGetTemplateInstance(uniqueMangledName);

        private static bool TypeIsNotFullySpecialized(TypeNode t)
        {
            TypeNode node = StripModifiers(t);
            if (((node is TypeParameter) || (node is ClassParameter)) || node.IsNotFullySpecialized)
            {
                return true;
            }
            int num = 0;
            int num2 = (node.StructuralElementTypes == null) ? 0 : node.StructuralElementTypes.Count;
            while (num < num2)
            {
                TypeNode node2 = node.StructuralElementTypes[num];
                if ((node2 != null) && TypeIsNotFullySpecialized(node2))
                {
                    return true;
                }
                num++;
            }
            return false;
        }

        protected virtual void UpdateMemberTable(int range)
        {
            MemberList members = this.Members;
            lock (this)
            {
                if (this.memberTable == null)
                {
                    this.memberTable = new TrivialHashtable(0x20);
                }
                for (int i = this.memberCount; i < range; i++)
                {
                    Member element = members[i];
                    if ((element != null) && (element.Name != null))
                    {
                        MemberList list2 = (MemberList) this.memberTable[element.Name.UniqueIdKey];
                        if (list2 == null)
                        {
                            this.memberTable[element.Name.UniqueIdKey] = list2 = new MemberList();
                        }
                        list2.Add(element);
                    }
                }
                this.memberCount = range;
                this.constructors = null;
            }
        }

        protected static MemberList WeedOutNonSpecialMethods(MemberList members, MethodFlags mask)
        {
            if (members == null)
            {
                return null;
            }
            bool flag = true;
            int num = 0;
            int count = members.Count;
            while (num < count)
            {
                System.Compiler.Method method = members[num] as System.Compiler.Method;
                if ((method == null) || ((method.Flags & mask) == MethodFlags.CompilerControlled))
                {
                    flag = false;
                    break;
                }
                num++;
            }
            if (flag)
            {
                return members;
            }
            MemberList list = new MemberList();
            int num3 = 0;
            int num4 = members.Count;
            while (num3 < num4)
            {
                System.Compiler.Method element = members[num3] as System.Compiler.Method;
                if ((element != null) && ((element.Flags & mask) != MethodFlags.CompilerControlled))
                {
                    list.Add(element);
                }
                num3++;
            }
            return list;
        }

        public override void WriteDocumentation(XmlTextWriter xwriter)
        {
            base.WriteDocumentation(xwriter);
            MemberList members = this.Members;
            int num = 0;
            int num2 = (members == null) ? 0 : members.Count;
            while (num < num2)
            {
                Member member = members[num];
                if (member != null)
                {
                    member.WriteDocumentation(xwriter);
                }
                num++;
            }
        }

        public override AttributeList Attributes
        {
            get
            {
                if (base.attributes == null)
                {
                    TypeAttributeProvider provideTypeAttributes = this.ProvideTypeAttributes;
                    if ((provideTypeAttributes != null) && (this.ProviderHandle != null))
                    {
                        lock (System.Compiler.Module.GlobalLock)
                        {
                            if (base.attributes == null)
                            {
                                this.ProvideTypeAttributes = null;
                                provideTypeAttributes(this, this.ProviderHandle);
                            }
                            goto Label_0066;
                        }
                    }
                    base.attributes = new AttributeList(0);
                }
            Label_0066:
                return base.attributes;
            }
            set
            {
                base.attributes = value;
            }
        }

        public virtual TypeNode BaseType
        {
            get
            {
                switch (base.NodeType)
                {
                    case NodeType.ArrayType:
                        return CoreSystemTypes.Array;

                    case NodeType.Class:
                    case NodeType.ClassParameter:
                        return ((Class) this).BaseClass;

                    case NodeType.DelegateNode:
                        return CoreSystemTypes.MulticastDelegate;

                    case NodeType.EnumNode:
                        return CoreSystemTypes.Enum;

                    case NodeType.Struct:
                    case NodeType.TupleType:
                    case NodeType.TypeAlias:
                    case NodeType.TypeIntersection:
                    case NodeType.TypeUnion:
                        return CoreSystemTypes.ValueType;
                }
                return null;
            }
        }

        public int ClassSize
        {
            get => 
                this.classSize;
            set
            {
                this.classSize = value;
            }
        }

        public virtual TypeNodeList ConsolidatedTemplateArguments
        {
            get
            {
                if (this.consolidatedTemplateArguments == null)
                {
                    this.consolidatedTemplateArguments = this.GetConsolidatedTemplateArguments();
                }
                return this.consolidatedTemplateArguments;
            }
            set
            {
                this.consolidatedTemplateArguments = value;
            }
        }

        public virtual TypeNodeList ConsolidatedTemplateParameters
        {
            get
            {
                if (this.consolidatedTemplateParameters == null)
                {
                    this.consolidatedTemplateParameters = this.GetConsolidatedTemplateParameters();
                }
                return this.consolidatedTemplateParameters;
            }
            set
            {
                this.consolidatedTemplateParameters = value;
            }
        }

        public virtual Identifier ConstructorName
        {
            get
            {
                if (this.constructorName == null)
                {
                    Identifier name = base.Name;
                    if (this.IsNormalized && this.IsGeneric)
                    {
                        name = Identifier.For(this.GetUnmangledNameWithoutTypeParameters());
                    }
                    this.constructorName = name;
                }
                return this.constructorName;
            }
        }

        public TypeContract Contract
        {
            get
            {
                MemberList members = this.Members;
                return this.contract;
            }
            set
            {
                this.contract = value;
            }
        }

        public System.Compiler.Module DeclaringModule
        {
            get => 
                this.declaringModule;
            set
            {
                this.declaringModule = value;
            }
        }

        public override TypeNode DeclaringType
        {
            get
            {
                InterfaceList interfaces = this.Interfaces;
                return base.DeclaringType;
            }
            set
            {
                base.DeclaringType = value;
            }
        }

        public virtual MemberList DefaultMembers
        {
            get
            {
                int count = this.Members.Count;
                if (count != this.memberCount)
                {
                    this.UpdateMemberTable(count);
                    this.defaultMembers = null;
                }
                if (this.defaultMembers == null)
                {
                    AttributeList attributes = this.Attributes;
                    Identifier name = null;
                    int num2 = 0;
                    int num3 = (attributes == null) ? 0 : attributes.Count;
                    while (num2 < num3)
                    {
                        AttributeNode node = attributes[num2];
                        if (node != null)
                        {
                            MemberBinding constructor = node.Constructor as MemberBinding;
                            if (((constructor != null) && (constructor.BoundMember != null)) && (constructor.BoundMember.DeclaringType == SystemTypes.DefaultMemberAttribute))
                            {
                                if ((node.Expressions != null) && (node.Expressions.Count > 0))
                                {
                                    Literal literal = node.Expressions[0] as Literal;
                                    if ((literal != null) && (literal.Value is string))
                                    {
                                        name = Identifier.For((string) literal.Value);
                                    }
                                }
                                break;
                            }
                            Literal literal2 = node.Constructor as Literal;
                            if ((literal2 != null) && ((literal2.Value as TypeNode) == SystemTypes.DefaultMemberAttribute))
                            {
                                if ((node.Expressions != null) && (node.Expressions.Count > 0))
                                {
                                    Literal literal3 = node.Expressions[0] as Literal;
                                    if ((literal3 != null) && (literal3.Value is string))
                                    {
                                        name = Identifier.For((string) literal3.Value);
                                    }
                                }
                                break;
                            }
                        }
                        num2++;
                    }
                    if (name != null)
                    {
                        this.defaultMembers = this.GetMembersNamed(name);
                    }
                    else
                    {
                        this.defaultMembers = new MemberList(0);
                    }
                }
                return this.defaultMembers;
            }
            set
            {
                this.defaultMembers = value;
            }
        }

        public virtual TypeNode EffectiveTypeNode =>
            this;

        public virtual MemberList ExplicitCoercionMethods
        {
            get
            {
                if (this.Members.Count != this.memberCount)
                {
                    this.explicitCoercionMethods = null;
                }
                if (this.explicitCoercionMethods != null)
                {
                    return this.explicitCoercionMethods;
                }
                lock (this)
                {
                    if (this.explicitCoercionMethods != null)
                    {
                        return this.explicitCoercionMethods;
                    }
                    return (this.explicitCoercionMethods = WeedOutNonSpecialMethods(this.GetMembersNamed(StandardIds.opExplicit), MethodFlags.SpecialName));
                }
            }
        }

        public TypeNodeList Extensions
        {
            get
            {
                this.extensionsExamined = true;
                return this.extensions;
            }
            set
            {
                this.extensions = value;
            }
        }

        public TypeNodeList ExtensionsNoTouch =>
            this.extensions;

        public TypeFlags Flags
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
                if (this.fullName != null)
                {
                    return this.fullName;
                }
                if (this.DeclaringType != null)
                {
                    return (this.fullName = this.DeclaringType.FullName + "+" + ((base.Name == null) ? "" : base.Name.ToString()));
                }
                if ((this.Namespace != null) && (this.Namespace.UniqueIdKey != Identifier.Empty.UniqueIdKey))
                {
                    return (this.fullName = this.Namespace.ToString() + "." + ((base.Name == null) ? "" : base.Name.ToString()));
                }
                if (base.Name != null)
                {
                    return (this.fullName = base.Name.ToString());
                }
                return (this.fullName = "");
            }
        }

        public virtual string FullNameDuringParsing =>
            this.FullName;

        public virtual MemberList ImplicitCoercionMethods
        {
            get
            {
                if (this.Members.Count != this.memberCount)
                {
                    this.implicitCoercionMethods = null;
                }
                if (this.implicitCoercionMethods != null)
                {
                    return this.implicitCoercionMethods;
                }
                lock (this)
                {
                    if (this.implicitCoercionMethods != null)
                    {
                        return this.implicitCoercionMethods;
                    }
                    return (this.implicitCoercionMethods = WeedOutNonSpecialMethods(this.GetMembersNamed(StandardIds.opImplicit), MethodFlags.SpecialName));
                }
            }
        }

        public virtual InterfaceList Interfaces
        {
            get
            {
                if (this.interfaces == null)
                {
                    TypeSignatureProvider provideTypeSignature = this.ProvideTypeSignature;
                    if ((provideTypeSignature != null) && (this.ProviderHandle != null))
                    {
                        lock (System.Compiler.Module.GlobalLock)
                        {
                            if (this.interfaces == null)
                            {
                                this.ProvideTypeSignature = null;
                                provideTypeSignature(this, this.ProviderHandle);
                            }
                            goto Label_0066;
                        }
                    }
                    this.interfaces = new InterfaceList(0);
                }
            Label_0066:
                return this.interfaces;
            }
            set
            {
                this.interfaces = value;
            }
        }

        public virtual bool IsAbstract =>
            ((this.Flags & TypeFlags.Abstract) != TypeFlags.AnsiClass);

        public override bool IsAssembly
        {
            get
            {
                TypeFlags flags = this.Flags & TypeFlags.NestedFamORAssem;
                if (flags != TypeFlags.AnsiClass)
                {
                    return (flags == TypeFlags.NestedAssembly);
                }
                return true;
            }
        }

        public override bool IsCompilerControlled =>
            false;

        public override bool IsFamily =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedFamily);

        public override bool IsFamilyAndAssembly =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedFamANDAssem);

        public override bool IsFamilyOrAssembly =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedFamORAssem);

        public virtual bool IsGeneric
        {
            get => 
                this.isGeneric;
            set
            {
                this.isGeneric = value;
            }
        }

        public virtual bool IsNestedAssembly =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedAssembly);

        public virtual bool IsNestedFamily =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedFamily);

        public virtual bool IsNestedFamilyAndAssembly =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedFamANDAssem);

        public virtual bool IsNestedInternal =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedFamORAssem);

        public virtual bool IsNestedPublic =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedPublic);

        public virtual bool IsNonPublic =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.AnsiClass);

        public virtual bool IsNormalized
        {
            get
            {
                if (this.isNormalized)
                {
                    return true;
                }
                if (this.DeclaringModule == null)
                {
                    return false;
                }
                return (this.isNormalized = this.DeclaringModule.IsNormalized);
            }
            set
            {
                this.isNormalized = value;
            }
        }

        public virtual bool IsPointerType =>
            false;

        public virtual bool IsPrimitive
        {
            get
            {
                switch (this.typeCode)
                {
                    case ElementType.Boolean:
                    case ElementType.Char:
                    case ElementType.Int8:
                    case ElementType.UInt8:
                    case ElementType.Int16:
                    case ElementType.UInt16:
                    case ElementType.Int32:
                    case ElementType.UInt32:
                    case ElementType.Int64:
                    case ElementType.UInt64:
                    case ElementType.Single:
                    case ElementType.Double:
                    case ElementType.String:
                    case ElementType.IntPtr:
                    case ElementType.UIntPtr:
                        return true;
                }
                return false;
            }
        }

        public virtual bool IsPrimitiveComparable
        {
            get
            {
                switch (this.typeCode)
                {
                    case ElementType.Boolean:
                    case ElementType.Char:
                    case ElementType.Int8:
                    case ElementType.UInt8:
                    case ElementType.Int16:
                    case ElementType.UInt16:
                    case ElementType.Int32:
                    case ElementType.UInt32:
                    case ElementType.Int64:
                    case ElementType.UInt64:
                    case ElementType.Single:
                    case ElementType.Double:
                    case ElementType.IntPtr:
                    case ElementType.UIntPtr:
                        return true;
                }
                if ((this is Struct) && !(this is EnumNode))
                {
                    return (this is System.Compiler.Pointer);
                }
                return true;
            }
        }

        public virtual bool IsPrimitiveInteger
        {
            get
            {
                switch (this.typeCode)
                {
                    case ElementType.Int8:
                    case ElementType.UInt8:
                    case ElementType.Int16:
                    case ElementType.UInt16:
                    case ElementType.Int32:
                    case ElementType.UInt32:
                    case ElementType.Int64:
                    case ElementType.UInt64:
                    case ElementType.IntPtr:
                    case ElementType.UIntPtr:
                        return true;
                }
                return false;
            }
        }

        public virtual bool IsPrimitiveNumeric
        {
            get
            {
                switch (this.typeCode)
                {
                    case ElementType.Int8:
                    case ElementType.UInt8:
                    case ElementType.Int16:
                    case ElementType.UInt16:
                    case ElementType.Int32:
                    case ElementType.UInt32:
                    case ElementType.Int64:
                    case ElementType.UInt64:
                    case ElementType.Single:
                    case ElementType.Double:
                    case ElementType.IntPtr:
                    case ElementType.UIntPtr:
                        return true;
                }
                return false;
            }
        }

        public virtual bool IsPrimitiveUnsignedInteger
        {
            get
            {
                switch (this.typeCode)
                {
                    case ElementType.UInt8:
                    case ElementType.UInt16:
                    case ElementType.UInt32:
                    case ElementType.UInt64:
                    case ElementType.UIntPtr:
                        return true;
                }
                return false;
            }
        }

        public override bool IsPrivate =>
            ((this.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.NestedPrivate);

        public override bool IsPublic
        {
            get
            {
                TypeFlags flags = this.Flags & TypeFlags.NestedFamORAssem;
                if (flags != TypeFlags.Public)
                {
                    return (flags == TypeFlags.NestedPublic);
                }
                return true;
            }
        }

        public virtual bool IsReferenceType
        {
            get
            {
                NodeType nodeType = base.NodeType;
                if (nodeType <= NodeType.DelegateNode)
                {
                    switch (nodeType)
                    {
                        case NodeType.Class:
                        case NodeType.DelegateNode:
                        case NodeType.ArrayType:
                            goto Label_004D;
                    }
                    goto Label_006B;
                }
                if ((nodeType != NodeType.Interface) && (nodeType != NodeType.Pointer))
                {
                    goto Label_006B;
                }
            Label_004D:
                if (this != SystemTypes.ValueType)
                {
                    return (this != SystemTypes.Enum);
                }
                return false;
            Label_006B:
                return false;
            }
        }

        public virtual bool IsSealed =>
            ((this.Flags & TypeFlags.Sealed) != TypeFlags.AnsiClass);

        public override bool IsSpecialName =>
            ((this.Flags & TypeFlags.SpecialName) != TypeFlags.AnsiClass);

        public override bool IsStatic =>
            true;

        public virtual bool IsStructural =>
            (this.Template != null);

        public virtual bool IsTemplateParameter =>
            false;

        public virtual bool IsUnmanaged =>
            false;

        public virtual bool IsUnsignedPrimitiveNumeric
        {
            get
            {
                switch (this.typeCode)
                {
                    case ElementType.UInt8:
                    case ElementType.UInt16:
                    case ElementType.UInt32:
                    case ElementType.UInt64:
                    case ElementType.UIntPtr:
                        return true;
                }
                return false;
            }
        }

        public virtual bool IsValueType
        {
            get
            {
                switch (base.NodeType)
                {
                    case NodeType.ConstrainedType:
                    case NodeType.TupleType:
                    case NodeType.TypeAlias:
                    case NodeType.TypeIntersection:
                    case NodeType.TypeUnion:
                    case NodeType.EnumNode:
                        return true;

                    case NodeType.Struct:
                        return true;
                }
                return false;
            }
        }

        public override bool IsVisibleOutsideAssembly
        {
            get
            {
                if ((this.DeclaringType == null) || this.DeclaringType.IsVisibleOutsideAssembly)
                {
                    switch ((this.Flags & TypeFlags.NestedFamORAssem))
                    {
                        case TypeFlags.Public:
                        case TypeFlags.NestedPublic:
                            return true;

                        case TypeFlags.NestedFamily:
                        case TypeFlags.NestedFamORAssem:
                            if (this.DeclaringType == null)
                            {
                                return false;
                            }
                            return !this.DeclaringType.IsSealed;
                    }
                }
                return false;
            }
        }

        public virtual MemberList Members
        {
            get
            {
                if ((this.members == null) || this.membersBeingPopulated)
                {
                    if ((this.ProvideTypeMembers != null) && (this.ProviderHandle != null))
                    {
                        lock (System.Compiler.Module.GlobalLock)
                        {
                            if (this.members == null)
                            {
                                this.membersBeingPopulated = true;
                                this.ProvideTypeMembers(this, this.ProviderHandle);
                                this.membersBeingPopulated = false;
                            }
                            goto Label_0080;
                        }
                    }
                    this.members = new MemberList();
                }
            Label_0080:
                return this.members;
            }
            set
            {
                this.members = value;
                this.memberCount = 0;
                this.memberTable = null;
                this.constructors = null;
                this.defaultMembers = null;
                this.explicitCoercionFromTable = null;
                this.explicitCoercionMethods = null;
                this.explicitCoercionToTable = null;
                this.implicitCoercionFromTable = null;
                this.implicitCoercionMethods = null;
                this.implicitCoercionToTable = null;
                this.opFalse = null;
                this.opTrue = null;
            }
        }

        public Identifier Namespace
        {
            get => 
                this.@namespace;
            set
            {
                this.@namespace = value;
            }
        }

        public virtual TypeNodeList NestedTypes
        {
            get
            {
                if ((this.nestedTypes != null) && ((this.members == null) || (this.members.Count == this.memberCount)))
                {
                    return this.nestedTypes;
                }
                if ((this.ProvideNestedTypes != null) && (this.ProviderHandle != null))
                {
                    lock (System.Compiler.Module.GlobalLock)
                    {
                        this.ProvideNestedTypes(this, this.ProviderHandle);
                        goto Label_00CE;
                    }
                }
                MemberList members = this.Members;
                TypeNodeList list2 = new TypeNodeList();
                int num = 0;
                int num2 = (members == null) ? 0 : members.Count;
                while (num < num2)
                {
                    TypeNode element = members[num] as TypeNode;
                    if (element != null)
                    {
                        list2.Add(element);
                    }
                    num++;
                }
                this.nestedTypes = list2;
            Label_00CE:
                return this.nestedTypes;
            }
            set
            {
                this.nestedTypes = value;
            }
        }

        public int PackingSize
        {
            get => 
                this.packingSize;
            set
            {
                this.packingSize = value;
            }
        }

        public SecurityAttributeList SecurityAttributes
        {
            get
            {
                if (this.securityAttributes != null)
                {
                    return this.securityAttributes;
                }
                if (base.attributes == null)
                {
                    if (this.Attributes != null)
                    {
                    }
                    if (this.securityAttributes != null)
                    {
                        return this.securityAttributes;
                    }
                }
                return (this.securityAttributes = new SecurityAttributeList(0));
            }
            set
            {
                this.securityAttributes = value;
            }
        }

        public virtual TypeNodeList StructuralElementTypes
        {
            get
            {
                TypeNodeList templateArguments = this.TemplateArguments;
                if ((templateArguments != null) && (templateArguments.Count > 0))
                {
                    return templateArguments;
                }
                return this.TemplateParameters;
            }
        }

        public virtual TypeNode Template
        {
            get
            {
                TypeNode template = this.template;
                if (template == null)
                {
                    if (this.isGeneric || (TargetPlatform.GenericTypeNamesMangleChar != '_'))
                    {
                        return null;
                    }
                    AttributeList attributes = this.Attributes;
                    lock (this)
                    {
                        if (this.template != null)
                        {
                            if (this.template == NotSpecified)
                            {
                                return null;
                            }
                            return this.template;
                        }
                        if (template == null)
                        {
                            this.template = NotSpecified;
                        }
                        return template;
                    }
                }
                if (template == NotSpecified)
                {
                    return null;
                }
                return template;
            }
            set
            {
                this.template = value;
            }
        }

        public virtual TypeNodeList TemplateArguments
        {
            get
            {
                if ((this.template == null) && (this.Template != null))
                {
                }
                return this.templateArguments;
            }
            set
            {
                this.templateArguments = value;
            }
        }

        public TypeNodeList TemplateInstances
        {
            get => 
                this.templateInstances;
            set
            {
                this.templateInstances = value;
            }
        }

        public virtual TypeNodeList TemplateParameters
        {
            get
            {
                InterfaceList interfaces = this.Interfaces;
                return this.templateParameters;
            }
            set
            {
                if (value == null)
                {
                    if (this.templateParameters == null)
                    {
                        return;
                    }
                    if (this.templateParameters.Count > 0)
                    {
                        value = new TypeNodeList(0);
                    }
                }
                this.templateParameters = value;
            }
        }

        public virtual System.TypeCode TypeCode
        {
            get
            {
                switch (this.typeCode)
                {
                    case ElementType.Void:
                        return System.TypeCode.Empty;

                    case ElementType.Boolean:
                        return System.TypeCode.Boolean;

                    case ElementType.Char:
                        return System.TypeCode.Char;

                    case ElementType.Int8:
                        return System.TypeCode.SByte;

                    case ElementType.UInt8:
                        return System.TypeCode.Byte;

                    case ElementType.Int16:
                        return System.TypeCode.Int16;

                    case ElementType.UInt16:
                        return System.TypeCode.UInt16;

                    case ElementType.Int32:
                        return System.TypeCode.Int32;

                    case ElementType.UInt32:
                        return System.TypeCode.UInt32;

                    case ElementType.Int64:
                        return System.TypeCode.Int64;

                    case ElementType.UInt64:
                        return System.TypeCode.UInt64;

                    case ElementType.Single:
                        return System.TypeCode.Single;

                    case ElementType.Double:
                        return System.TypeCode.Double;
                }
                if (this == CoreSystemTypes.String)
                {
                    return System.TypeCode.String;
                }
                if (this == CoreSystemTypes.Decimal)
                {
                    return System.TypeCode.Decimal;
                }
                if (this == CoreSystemTypes.DateTime)
                {
                    return System.TypeCode.DateTime;
                }
                if (this == CoreSystemTypes.DBNull)
                {
                    return System.TypeCode.DBNull;
                }
                return System.TypeCode.Object;
            }
        }

        private class CachingModuleForGenericsInstances : System.Compiler.Module
        {
            public override TypeNode GetStructurallyEquivalentType(Identifier ns, Identifier id, Identifier uniqueMangledName, bool lookInReferencedAssemblies)
            {
                if (uniqueMangledName == null)
                {
                    return null;
                }
                return (TypeNode) base.StructurallyEquivalentType[uniqueMangledName.UniqueIdKey];
            }
        }

        public delegate void NestedTypeProvider(TypeNode type, object handle);

        public delegate void TypeAttributeProvider(TypeNode type, object handle);

        public delegate void TypeMemberProvider(TypeNode type, object handle);

        public delegate void TypeSignatureProvider(TypeNode type, object handle);
    }
}

