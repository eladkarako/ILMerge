namespace System.Compiler
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Xml;

    internal abstract class Member : Node
    {
        protected AttributeList attributes;
        public System.Compiler.Namespace DeclaringNamespace;
        private TypeNode declaringType;
        protected XmlNode documentation;
        protected Identifier documentationId;
        public Node DocumentationNode;
        private int filterPriority;
        protected string helpText;
        protected Member hiddenMember;
        protected bool hidesBaseClassMember;
        protected bool hidesBaseClassMemberSpecifiedExplicitly;
        public bool IsUnsafe;
        private Identifier name;
        private bool notObsolete;
        private System.ObsoleteAttribute obsoleteAttribute;
        protected Member overriddenMember;
        protected bool overridesBaseClassMember;
        protected bool overridesBaseClassMemberSpecifiedExplicitly;
        public NodeList References;
        private static readonly char[] tags = new char[] { 'E', 'F', 'M', 'P', 'T' };

        protected Member(NodeType nodeType) : base(nodeType)
        {
        }

        protected Member(TypeNode declaringType, AttributeList attributes, Identifier name, NodeType nodeType) : base(nodeType)
        {
            this.attributes = attributes;
            this.declaringType = declaringType;
            this.name = name;
        }

        private void AppendValue(StringBuilder sb, XmlNode node)
        {
            string name = node.Value;
            if (name != null)
            {
                name = name.Trim();
                if (((name.Length > 2) && (name[1] == ':')) && (name.LastIndexOfAny(tags, 0, 1) == 0))
                {
                    char ch = name[0];
                    name = name.Substring(2);
                    if ((ch == 'T') && (name.IndexOf(TargetPlatform.GenericTypeNamesMangleChar) >= 0))
                    {
                        Module declaringModule = null;
                        if (this.DeclaringType != null)
                        {
                            declaringModule = this.DeclaringType.DeclaringModule;
                        }
                        else if (this is TypeNode)
                        {
                            declaringModule = ((TypeNode) this).DeclaringModule;
                        }
                        if (declaringModule != null)
                        {
                            Identifier empty;
                            Identifier identifier2;
                            int length = name.LastIndexOf('.');
                            if ((length < 0) || (length >= name.Length))
                            {
                                empty = Identifier.Empty;
                                identifier2 = Identifier.For(name);
                            }
                            else
                            {
                                empty = Identifier.For(name.Substring(0, length));
                                identifier2 = Identifier.For(name.Substring(length + 1));
                            }
                            TypeNode node2 = declaringModule.GetType(empty, identifier2, true);
                            if (node2 != null)
                            {
                                name = node2.GetFullUnmangledNameWithTypeParameters();
                            }
                        }
                    }
                }
                if ((name != null) && (name.Length != 0))
                {
                    bool flag = false;
                    if (((sb.Length > 0) && !char.IsPunctuation(name[0])) && !char.IsWhiteSpace(name[0]))
                    {
                        sb.Append(' ');
                        flag = true;
                    }
                    foreach (char ch2 in name)
                    {
                        if (char.IsWhiteSpace(ch2))
                        {
                            if (!flag)
                            {
                                flag = true;
                                sb.Append(' ');
                            }
                        }
                        else
                        {
                            flag = false;
                            sb.Append(ch2);
                        }
                    }
                    if ((sb.Length > 0) && char.IsWhiteSpace(sb[sb.Length - 1]))
                    {
                        sb.Length--;
                    }
                }
            }
        }

        public virtual AttributeNode GetAttribute(TypeNode attributeType)
        {
            if (attributeType != null)
            {
                AttributeList attributes = this.Attributes;
                int num = 0;
                int num2 = (attributes == null) ? 0 : attributes.Count;
                while (num < num2)
                {
                    AttributeNode node = attributes[num];
                    if (node != null)
                    {
                        MemberBinding constructor = node.Constructor as MemberBinding;
                        if (constructor != null)
                        {
                            if ((constructor.BoundMember != null) && !(constructor.BoundMember.DeclaringType != attributeType))
                            {
                                return node;
                            }
                        }
                        else
                        {
                            Literal literal = node.Constructor as Literal;
                            if ((literal != null) && !((literal.Value as TypeNode) != attributeType))
                            {
                                return node;
                            }
                        }
                    }
                    num++;
                }
            }
            return null;
        }

        protected virtual Identifier GetDocumentationId() => 
            Identifier.Empty;

        public virtual AttributeList GetFilteredAttributes(TypeNode attributeType)
        {
            if (attributeType == null)
            {
                return this.Attributes;
            }
            AttributeList attributes = this.Attributes;
            AttributeList list2 = new AttributeList();
            int num = 0;
            int num2 = (attributes == null) ? 0 : attributes.Count;
            while (num < num2)
            {
                AttributeNode element = attributes[num];
                if (element != null)
                {
                    MemberBinding constructor = element.Constructor as MemberBinding;
                    if (constructor != null)
                    {
                        if ((constructor.BoundMember == null) || (constructor.BoundMember.DeclaringType != attributeType))
                        {
                            list2.Add(element);
                        }
                    }
                    else
                    {
                        Literal literal = element.Constructor as Literal;
                        if ((literal == null) || ((literal.Value as TypeNode) != attributeType))
                        {
                            list2.Add(element);
                        }
                    }
                }
                num++;
            }
            return list2;
        }

        private string GetHelpText(XmlNode node)
        {
            if (node == null)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            if (node.HasChildNodes)
            {
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.NodeType)
                    {
                        case XmlNodeType.Element:
                        {
                            string helpText = this.GetHelpText(node2);
                            if ((helpText != null) && (helpText.Length != 0))
                            {
                                if ((sb.Length > 0) && !char.IsPunctuation(helpText[0]))
                                {
                                    sb.Append(' ');
                                }
                                sb.Append(helpText);
                            }
                            break;
                        }
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Entity:
                            this.AppendValue(sb, node2);
                            break;
                    }
                }
            }
            else if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    this.AppendValue(sb, attribute);
                }
            }
            return sb.ToString();
        }

        public virtual string GetParameterHelpText(string parameterName)
        {
            XmlNode documentation = this.Documentation;
            if ((documentation != null) && (documentation.ChildNodes != null))
            {
                foreach (XmlNode node2 in documentation.ChildNodes)
                {
                    if (((node2 != null) && (node2.Name == "param")) && (node2.Attributes != null))
                    {
                        foreach (XmlAttribute attribute in node2.Attributes)
                        {
                            if (((attribute != null) && (attribute.Name == "name")) && ((attribute.Value == parameterName) && node2.HasChildNodes))
                            {
                                return this.GetHelpText(node2);
                            }
                        }
                    }
                }
            }
            return null;
        }

        public virtual void WriteDocumentation(XmlTextWriter xwriter)
        {
            if ((this.documentation != null) && (xwriter != null))
            {
                xwriter.WriteStartElement("member");
                if (this.DocumentationId != null)
                {
                    xwriter.WriteAttributeString("name", this.DocumentationId.ToString());
                    this.documentation.WriteContentTo(xwriter);
                    xwriter.WriteEndElement();
                }
            }
        }

        public virtual AttributeList Attributes
        {
            get
            {
                if (this.attributes != null)
                {
                    return this.attributes;
                }
                return (this.attributes = new AttributeList());
            }
            set
            {
                this.attributes = value;
            }
        }

        public virtual TypeNode DeclaringType
        {
            get => 
                this.declaringType;
            set
            {
                this.declaringType = value;
            }
        }

        public virtual XmlNode Documentation
        {
            get
            {
                XmlNode documentation = this.documentation;
                if (documentation != null)
                {
                    return documentation;
                }
                TypeNode declaringType = this.DeclaringType;
                if (declaringType == null)
                {
                    declaringType = this as TypeNode;
                }
                TrivialHashtable memberDocumentationCache = declaringType?.DeclaringModule?.GetMemberDocumentationCache();
                if (memberDocumentationCache == null)
                {
                    return null;
                }
                return (this.documentation = (XmlNode) memberDocumentationCache[this.DocumentationId.UniqueIdKey]);
            }
            set
            {
                this.documentation = value;
            }
        }

        public Identifier DocumentationId
        {
            get
            {
                Identifier identifier2;
                Identifier documentationId = this.documentationId;
                if (documentationId != null)
                {
                    return documentationId;
                }
                this.DocumentationId = identifier2 = this.GetDocumentationId();
                return identifier2;
            }
            set
            {
                this.documentationId = value;
            }
        }

        public virtual EditorBrowsableState FilterPriority
        {
            get
            {
                if (this.filterPriority > 0)
                {
                    return (EditorBrowsableState) (this.filterPriority - 1);
                }
                int result = 0;
                XmlNode documentation = this.Documentation;
                if ((documentation != null) && documentation.HasChildNodes)
                {
                    foreach (XmlNode node2 in documentation.ChildNodes)
                    {
                        if (node2.Name != "filterpriority")
                        {
                            continue;
                        }
                        PlatformHelpers.TryParseInt32(node2.InnerText, out result);
                        switch (result)
                        {
                            case 2:
                                this.filterPriority = 2;
                                break;

                            case 3:
                                this.filterPriority = 1;
                                break;

                            default:
                                this.filterPriority = 0;
                                break;
                        }
                        this.filterPriority++;
                        return (EditorBrowsableState) (this.filterPriority - 1);
                    }
                }
                AttributeList attributes = this.Attributes;
                int num3 = 0;
                int num4 = (attributes == null) ? 0 : attributes.Count;
                while (num3 < num4)
                {
                    AttributeNode node3 = attributes[num3];
                    if ((((node3 != null) && (node3.Type != null)) && ((node3.Expressions != null) && (node3.Expressions.Count >= 1))) && (node3.Type.FullName == "System.ComponentModel.EditorBrowsableAttribute"))
                    {
                        Literal literal = node3.Expressions[0] as Literal;
                        if ((literal != null) && (literal.Value is int))
                        {
                            result = (int) literal.Value;
                            return (EditorBrowsableState) ((this.filterPriority = result + 1) - 1);
                        }
                    }
                    num3++;
                }
                return (EditorBrowsableState) ((this.filterPriority = 1) - 1);
            }
            set
            {
                this.filterPriority = ((int) value) + 1;
            }
        }

        public abstract string FullName { get; }

        public virtual string HelpText
        {
            get
            {
                string helpText = this.helpText;
                if (helpText != null)
                {
                    return helpText;
                }
                XmlNode documentation = this.Documentation;
                if ((documentation != null) && documentation.HasChildNodes)
                {
                    foreach (XmlNode node2 in documentation.ChildNodes)
                    {
                        if (node2.Name == "summary")
                        {
                            return (this.helpText = this.GetHelpText(node2));
                        }
                    }
                }
                return (this.helpText = "");
            }
            set
            {
                this.helpText = value;
            }
        }

        public virtual Member HiddenMember
        {
            get => 
                this.hiddenMember;
            set
            {
                this.hiddenMember = value;
            }
        }

        public bool HidesBaseClassMember
        {
            get
            {
                if (this.hidesBaseClassMemberSpecifiedExplicitly)
                {
                    return this.hidesBaseClassMember;
                }
                return (this.HiddenMember != null);
            }
            set
            {
                this.hidesBaseClassMember = value;
                this.hidesBaseClassMemberSpecifiedExplicitly = true;
            }
        }

        public abstract bool IsAssembly { get; }

        public abstract bool IsCompilerControlled { get; }

        public abstract bool IsFamily { get; }

        public abstract bool IsFamilyAndAssembly { get; }

        public abstract bool IsFamilyOrAssembly { get; }

        public abstract bool IsPrivate { get; }

        public abstract bool IsPublic { get; }

        public abstract bool IsSpecialName { get; }

        public abstract bool IsStatic { get; }

        public abstract bool IsVisibleOutsideAssembly { get; }

        public Identifier Name
        {
            get => 
                this.name;
            set
            {
                this.name = value;
            }
        }

        public System.ObsoleteAttribute ObsoleteAttribute
        {
            get
            {
                if (this.notObsolete)
                {
                    return null;
                }
                if (this.obsoleteAttribute == null)
                {
                    AttributeNode attribute = this.GetAttribute(SystemTypes.ObsoleteAttribute);
                    if (attribute != null)
                    {
                        ExpressionList expressions = attribute.Expressions;
                        int num = (expressions == null) ? 0 : expressions.Count;
                        Literal literal = (num > 0) ? (expressions[0] as Literal) : null;
                        Literal literal2 = (num > 1) ? (expressions[1] as Literal) : null;
                        string message = (literal != null) ? (literal.Value as string) : null;
                        object obj2 = literal2?.Value;
                        if (obj2 is bool)
                        {
                            return (this.obsoleteAttribute = new System.ObsoleteAttribute(message, (bool) obj2));
                        }
                        return (this.obsoleteAttribute = new System.ObsoleteAttribute(message));
                    }
                    this.notObsolete = true;
                }
                return this.obsoleteAttribute;
            }
            set
            {
                this.obsoleteAttribute = value;
                this.notObsolete = false;
            }
        }

        public virtual Member OverriddenMember
        {
            get => 
                this.overriddenMember;
            set
            {
                this.overriddenMember = value;
            }
        }

        public virtual bool OverridesBaseClassMember
        {
            get
            {
                if (this.overridesBaseClassMemberSpecifiedExplicitly)
                {
                    return this.overridesBaseClassMember;
                }
                return (this.OverriddenMember != null);
            }
            set
            {
                this.overridesBaseClassMember = value;
                this.overridesBaseClassMemberSpecifiedExplicitly = true;
            }
        }
    }
}

