namespace System.Compiler
{
    using System;
    using System.Reflection;
    using System.Text;

    internal class Event : Member
    {
        public Field BackingField;
        protected EventInfo eventInfo;
        private EventFlags flags;
        protected string fullName;
        private Method handlerAdder;
        private Method handlerCaller;
        private MethodFlags handlerFlags;
        private Method handlerRemover;
        private TypeNode handlerType;
        public TypeNode HandlerTypeExpression;
        protected Property hiddenEvent;
        public TypeNodeList ImplementedTypeExpressions;
        public TypeNodeList ImplementedTypes;
        public Expression InitialHandler;
        public static readonly Event NotSpecified = new Event();
        private MethodList otherMethods;
        protected Property overriddenEvent;

        public Event() : base(NodeType.Event)
        {
        }

        public Event(TypeNode declaringType, AttributeList attributes, EventFlags flags, Identifier name, Method handlerAdder, Method handlerCaller, Method handlerRemover, TypeNode handlerType) : base(declaringType, attributes, name, NodeType.Event)
        {
            this.Flags = flags;
            this.HandlerAdder = handlerAdder;
            this.HandlerCaller = handlerCaller;
            this.HandlerRemover = handlerRemover;
            this.HandlerType = handlerType;
        }

        protected override Identifier GetDocumentationId()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("E:");
            if (this.DeclaringType == null)
            {
                return Identifier.Empty;
            }
            this.DeclaringType.AppendDocumentIdMangledName(sb, null, null);
            sb.Append(".");
            if (base.Name == null)
            {
                return Identifier.Empty;
            }
            sb.Append(base.Name.Name);
            return Identifier.For(sb.ToString());
        }

        public static Event GetEvent(EventInfo eventInfo)
        {
            if (eventInfo == null)
            {
                return null;
            }
            return TypeNode.GetTypeNode(eventInfo.DeclaringType)?.GetEvent(Identifier.For(eventInfo.Name));
        }

        public virtual EventInfo GetEventInfo()
        {
            if (this.eventInfo == null)
            {
                TypeNode declaringType = this.DeclaringType;
                if (declaringType == null)
                {
                    return null;
                }
                Type runtimeType = declaringType.GetRuntimeType();
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
                this.eventInfo = runtimeType.GetEvent(base.Name.ToString(), declaredOnly);
            }
            return this.eventInfo;
        }

        public EventFlags Flags
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

        public Method HandlerAdder
        {
            get => 
                this.handlerAdder;
            set
            {
                this.handlerAdder = value;
            }
        }

        public Method HandlerCaller
        {
            get => 
                this.handlerCaller;
            set
            {
                this.handlerCaller = value;
            }
        }

        public MethodFlags HandlerFlags
        {
            get => 
                this.handlerFlags;
            set
            {
                this.handlerFlags = value;
            }
        }

        public Method HandlerRemover
        {
            get => 
                this.handlerRemover;
            set
            {
                this.handlerRemover = value;
            }
        }

        public TypeNode HandlerType
        {
            get => 
                this.handlerType;
            set
            {
                this.handlerType = value;
            }
        }

        public virtual Event HiddenEvent
        {
            get
            {
                if (base.hiddenMember == NotSpecified)
                {
                    return null;
                }
                Event hiddenMember = base.hiddenMember as Event;
                if (hiddenMember == null)
                {
                    Method hiddenMethod = this.HandlerAdder?.HiddenMethod;
                    Method method2 = this.HandlerCaller?.HiddenMethod;
                    Method method3 = this.HandlerRemover?.HiddenMethod;
                    Event event3 = (hiddenMethod == null) ? null : (hiddenMethod.DeclaringMember as Event);
                    Event event4 = (method2 == null) ? null : (method2.DeclaringMember as Event);
                    Event event5 = (method3 == null) ? null : (method3.DeclaringMember as Event);
                    hiddenMember = event3;
                    if ((event4 != null) && ((hiddenMember == null) || ((event4.DeclaringType != null) && event4.DeclaringType.IsDerivedFrom(hiddenMember.DeclaringType))))
                    {
                        hiddenMember = event4;
                    }
                    if ((event5 != null) && ((hiddenMember == null) || ((event5.DeclaringType != null) && event5.DeclaringType.IsDerivedFrom(hiddenMember.DeclaringType))))
                    {
                        hiddenMember = event5;
                    }
                    if (hiddenMember == null)
                    {
                        base.hiddenMember = NotSpecified;
                        return null;
                    }
                    base.hiddenMember = hiddenMember;
                }
                return hiddenMember;
            }
            set
            {
                base.hiddenMember = value;
            }
        }

        public override Member HiddenMember
        {
            get => 
                this.HiddenEvent;
            set
            {
                this.HiddenEvent = (Event) value;
            }
        }

        public bool IsAbstract =>
            ((this.HandlerFlags & MethodFlags.Abstract) != MethodFlags.CompilerControlled);

        public override bool IsAssembly =>
            ((this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.Assembly);

        public override bool IsCompilerControlled =>
            ((this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.CompilerControlled);

        public override bool IsFamily =>
            ((this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.Family);

        public override bool IsFamilyAndAssembly =>
            ((this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.FamANDAssem);

        public override bool IsFamilyOrAssembly =>
            ((this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.FamORAssem);

        public bool IsFinal =>
            ((this.HandlerFlags & MethodFlags.Final) != MethodFlags.CompilerControlled);

        public override bool IsPrivate =>
            ((this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.Private);

        public override bool IsPublic =>
            ((this.HandlerFlags & MethodFlags.MethodAccessMask) == MethodFlags.Public);

        public override bool IsSpecialName =>
            ((this.Flags & EventFlags.SpecialName) != EventFlags.None);

        public override bool IsStatic =>
            ((this.HandlerFlags & MethodFlags.Static) != MethodFlags.CompilerControlled);

        public bool IsVirtual =>
            ((this.HandlerFlags & MethodFlags.Virtual) != MethodFlags.CompilerControlled);

        public override bool IsVisibleOutsideAssembly
        {
            get
            {
                if (((this.HandlerAdder == null) || !this.HandlerAdder.IsVisibleOutsideAssembly) && ((this.HandlerCaller == null) || !this.HandlerCaller.IsVisibleOutsideAssembly))
                {
                    return ((this.HandlerRemover != null) && this.HandlerRemover.IsVisibleOutsideAssembly);
                }
                return true;
            }
        }

        public MethodList OtherMethods
        {
            get => 
                this.otherMethods;
            set
            {
                this.otherMethods = value;
            }
        }

        public virtual Event OverriddenEvent
        {
            get
            {
                if (base.overriddenMember == NotSpecified)
                {
                    return null;
                }
                Event overriddenMember = base.overriddenMember as Event;
                if (overriddenMember == null)
                {
                    Method overriddenMethod = this.HandlerAdder?.OverriddenMethod;
                    Method method2 = this.HandlerCaller?.OverriddenMethod;
                    Method method3 = this.HandlerRemover?.OverriddenMethod;
                    Event event3 = (overriddenMethod == null) ? null : (overriddenMethod.DeclaringMember as Event);
                    Event event4 = (method2 == null) ? null : (method2.DeclaringMember as Event);
                    Event event5 = (method3 == null) ? null : (method3.DeclaringMember as Event);
                    overriddenMember = event3;
                    if ((event4 != null) && ((overriddenMember == null) || ((event4.DeclaringType != null) && event4.DeclaringType.IsDerivedFrom(overriddenMember.DeclaringType))))
                    {
                        overriddenMember = event4;
                    }
                    if ((event5 != null) && ((overriddenMember == null) || ((event5.DeclaringType != null) && event5.DeclaringType.IsDerivedFrom(overriddenMember.DeclaringType))))
                    {
                        overriddenMember = event5;
                    }
                    if (overriddenMember == null)
                    {
                        base.overriddenMember = NotSpecified;
                        return null;
                    }
                    base.overriddenMember = overriddenMember;
                }
                return overriddenMember;
            }
            set
            {
                base.overriddenMember = value;
            }
        }

        public override Member OverriddenMember
        {
            get => 
                this.OverriddenEvent;
            set
            {
                this.OverriddenEvent = (Event) value;
            }
        }
    }
}

