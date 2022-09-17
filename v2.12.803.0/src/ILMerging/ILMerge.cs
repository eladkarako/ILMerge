namespace ILMerging
{
    using AssemblyResolving;
    using System;
    using System.Collections;
    using System.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class ILMerge
    {
        private bool allowAllDuplicates;
        private bool allowMultipleAssemblyLevelAttributes;
        private bool allowWildCards;
        private bool allowZeroPeKind;
        private ArrayList assemblyNames;
        private AssemblyNode attributeAssembly;
        private string attributeFileName;
        private bool closed;
        protected string clrDir;
        protected string clrVersion;
        private bool copyattrs;
        private bool copyXmlDocumentation;
        private Duplicator d;
        private bool debugInfo = true;
        private bool delaySign;
        private string[] directories;
        private string excludeFile = "";
        private ArrayList exemptionList;
        private int fileAlignment = 0x200;
        private Class hiddenClass;
        private bool internalize;
        private bool keepFirstOfMultipleAssemblyLevelAttributes;
        private string keyfile;
        private bool keyfileSpecified;
        private bool log;
        private string logFile;
        private ArrayList memberList;
        private string outputFileName;
        private ArrayList resourceList = new ArrayList();
        private ArrayList searchDirs = new ArrayList();
        private bool shortB;
        private bool strongNameLost;
        private AssemblyNode targetAssembly;
        private Kind targetKind = Kind.SameAsPrimaryAssembly;
        protected bool targetPlatformSpecified;
        private Hashtable typeList = new Hashtable();
        private Hashtable typesToAllowDuplicatesOf = new Hashtable();
        private bool unionMerge;
        private bool usePublicKeyTokensForAssemblyReferences = true;
        private System.Version version;

        private void AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(TypeNode t, AttributeNode assemblyComVisibleAttribute)
        {
            if (((assemblyComVisibleAttribute != null) && t.IsPublic) && (t.GetAttribute(SystemTypes.ComVisibleAttribute) == null))
            {
                t.Attributes.Add(assemblyComVisibleAttribute);
            }
            int num = 0;
            int count = t.Members.Count;
            while (num < count)
            {
                TypeNode node = t.Members[num] as TypeNode;
                if (node != null)
                {
                    this.AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(node, null);
                }
                else
                {
                    System.Compiler.Method meth = t.Members[num] as System.Compiler.Method;
                    if (meth != null)
                    {
                        bool flag = (meth.Flags & MethodFlags.NewSlot) != MethodFlags.CompilerControlled;
                        if ((meth.IsVirtual && meth.IsFamily) && !flag)
                        {
                            for (TypeNode node2 = t.BaseType; node2 != null; node2 = node2.BaseType)
                            {
                                System.Compiler.Method implementingMethod = node2.GetImplementingMethod(meth, false);
                                if (((implementingMethod != null) && implementingMethod.IsFamilyOrAssembly) && (implementingMethod.DeclaringType.DeclaringModule == meth.DeclaringType.DeclaringModule))
                                {
                                    meth.Flags |= MethodFlags.FamORAssem;
                                    break;
                                }
                            }
                        }
                    }
                }
                num++;
            }
        }

        public void AllowDuplicateType(string typeName)
        {
            if (typeName == null)
            {
                this.allowAllDuplicates = true;
            }
            else
            {
                this.typesToAllowDuplicatesOf[typeName] = true;
            }
        }

        private bool AttributeExistsInTarget(AttributeNode possiblyDuplicateAttr, AttributeList targetList)
        {
            bool flag = false;
            int num = 0;
            while (num < targetList.Count)
            {
                if (possiblyDuplicateAttr.Type == targetList[num].Type)
                {
                    ArrayList list = new ArrayList();
                    ArrayList list2 = new ArrayList();
                    if (possiblyDuplicateAttr.Expressions != null)
                    {
                        ExpressionList.Enumerator enumerator = possiblyDuplicateAttr.Expressions.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            list.Add(enumerator.Current.ToString());
                        }
                    }
                    if (targetList[num].Expressions != null)
                    {
                        ExpressionList.Enumerator enumerator2 = targetList[num].Expressions.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            list2.Add(enumerator2.Current.ToString());
                        }
                    }
                    if (list.Count == list2.Count)
                    {
                        bool flag2 = true;
                        foreach (string str in list)
                        {
                            if (!list2.Contains(str))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                        if (flag2)
                        {
                            foreach (string str2 in list2)
                            {
                                if (!list.Contains(str2))
                                {
                                    flag2 = false;
                                    break;
                                }
                            }
                        }
                        if (flag2)
                        {
                            break;
                        }
                    }
                }
                num++;
            }
            if (num == targetList.Count)
            {
                flag = true;
            }
            return flag;
        }

        protected virtual bool CheckUsage(string[] args)
        {
            if ((args.Length >= 1) && ((args.Length != 1) || (((string.Compare(args[0], "-?", true) != 0) && (string.Compare(args[0], "/?", true) != 0)) && (string.Compare(args[0], "-h", true) != 0))))
            {
                return false;
            }
            return true;
        }

        protected virtual Duplicator CreateDuplicator(System.Compiler.Module module) => 
            new Duplicator(module, null);

        protected virtual Duplicator CreateDuplicator(System.Compiler.Module module, TypeNode typeNode) => 
            new Duplicator(module, typeNode);

        private AssemblyNode CreateTargetAssembly(string outputAssemblyName, ModuleKindFlags kind)
        {
            AssemblyNode declaringModule = new AssemblyNode {
                Name = outputAssemblyName,
                Kind = kind
            };
            if (declaringModule != null)
            {
                declaringModule.Version = new System.Version(0, 0, 0, 0);
            }
            declaringModule.ModuleReferences = new ModuleReferenceList();
            declaringModule.Documentation = new XmlDocument();
            declaringModule.AssemblyReferences = new AssemblyReferenceList();
            TypeNodeList list2 = declaringModule.Types = new TypeNodeList();
            this.hiddenClass = new Class(declaringModule, null, null, TypeFlags.Public, Identifier.Empty, Identifier.For("<Module>"), null, new InterfaceList(), new MemberList(0));
            list2.Add(this.hiddenClass);
            return declaringModule;
        }

        private bool ExemptType(TypeNode t)
        {
            if (this.exemptionList != null)
            {
                foreach (Regex regex in this.exemptionList)
                {
                    if (regex.Match(t.FullName).Success)
                    {
                        return true;
                    }
                    if (regex.Match("[" + t.DeclaringModule.Name + "]" + t.FullName).Success)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void FuzzilyForwardReferencesFromSource2Target(AssemblyNode targetAssembly, AssemblyNode sourceAssembly)
        {
            int num = 1;
            int count = sourceAssembly.Types.Count;
            while (num < count)
            {
                TypeNode node = sourceAssembly.Types[num];
                TypeNode type = targetAssembly.GetType(node.Namespace, node.Name);
                if (type == null)
                {
                    if (this.d.TypesToBeDuplicated[node.UniqueKey] == null)
                    {
                        this.d.FindTypesToBeDuplicated(new TypeNodeList(new TypeNode[] { node }));
                    }
                }
                else
                {
                    this.d.DuplicateFor[node.UniqueKey] = type;
                    int num3 = 0;
                    int num4 = node.Members.Count;
                    while (num3 < num4)
                    {
                        Member m = node.Members[num3];
                        Member member2 = this.FuzzilyGetMatchingMember(type, m);
                        if (member2 != null)
                        {
                            this.d.DuplicateFor[m.UniqueKey] = member2;
                        }
                        num3++;
                    }
                }
                num++;
            }
        }

        private Member FuzzilyGetMatchingMember(TypeNode t, Member m)
        {
            MemberList membersNamed = t.GetMembersNamed(m.Name);
            int num = 0;
            int count = membersNamed.Count;
            while (num < count)
            {
                Member member = membersNamed[num];
                if (member.NodeType == m.NodeType)
                {
                    System.Compiler.Method method = member as System.Compiler.Method;
                    if (method != null)
                    {
                        if (this.FuzzyEqual(((System.Compiler.Method) m).Parameters, method.Parameters))
                        {
                            return member;
                        }
                    }
                    else if (m.NodeType == NodeType.Field)
                    {
                        if (this.FuzzyEqual(((Field) m).Type, ((Field) member).Type))
                        {
                            return member;
                        }
                    }
                    else if (m.NodeType == NodeType.Event)
                    {
                        if (this.FuzzyEqual(((Event) m).HandlerType, ((Event) member).HandlerType))
                        {
                            return member;
                        }
                    }
                    else if (m.NodeType == NodeType.Property)
                    {
                        if (this.FuzzyEqual(((Property) m).Type, ((Property) member).Type))
                        {
                            return member;
                        }
                    }
                    else
                    {
                        TypeNode node = member as TypeNode;
                        if ((node != null) && this.FuzzyEqual((TypeNode) m, node))
                        {
                            return member;
                        }
                    }
                }
                num++;
            }
            return null;
        }

        private bool FuzzyEqual(ParameterList xs, ParameterList ys)
        {
            if (xs.Count != ys.Count)
            {
                return false;
            }
            int num = 0;
            int count = xs.Count;
            while (num < count)
            {
                if (!this.FuzzyEqual(xs[num].Type, ys[num].Type))
                {
                    return false;
                }
                num++;
            }
            return true;
        }

        private bool FuzzyEqual(TypeNode t1, TypeNode t2) => 
            ((t1 == t2) || (((((t1 != null) && (t2 != null)) && ((t1.Namespace != null) && (t2.Namespace != null))) && (((t1.Name != null) && (t2.Name != null)) && (t1.Namespace.Name == t2.Namespace.Name))) && (t1.Name.Name == t2.Name.Name)));

        private static AttributeNode GetAttributeByName(TypeNode typeNode, string name)
        {
            int num = 0;
            int num2 = (typeNode.Attributes == null) ? 0 : typeNode.Attributes.Count;
            while (num < num2)
            {
                AttributeNode node = typeNode.Attributes[num];
                if ((node != null) && node.Type.FullName.Equals(name))
                {
                    return node;
                }
                num++;
            }
            return null;
        }

        private static bool GetComVisibleSettingForAssembly(AssemblyNode a)
        {
            bool flag = true;
            int num = 0;
            int num2 = (a.Attributes == null) ? 0 : a.Attributes.Count;
            while (num < num2)
            {
                AttributeNode node = a.Attributes[num];
                if (((node != null) && (node.Type == SystemTypes.ComVisibleAttribute)) && ((node.Expressions != null) && (0 < node.Expressions.Count)))
                {
                    Literal literal = node.Expressions[0] as Literal;
                    if ((literal != null) && (literal.Type == SystemTypes.Boolean))
                    {
                        flag = (bool) literal.Value;
                    }
                }
                num++;
            }
            return flag;
        }

        private bool IsCompilerGenerated(TypeNode t) => 
            (GetAttributeByName(t, "System.Runtime.CompilerServices.CompilerGeneratedAttribute") != null);

        [STAThread]
        public static int Main(string[] args)
        {
            ILMerge merge = new ILMerge();
            if (merge.CheckUsage(args))
            {
                Console.WriteLine(merge.UsageString);
                return 0;
            }
            if (!merge.ProcessCommandLineOptions(args))
            {
                Console.WriteLine(merge.UsageString);
                return 1;
            }
            if (!merge.ValidateOptions())
            {
                Console.WriteLine(merge.UsageString);
                return 1;
            }
            Assembly assembly = typeof(ILMerge).Assembly;
            merge.WriteToLog("ILMerge version " + assembly.GetName().Version.ToString(), new object[0]);
            merge.WriteToLog("Copyright (C) Microsoft Corporation 2004-2006. All rights reserved.", new object[0]);
            string s = "ILMerge ";
            foreach (string str2 in args)
            {
                s = s + str2 + " ";
            }
            merge.WriteToLog(s, new object[0]);
            try
            {
                merge.Merge();
            }
            catch (Exception exception)
            {
                if (merge.log)
                {
                    merge.WriteToLog("An exception occurred during merging:", new object[0]);
                    merge.WriteToLog(exception.Message, new object[0]);
                    merge.WriteToLog(exception.StackTrace, new object[0]);
                }
                else
                {
                    Console.WriteLine("An exception occurred during merging:");
                    Console.WriteLine(exception.Message);
                    Console.WriteLine(exception.StackTrace);
                }
                return 1;
            }
            return 0;
        }

        public void Merge()
        {
            string[] directories;
            int num;
            ModuleKindFlags dynamicallyLinkedLibrary;
            if (((this.outputFileName == null) || (this.assemblyNames == null)) || (this.assemblyNames.Count <= 0))
            {
                throw new InvalidOperationException("ILMerge.Merge: Must set the InputAssemblies and OutputFile properties before calling this method.");
            }
            using (IEnumerator enumerator = this.assemblyNames.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == null)
                    {
                        throw new InvalidOperationException("ILMerge.Merge: Cannot have any null elements in the InputAssemblies.");
                    }
                }
            }
            if (this.targetKind == Kind.SameAsPrimaryAssembly)
            {
                string extension = Path.GetExtension(this.outputFileName);
                if (string.Compare(Path.GetExtension((string) this.assemblyNames[0]), extension, true) != 0)
                {
                    string s = "/target not specified, but output file, '" + this.outputFileName + "', has a different extension than the primary assembly, '" + ((string) this.assemblyNames[0]) + "'.";
                    this.WriteToLog(s, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + s);
                }
            }
            else
            {
                string strA = Path.GetExtension(this.outputFileName);
                if (this.targetKind == Kind.Dll)
                {
                    if (string.Compare(strA, ".dll", true) != 0)
                    {
                        string str5 = "/target specified as library, but output file, '" + this.outputFileName + "', does not have a .dll extension.";
                        this.WriteToLog(str5, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str5);
                    }
                }
                else if (string.Compare(strA, ".exe", true) != 0)
                {
                    string str6 = "/target specified as an executable, but output file, '" + this.outputFileName + "', does not have a .exe extension.";
                    this.WriteToLog(str6, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str6);
                }
            }
            if (this.directories != null)
            {
                directories = this.directories;
                num = 0;
                while (num < directories.Length)
                {
                    string path = directories[num];
                    if (!Directory.Exists(path))
                    {
                        throw new InvalidOperationException("Specified search directory '" + path + "' not found.");
                    }
                    num++;
                }
            }
            if ((this.keyfile != null) && !File.Exists(this.keyfile))
            {
                throw new InvalidOperationException("Specified key file '" + this.keyfile + "' not found.");
            }
            if ((this.excludeFile != "") && !File.Exists(this.excludeFile))
            {
                throw new InvalidOperationException("Specified exclude file '" + this.excludeFile + "' not found.");
            }
            if ((this.attributeFileName != null) && this.copyattrs)
            {
                throw new InvalidOperationException("Cannot specify both an attribute file and to copy attributes from the input assemblies.");
            }
            if (this.unionMerge && this.allowAllDuplicates)
            {
                throw new InvalidOperationException("Cannot specify both /union and /allowDup.");
            }
            if (this.targetPlatformSpecified)
            {
                this.SetTargetPlatform(this.clrVersion, this.clrDir);
                if (this.DebugInfo && (TargetPlatform.TargetVersion.Major < 2))
                {
                    this.WriteToLog("Target platform is not v2: turning off debug info", new object[0]);
                    this.DebugInfo = false;
                }
            }
            else
            {
                this.SetTargetPlatform("v2", null);
            }
            TargetPlatform.ResetCci(TargetPlatform.PlatformAssembliesLocation, TargetPlatform.TargetVersion, true, this.DebugInfo);
            try
            {
                this.WriteToLog("Running on Microsoft (R) .NET Framework " + Path.GetFileName(Path.GetDirectoryName(typeof(object).Module.Assembly.Location)), new object[0]);
                this.WriteToLog("mscorlib.dll version = " + typeof(object).Module.Assembly.GetName().Version, new object[0]);
            }
            catch (Exception exception)
            {
                this.WriteToLog("Could not determine runtime version.", new object[0]);
                this.WriteToLog("Exception occurred: ", new object[0]);
                this.WriteToLog(exception.ToString(), new object[0]);
            }
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.outputFileName);
            IDictionary staticAssemblyCache = TargetPlatform.StaticAssemblyCache;
            AssemblyResolver resolver = new AssemblyResolver(staticAssemblyCache);
            if ((this.directories != null) && (this.directories.Length > 0))
            {
                resolver.SearchDirectories = this.directories;
            }
            resolver.DebugInfo = this.debugInfo;
            resolver.Log = this.log;
            resolver.LogFile = this.logFile;
            resolver.PreserveShortBranches = this.shortB;
            if ((this.excludeFile != null) && (this.excludeFile != ""))
            {
                int num2 = 0;
                this.exemptionList = new ArrayList();
                try
                {
                    using (StreamReader reader = new StreamReader(this.excludeFile))
                    {
                        string str9;
                        while ((str9 = reader.ReadLine()) != null)
                        {
                            this.exemptionList.Add(new Regex(str9));
                            num2++;
                        }
                        this.WriteToLog("Read {0} lines from the exclusion file '{1}'.", new object[] { num2, this.excludeFile });
                    }
                }
                catch (Exception exception2)
                {
                    this.WriteToLog("Something went wrong reading the exclusion file '{0}'; read in {1} lines, continuing processing.", new object[] { this.excludeFile, num2 });
                    this.WriteToLog(exception2.Message, new object[0]);
                }
            }
            this.WriteToLog("The list of input assemblies is:", new object[0]);
            foreach (string str10 in this.assemblyNames)
            {
                this.WriteToLog("\t" + str10, new object[0]);
            }
            if (!this.UnionMerge)
            {
                Hashtable hashtable = new Hashtable(this.assemblyNames.Count);
                foreach (string str11 in this.assemblyNames)
                {
                    if (hashtable.ContainsKey(str11))
                    {
                        string str12 = "Duplicate assembly name '" + str11 + "'.";
                        this.WriteToLog(str12, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str12);
                    }
                    hashtable.Add(str11, true);
                }
            }
            new Hashtable();
            AssemblyNodeList assems = new AssemblyNodeList();
            int num3 = 0;
            int count = this.assemblyNames.Count;
            while (num3 < count)
            {
                string str13 = (string) this.assemblyNames[num3];
                int num5 = assems.Count;
                int num6 = 0;
                int num7 = (this.directories == null) ? 1 : (this.directories.Length + 1);
                while (num6 < num7)
                {
                    string currentDirectory = null;
                    if (num6 == 0)
                    {
                        currentDirectory = Directory.GetCurrentDirectory();
                    }
                    else
                    {
                        currentDirectory = this.directories[num6 - 1];
                    }
                    string[] files = null;
                    if (this.allowWildCards)
                    {
                        if (Path.IsPathRooted(str13))
                        {
                            files = Directory.GetFiles(Path.GetDirectoryName(str13), Path.GetFileName(str13));
                        }
                        else
                        {
                            files = Directory.GetFiles(currentDirectory, str13);
                        }
                        this.WriteToLog("The number of files matching the pattern {0} is {1}.", new object[] { str13, files.Length });
                        directories = files;
                        num = 0;
                        while (num < directories.Length)
                        {
                            string str15 = directories[num];
                            this.WriteToLog(str15, new object[0]);
                            num++;
                        }
                        if ((files != null) && (files.Length > 0))
                        {
                            goto Label_07F6;
                        }
                        goto Label_0A1C;
                    }
                    string str16 = Path.Combine(currentDirectory, str13);
                    if (!File.Exists(str16))
                    {
                        goto Label_0A1C;
                    }
                    files = new string[] { str16 };
                Label_07F6:
                    directories = files;
                    for (num = 0; num < directories.Length; num++)
                    {
                        string str17 = directories[num];
                        this.WriteToLog("Trying to read assembly from the file '{0}'.", new object[] { str17 });
                        bool debugInfo = this.debugInfo;
                        if (debugInfo && !File.Exists(Path.ChangeExtension(str17, ".pdb")))
                        {
                            this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", new object[] { (string) this.assemblyNames[num3] });
                            debugInfo = false;
                        }
                        AssemblyNode node = AssemblyNode.GetAssembly(str17, staticAssemblyCache, true, debugInfo, true, this.shortB);
                        if (node == null)
                        {
                            string str19 = "Could not load assembly from the location '" + str17 + "'. Skipping and processing rest of arguments.";
                            this.WriteToLog(str19, new object[0]);
                            throw new InvalidOperationException("ILMerge.Merge: " + str19);
                        }
                        this.WriteToLog("\tSuccessfully read in assembly.", new object[0]);
                        node.AssemblyReferenceResolution += new System.Compiler.Module.AssemblyReferenceResolver(resolver.Resolve);
                        assems.Add(node);
                        if ((node.MetadataImportErrors != null) && (node.MetadataImportErrors.Count > 0))
                        {
                            string str20 = "\tThere were errors reported in " + node.Name + "'s metadata.\n";
                            foreach (Exception exception3 in node.MetadataImportErrors)
                            {
                                str20 = str20 + "\t" + exception3.Message;
                            }
                            this.WriteToLog(str20, new object[0]);
                            throw new InvalidOperationException("ILMerge.Merge: " + str20);
                        }
                        this.WriteToLog("\tThere were no errors reported in {0}'s metadata.", new object[] { node.Name });
                        if (this.UnionMerge)
                        {
                            if (node.Name != null)
                            {
                                staticAssemblyCache.Remove(node.Name);
                            }
                            if (node.StrongName != null)
                            {
                                staticAssemblyCache.Remove(node.StrongName);
                            }
                        }
                    }
                    break;
                Label_0A1C:
                    num6++;
                }
                if (num5 == assems.Count)
                {
                    string str21 = "Could not find the file '" + str13 + "'.";
                    this.WriteToLog(str21, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str21);
                }
                num3++;
            }
            if (assems.Count <= 0)
            {
                string str22 = "There are no assemblies to merge in. Must have been an error in reading them in?";
                this.WriteToLog(str22, new object[0]);
                throw new InvalidOperationException("ILMerge.Merge: " + str22);
            }
            if (this.closed)
            {
                CloseAssemblies assemblies = new CloseAssemblies(assems);
                int num8 = 0;
                int num9 = assems.Count;
                while (num8 < num9)
                {
                    AssemblyNode node2 = assems[num8];
                    int num10 = 0;
                    int num11 = node2.AssemblyReferences.Count;
                    while (num10 < num11)
                    {
                        assemblies.Visit(node2.AssemblyReferences[num10].Assembly);
                        num10++;
                    }
                    num8++;
                }
                this.WriteToLog("In order to close the target assembly, the number of assemblies to be added to the input is {0}.", new object[] { assemblies.assembliesToBeAdded.Count });
                foreach (AssemblyNode node3 in assemblies.assembliesToBeAdded.Values)
                {
                    this.WriteToLog("\tAdding assembly '{0}' to the input list.", new object[] { node3.Name });
                    assems.Add(node3);
                }
            }
            this.WriteToLog("Checking to see that all of the input assemblies have a compatible PeKind.", new object[0]);
            PEKindFlags pEKind = assems[0].PEKind;
            this.WriteToLog(string.Concat(new object[] { "\t", assems[0].Name, ".PeKind = ", assems[0].PEKind }), new object[0]);
            if ((pEKind & PEKindFlags.ILonly) == 0)
            {
                if (!this.allowZeroPeKind)
                {
                    string str23 = "The assembly '" + assems[0].Name + "' is not marked as containing only managed code.\n(Consider using the /zeroPeKind option -- but read the documentation first!)";
                    this.WriteToLog(str23, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str23);
                }
                pEKind |= PEKindFlags.ILonly;
                this.WriteToLog(string.Concat(new object[] { "\tThe effective PeKind for '", assems[0].Name, "' will be considered to be: ", pEKind }), new object[0]);
            }
            int num12 = 1;
            int num13 = assems.Count;
            while (num12 < num13)
            {
                AssemblyNode node4 = assems[num12];
                PEKindFlags flags2 = node4.PEKind;
                this.WriteToLog(string.Concat(new object[] { "\t", node4.Name, ".PeKind = ", flags2 }), new object[0]);
                if ((flags2 & PEKindFlags.ILonly) == 0)
                {
                    if (!this.allowZeroPeKind)
                    {
                        string str24 = "The assembly '" + node4.Name + "' is not marked as containing only managed code.\n(Consider using the /zeroPeKind option -- but read the documentation first!)";
                        this.WriteToLog(str24, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str24);
                    }
                    flags2 |= PEKindFlags.ILonly;
                    this.WriteToLog(string.Concat(new object[] { "\tThe effective PeKind for '", node4.Name, "' will be considered to be: ", flags2 }), new object[0]);
                }
                if (flags2 != pEKind)
                {
                    if (pEKind == PEKindFlags.ILonly)
                    {
                        pEKind = flags2;
                    }
                    else if (flags2 != PEKindFlags.ILonly)
                    {
                        string str25 = string.Concat(new object[] { "The assembly '", node4.Name, "' has a value for its PeKind flag, '", flags2, "' that is not compatible with '", pEKind, "'." });
                        this.WriteToLog(str25, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str25);
                    }
                }
                num12++;
            }
            this.WriteToLog("All input assemblies have a compatible PeKind value.", new object[0]);
            switch (this.targetKind)
            {
                case Kind.Dll:
                    dynamicallyLinkedLibrary = ModuleKindFlags.DynamicallyLinkedLibrary;
                    break;

                case Kind.Exe:
                    dynamicallyLinkedLibrary = ModuleKindFlags.ConsoleApplication;
                    break;

                case Kind.WinExe:
                    dynamicallyLinkedLibrary = ModuleKindFlags.WindowsApplication;
                    break;

                case Kind.SameAsPrimaryAssembly:
                    dynamicallyLinkedLibrary = assems[0].Kind;
                    break;

                default:
                    throw new InvalidOperationException("ILMerge.Merge: Internal error.");
            }
            this.targetAssembly = this.CreateTargetAssembly(fileNameWithoutExtension, dynamicallyLinkedLibrary);
            this.targetAssembly.PEKind = pEKind;
            this.targetAssembly.UsePublicKeyTokensForAssemblyReferences = this.PublicKeyTokens;
            this.d = this.CreateDuplicator(this.targetAssembly);
            this.d.CopyDocumentation = this.copyXmlDocumentation;
            if (!this.UnionMerge)
            {
                this.ScanAssemblies(this.d, assems);
            }
            bool targetAssemblyIsComVisible = true;
            AssemblyNode node5 = assems[0];
            AssemblyNode node6 = null;
            if (this.attributeFileName != null)
            {
                this.WriteToLog("Trying to read attribute assembly from the file '{0}'.", new object[] { this.attributeFileName });
                node6 = AssemblyNode.GetAssembly(this.attributeFileName, staticAssemblyCache, true, false, true, false);
                if (node6 == null)
                {
                    string str26 = "The assembly '" + this.attributeFileName + "' could not be read in to be used for assembly-level information.";
                    this.WriteToLog(str26, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str26);
                }
                int num14 = 0;
                int num15 = (node6.Attributes == null) ? 0 : node6.Attributes.Count;
                while (num14 < num15)
                {
                    AttributeNode node7 = node6.Attributes[num14];
                    if (node7 != null)
                    {
                        if (node7.Type == SystemTypes.ComVisibleAttribute)
                        {
                            node6.Attributes[num14] = null;
                        }
                        else if (((node7.Type == SystemTypes.SecurityCriticalAttribute) || (node7.Type == SystemTypes.SecurityTransparentAttribute)) || ((node7.Type == SystemTypes.AllowPartiallyTrustedCallersAttribute) || node7.Type.FullName.Equals("System.Security.SecurityRules")))
                        {
                            this.WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly", new object[] { node7.Type.FullName, node6.Name });
                            node6.Attributes[num14] = null;
                        }
                    }
                    num14++;
                }
            }
            AssemblyNode a = (node6 != null) ? node6 : node5;
            if (!this.copyattrs)
            {
                this.WriteToLog("Using assembly '{0}' for assembly-level attributes for the target assembly.", new object[] { a.Name });
            }
            else
            {
                this.WriteToLog("Merging assembly-level attributes from input assemblies for the target assembly.", new object[0]);
            }
            targetAssemblyIsComVisible = GetComVisibleSettingForAssembly(a);
            AssemblyNodeList list2 = new AssemblyNodeList();
            int num16 = 0;
            int num17 = assems.Count;
            while (num16 < num17)
            {
                bool flag4;
                AssemblyNode node9 = assems[num16];
                this.WriteToLog("Merging assembly '{0}' into target assembly.", new object[] { node9.Name });
                bool makeNonPublic = false;
                if (num16 == 0)
                {
                    makeNonPublic = false;
                }
                else
                {
                    makeNonPublic = this.internalize;
                }
                if (this.UnionMerge)
                {
                    if (num16 == 0)
                    {
                        int num18 = 0;
                        int num19 = (node9.AssemblyReferences == null) ? 0 : node9.AssemblyReferences.Count;
                        while (num18 < num19)
                        {
                            list2.Add(node9.AssemblyReferences[num18].Assembly);
                            num18++;
                        }
                        this.d.FindTypesToBeDuplicated(node9.Types);
                    }
                    else
                    {
                        int num20 = 0;
                        int num21 = (node9.AssemblyReferences == null) ? 0 : node9.AssemblyReferences.Count;
                        while (num20 < num21)
                        {
                            AssemblyNode assembly = node9.AssemblyReferences[num20].Assembly;
                            int num22 = 0;
                            int num23 = list2.Count;
                            while (num22 < num23)
                            {
                                AssemblyNode targetAssembly = list2[num22];
                                if (assembly.Name == targetAssembly.Name)
                                {
                                    this.FuzzilyForwardReferencesFromSource2Target(targetAssembly, assembly);
                                    break;
                                }
                                num22++;
                            }
                            if (num22 == num23)
                            {
                                list2.Add(node9.AssemblyReferences[num20].Assembly);
                            }
                            num20++;
                        }
                    }
                    flag4 = this.MergeInAssembly_Union(node9, targetAssemblyIsComVisible);
                }
                else
                {
                    flag4 = this.MergeInAssembly(node9, makeNonPublic, targetAssemblyIsComVisible);
                }
                if (!flag4)
                {
                    this.WriteToLog("Could not merge in assembly. Skipping and processing rest of arguments.", new object[0]);
                }
                num16++;
            }
            TypeNode node12 = null;
            if ((a.Types != null) && (a.Types.Count > 0))
            {
                node12 = a.Types[0];
            }
            if (((node12 == null) && (node5.Types != null)) && (node5.Types.Count > 0))
            {
                node12 = node5.Types[0];
            }
            if (node12 != null)
            {
                this.hiddenClass.Flags = node12.Flags;
            }
            if (!this.copyattrs)
            {
                this.targetAssembly.Attributes = this.d.VisitAttributeList(a.Attributes);
                this.targetAssembly.SecurityAttributes = this.d.VisitSecurityAttributeList(a.SecurityAttributes);
                this.targetAssembly.ModuleAttributes = this.d.VisitAttributeList(a.ModuleAttributes);
            }
            this.targetAssembly.Culture = a.Culture;
            this.targetAssembly.DllCharacteristics = a.DllCharacteristics;
            this.targetAssembly.Flags = a.Flags;
            this.targetAssembly.PublicKeyOrToken = (byte[]) a.PublicKeyOrToken.Clone();
            this.targetAssembly.KeyContainerName = a.KeyContainerName;
            if (a.KeyBlob != null)
            {
                this.targetAssembly.KeyBlob = (byte[]) a.KeyBlob.Clone();
            }
            this.targetAssembly.Version = (System.Version) a.Version.Clone();
            if (((a == null) || (a.Win32Resources == null)) || (a.Win32Resources.Count <= 0))
            {
                this.WriteToLog("No Win32 Resources found in assembly '{0}'; target assembly will (also) not have any.", new object[] { a.Name });
            }
            else
            {
                int capacity = a.Win32Resources.Count;
                this.WriteToLog("Copying {1} Win32 Resources from assembly '{0}' into target assembly.", new object[] { a.Name, capacity });
                this.targetAssembly.Win32Resources = new Win32ResourceList(capacity);
                int num25 = 0;
                int num26 = capacity;
                while (num25 < num26)
                {
                    this.targetAssembly.Win32Resources.Add(a.Win32Resources[num25]);
                    num25++;
                }
            }
            InstanceInitializer constructor = SystemTypes.ComVisibleAttribute.GetConstructor(new TypeNode[] { SystemTypes.Boolean });
            AttributeNode element = new AttributeNode(new MemberBinding(null, constructor), new ExpressionList(new Expression[] { new Literal(targetAssemblyIsComVisible, SystemTypes.Boolean) }));
            this.targetAssembly.Attributes.Add(element);
            if ((this.targetAssembly.Kind == ModuleKindFlags.ConsoleApplication) || (this.targetAssembly.Kind == ModuleKindFlags.WindowsApplication))
            {
                if (node5.EntryPoint == null)
                {
                    this.WriteToLog("Trying to make the target assembly an executable, but cannot find an entry point in the primary assembly, '{0}'.", new object[] { node5.Name });
                    this.WriteToLog("Converting target assembly into a dll.", new object[0]);
                    this.targetAssembly.Kind = ModuleKindFlags.DynamicallyLinkedLibrary;
                    this.outputFileName = Path.ChangeExtension(this.outputFileName, "dll");
                }
                else
                {
                    System.Compiler.Method entryPoint = node5.EntryPoint;
                    string str27 = "entry point '" + entryPoint.FullName + "' from assembly '" + node5.Name + "' to assembly '" + this.targetAssembly.Name + "'.";
                    this.WriteToLog("Transferring " + str27, new object[0]);
                    Class declaringType = (Class) entryPoint.DeclaringType;
                    Class class3 = (Class) this.d.DuplicateFor[declaringType.UniqueKey];
                    int num1 = class3.Members.Count;
                    this.targetAssembly.EntryPoint = (System.Compiler.Method) this.d.DuplicateFor[entryPoint.UniqueKey];
                    if (this.targetAssembly.EntryPoint == null)
                    {
                        this.WriteToLog("Error in transferring " + str27, new object[0]);
                        throw new InvalidOperationException("Error in transferring " + str27);
                    }
                }
            }
            if (this.keyfile != null)
            {
                if (!File.Exists(this.keyfile))
                {
                    this.WriteToLog("ILMerge: Cannot open key file: '{0}'. Not trying to sign output.", new object[] { this.keyfile });
                }
                else
                {
                    FileStream stream = File.Open(this.keyfile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    int length = (int) stream.Length;
                    byte[] buffer = new byte[length];
                    if (stream.Read(buffer, 0, length) != length)
                    {
                        this.WriteToLog("ILMerge: Error reading contents of key file: '{0}'. Not trying to sign output.", new object[] { this.keyfile });
                    }
                    else
                    {
                        this.WriteToLog("ILMerge: Signing assembly with the key file '{0}'.", new object[] { this.keyfile });
                        this.targetAssembly.KeyBlob = new byte[length];
                        Array.Copy(buffer, 0, this.targetAssembly.KeyBlob, 0, length);
                    }
                    stream.Close();
                }
            }
            else if ((node5.PublicKeyOrToken != null) && (node5.PublicKeyOrToken.Length > 0))
            {
                this.WriteToLog("ILMerge: Important! Primary assembly had a strong name, but the output does not.", new object[0]);
                this.targetAssembly.PublicKeyOrToken = null;
                if ((this.targetAssembly.Flags & AssemblyFlags.PublicKey) != AssemblyFlags.None)
                {
                    this.targetAssembly.Flags &= ~AssemblyFlags.PublicKey;
                }
                this.strongNameLost = true;
            }
            if (this.version != null)
            {
                this.targetAssembly.Version = this.version;
            }
            if ((this.targetAssembly.MetadataImportErrors != null) && (this.targetAssembly.MetadataImportErrors.Count > 0))
            {
                this.WriteToLog("\tThere were errors reported in the target assembly's metadata.", new object[0]);
                foreach (Exception exception4 in this.targetAssembly.MetadataImportErrors)
                {
                    this.WriteToLog("\t" + exception4.Message, new object[0]);
                }
            }
            else
            {
                this.WriteToLog("\tThere were no errors reported in the target assembly's metadata.", new object[0]);
            }
            this.WriteToLog("ILMerge: Writing target assembly '{0}'.", new object[] { this.outputFileName });
            CompilerOptions options = new CompilerOptions();
            if (!this.delaySign)
            {
                try
                {
                    options.IncludeDebugInformation = this.debugInfo;
                    options.FileAlignment = this.FileAlignment;
                    this.targetAssembly.WriteModule(this.outputFileName, options);
                    if (this.copyXmlDocumentation)
                    {
                        this.targetAssembly.WriteDocumentation(new StreamWriter(Path.ChangeExtension(this.outputFileName, "xml")));
                    }
                    if (this.keyfileSpecified)
                    {
                        this.WriteToLog("ILMerge: Signed assembly '{0}' with a strong name.", new object[] { this.outputFileName });
                    }
                }
                catch (AssemblyCouldNotBeSignedException)
                {
                    this.WriteToLog("ILMerge error: The target assembly was not able to be strongly named (did you forget to use the /delaysign option?).", new object[0]);
                }
            }
            else
            {
                if (this.targetAssembly.Attributes == null)
                {
                    this.targetAssembly.Attributes = new AttributeList(1);
                }
                int num30 = 0;
                int num31 = this.targetAssembly.Attributes.Count;
                while (num30 < num31)
                {
                    AttributeNode node14 = this.targetAssembly.Attributes[num30];
                    if ((node14 != null) && (node14.Type == SystemTypes.AssemblyDelaySignAttribute))
                    {
                        break;
                    }
                    num30++;
                }
                if (num30 == num31)
                {
                    InstanceInitializer boundMember = SystemTypes.AssemblyDelaySignAttribute.GetConstructor(new TypeNode[] { SystemTypes.Boolean });
                    if (boundMember != null)
                    {
                        AttributeNode node15 = new AttributeNode(new MemberBinding(null, boundMember), new ExpressionList(new Expression[] { Literal.True }));
                        this.targetAssembly.Attributes.Add(node15);
                    }
                }
                options.DelaySign = true;
                options.IncludeDebugInformation = this.debugInfo;
                options.FileAlignment = this.FileAlignment;
                this.targetAssembly.WriteModule(this.outputFileName, options);
                if (this.copyXmlDocumentation)
                {
                    this.targetAssembly.WriteDocumentation(new StreamWriter(Path.ChangeExtension(this.outputFileName, "xml")));
                }
                this.WriteToLog("ILMerge: Delay signed assembly '{0}'.", new object[] { this.outputFileName });
            }
            int num32 = 0;
            int num33 = this.targetAssembly.AssemblyReferences.Count;
            while (num32 < num33)
            {
                AssemblyNode node16 = this.targetAssembly.AssemblyReferences[num32].Assembly;
                if (string.Compare(node16.Name, this.targetAssembly.Name, true) == 0)
                {
                    string str28 = "The target assembly '" + this.targetAssembly.Name + "' lists itself as an external reference.";
                    this.WriteToLog(str28, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str28);
                }
                int num34 = 0;
                int num35 = assems.Count;
                while (num34 < num35)
                {
                    if (node16 == assems[num34])
                    {
                        string str29 = "The assembly '" + assems[num34].Name + "' was not merged in correctly. It is still listed as an external reference in the target assembly.";
                        this.WriteToLog(str29, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str29);
                    }
                    num34++;
                }
                num32++;
            }
            try
            {
                for (int i = 0; i < this.targetAssembly.ModuleReferences.Count; i++)
                {
                    System.Compiler.Module module = this.targetAssembly.ModuleReferences[i].Module;
                    this.WriteToLog("Location for referenced module '{0}' is '{1}'", new object[] { module.Name, module.Location });
                }
                for (int j = 0; j < this.targetAssembly.AssemblyReferences.Count; j++)
                {
                    AssemblyNode node17 = this.targetAssembly.AssemblyReferences[j].Assembly;
                    this.WriteToLog("Location for referenced assembly '{0}' is '{1}'", new object[] { node17.Name, node17.Location });
                    if ((node17.MetadataImportErrors != null) && (node17.MetadataImportErrors.Count > 0))
                    {
                        this.WriteToLog("\tThere were errors reported in {0}'s metadata.", new object[] { node17.Name });
                        foreach (Exception exception5 in node17.MetadataImportErrors)
                        {
                            this.WriteToLog("\t" + exception5.Message, new object[0]);
                        }
                    }
                    else
                    {
                        this.WriteToLog("\tThere were no errors reported in  {0}'s metadata.", new object[] { node17.Name });
                    }
                }
            }
            catch (Exception exception6)
            {
                this.WriteToLog("Exception occurred while trying to print out information on references.", new object[0]);
                this.WriteToLog(exception6.ToString(), new object[0]);
            }
            this.targetAssembly.Dispose();
            TargetPlatform.Clear();
            this.WriteToLog("ILMerge: Done.", new object[0]);
        }

        private void MergeAttributeLists(AttributeList targetList, AttributeList sourceList, bool allowMultiples, bool keepFirst)
        {
            bool flag = (this.targetAssembly.Kind == ModuleKindFlags.ConsoleApplication) || (this.targetAssembly.Kind == ModuleKindFlags.WindowsApplication);
            if (sourceList != null)
            {
                AttributeList.Enumerator enumerator = sourceList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string str;
                    AttributeNode current = enumerator.Current;
                    if ((current != null) && ((!flag || ((str = current.Type.FullName) == null)) || (((str != "System.Security.AllowPartiallyTrustedCallersAttribute") && (str != "System.Security.SecurityCriticalAttribute")) && (str != "System.Security.SecurityTransparentAttribute"))))
                    {
                        if (this.UnionMerge)
                        {
                            if (!this.AttributeExistsInTarget(current, targetList))
                            {
                                targetList.Add(current);
                            }
                            continue;
                        }
                        if (allowMultiples && current.AllowMultiple)
                        {
                            targetList.Add(current);
                            continue;
                        }
                        int num = 0;
                        int count = targetList.Count;
                        while (num < count)
                        {
                            if (current.Type == targetList[num].Type)
                            {
                                if (!keepFirst)
                                {
                                    targetList[num] = current;
                                }
                                break;
                            }
                            num++;
                        }
                        if (num == count)
                        {
                            targetList.Add(current);
                        }
                    }
                }
            }
        }

        private bool MergeInAssembly(AssemblyNode a, bool makeNonPublic, bool targetAssemblyIsComVisible)
        {
            int num = 1;
            int count = a.Types.Count;
            while (num < count)
            {
                TypeNode t = a.Types[num];
                if (this.targetAssembly.GetType(t.Namespace, t.Name) != null)
                {
                    string str2;
                    if ((t.IsPublic && !this.allowAllDuplicates) && !this.typesToAllowDuplicatesOf.ContainsKey(t.Name.Name))
                    {
                        string s = $"ERROR!!: Duplicate type '{t.FullName}' found in assembly '{t.DeclaringModule.Name}'. Do you want to use the /alllowDup option?";
                        this.WriteToLog(s, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + s);
                    }
                    string name = t.Name.Name;
                    if (this.IsCompilerGenerated(t))
                    {
                        str2 = a.Name + "." + name;
                    }
                    else
                    {
                        str2 = a.Name + a.UniqueKey.ToString() + "." + name;
                    }
                    this.WriteToLog("Duplicate type name: modifying name of the type '{0}' (from assembly '{1}') to '{2}'", new object[] { t.FullName, t.DeclaringModule.Name, str2 });
                    Identifier identifier = Identifier.For(str2);
                    TypeNode node3 = (TypeNode) this.d.DuplicateFor[t.UniqueKey];
                    if (node3 == null)
                    {
                        t.Name = identifier;
                    }
                    else
                    {
                        node3.Name = identifier;
                    }
                    int num4 = 0;
                    int num5 = a.Resources.Count;
                    while (num4 < num5)
                    {
                        Resource resource = a.Resources[num4];
                        if (resource.Name.Equals(name + ".resources"))
                        {
                            this.WriteToLog("Duplicate resource name: modifying name of the resource '{0}' (from assembly '{1}') to '{2}.resources'", new object[] { resource.Name, t.DeclaringModule.Name, str2 });
                            resource.Name = str2 + ".resources";
                            a.Resources[num4] = resource;
                            break;
                        }
                        num4++;
                    }
                }
                num++;
            }
            if ((a.Types[0].Members != null) && (a.Types[0].Members.Count > 0))
            {
                TypeNode targetType = this.d.TargetType;
                this.d.TargetType = this.hiddenClass;
                int num6 = 0;
                int num7 = a.Types[0].Members.Count;
                while (num6 < num7)
                {
                    Member element = (Member) this.d.Visit(a.Types[0].Members[num6]);
                    this.hiddenClass.Members.Add(element);
                    num6++;
                }
                this.d.TargetType = targetType;
            }
            bool comVisibleSettingForAssembly = GetComVisibleSettingForAssembly(a);
            AttributeNode assemblyComVisibleAttribute = null;
            if (comVisibleSettingForAssembly != targetAssemblyIsComVisible)
            {
                InstanceInitializer constructor = SystemTypes.ComVisibleAttribute.GetConstructor(new TypeNode[] { SystemTypes.Boolean });
                assemblyComVisibleAttribute = new AttributeNode(new MemberBinding(null, constructor), new ExpressionList(new Expression[] { new Literal(comVisibleSettingForAssembly, SystemTypes.Boolean) }));
            }
            int num8 = 0;
            int num9 = (a.Attributes == null) ? 0 : a.Attributes.Count;
            while (num8 < num9)
            {
                AttributeNode node6 = a.Attributes[num8];
                if (node6 != null)
                {
                    if (node6.Type == SystemTypes.ComVisibleAttribute)
                    {
                        a.Attributes[num8] = null;
                    }
                    else if (((node6.Type == SystemTypes.SecurityCriticalAttribute) || (node6.Type == SystemTypes.SecurityTransparentAttribute)) || ((node6.Type == SystemTypes.AllowPartiallyTrustedCallersAttribute) || node6.Type.FullName.Equals("System.Security.SecurityRules")))
                    {
                        this.WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly", new object[] { node6.Type.FullName, a.Name });
                        a.Attributes[num8] = null;
                    }
                }
                num8++;
            }
            int num10 = 1;
            int num11 = a.Types.Count;
            while (num10 < num11)
            {
                TypeNode node = a.Types[num10];
                TypeNode node9 = this.d.Visit(node) as TypeNode;
                if (node9 != null)
                {
                    if ((makeNonPublic && (node9.DeclaringType == null)) && (!this.ExemptType(node) && ((node9.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.Public)))
                    {
                        node9.Flags &= ~TypeFlags.Public;
                    }
                    this.AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(node9, assemblyComVisibleAttribute);
                    this.targetAssembly.Types.Add(node9);
                }
                num10++;
            }
            if (this.copyattrs && (this.attributeAssembly == null))
            {
                AttributeList sourceList = this.d.VisitAttributeList(a.Attributes);
                this.MergeAttributeLists(this.targetAssembly.Attributes, sourceList, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
                SecurityAttributeList list2 = this.d.VisitSecurityAttributeList(a.SecurityAttributes);
                int num12 = 0;
                int num13 = list2.Count;
                while (num12 < num13)
                {
                    System.Compiler.SecurityAttribute attribute = list2[num12];
                    SecurityAction action = attribute.Action;
                    int num14 = 0;
                    int num15 = this.targetAssembly.SecurityAttributes.Count;
                    while (num14 < num15)
                    {
                        if (this.targetAssembly.SecurityAttributes[num14].Action == action)
                        {
                            break;
                        }
                        num14++;
                    }
                    if (num14 == num15)
                    {
                        this.targetAssembly.SecurityAttributes.Add(attribute);
                    }
                    else
                    {
                        AttributeList permissionAttributes = this.targetAssembly.SecurityAttributes[num14].PermissionAttributes;
                        AttributeList list4 = attribute.PermissionAttributes;
                        this.MergeAttributeLists(permissionAttributes, list4, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
                    }
                    num12++;
                }
                sourceList = this.d.VisitAttributeList(a.ModuleAttributes);
                this.MergeAttributeLists(this.targetAssembly.ModuleAttributes, sourceList, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
            }
            int num16 = 0;
            int num17 = a.Resources.Count;
            while (num16 < num17)
            {
                Resource resource2 = a.Resources[num16];
                this.targetAssembly.Resources.Add(resource2);
                num16++;
            }
            return true;
        }

        private bool MergeInAssembly_Union(AssemblyNode a, bool targetAssemblyIsComVisible)
        {
            if ((a.Types[0].Members != null) && (a.Types[0].Members.Count > 0))
            {
                TypeNode targetType = this.d.TargetType;
                this.d.TargetType = this.hiddenClass;
                int num = 0;
                int num2 = a.Types[0].Members.Count;
                while (num < num2)
                {
                    Member element = (Member) this.d.Visit(a.Types[0].Members[num]);
                    if (!this.hiddenClass.Members.Contains(element))
                    {
                        this.hiddenClass.Members.Add(element);
                    }
                    num++;
                }
                this.d.TargetType = targetType;
            }
            bool comVisibleSettingForAssembly = GetComVisibleSettingForAssembly(a);
            AttributeNode assemblyComVisibleAttribute = null;
            if (comVisibleSettingForAssembly != targetAssemblyIsComVisible)
            {
                InstanceInitializer constructor = SystemTypes.ComVisibleAttribute.GetConstructor(new TypeNode[] { SystemTypes.Boolean });
                assemblyComVisibleAttribute = new AttributeNode(new MemberBinding(null, constructor), new ExpressionList(new Expression[] { new Literal(comVisibleSettingForAssembly, SystemTypes.Boolean) }));
            }
            int num3 = 0;
            int num4 = (a.Attributes == null) ? 0 : a.Attributes.Count;
            while (num3 < num4)
            {
                AttributeNode node3 = a.Attributes[num3];
                if (node3 != null)
                {
                    if (node3.Type == SystemTypes.ComVisibleAttribute)
                    {
                        a.Attributes[num3] = null;
                    }
                    else if (((node3.Type == SystemTypes.SecurityCriticalAttribute) || (node3.Type == SystemTypes.SecurityTransparentAttribute)) || ((node3.Type == SystemTypes.AllowPartiallyTrustedCallersAttribute) || node3.Type.FullName.Equals("System.Security.SecurityRules")))
                    {
                        this.WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly", new object[] { node3.Type.FullName, a.Name });
                        a.Attributes[num3] = null;
                    }
                }
                num3++;
            }
            this.FuzzilyForwardReferencesFromSource2Target(this.targetAssembly, a);
            int num5 = 1;
            int count = a.Types.Count;
            while (num5 < count)
            {
                TypeNode node = a.Types[num5];
                TypeNode type = this.targetAssembly.GetType(node.Namespace, node.Name);
                if (type != null)
                {
                    this.memberList = (ArrayList) this.typeList[node.DocumentationId.ToString()];
                    TypeNode node6 = this.d.TargetType;
                    this.d.TargetType = type;
                    int num7 = 0;
                    int num8 = node.Members.Count;
                    while (num7 < num8)
                    {
                        Member member2 = node.Members[num7];
                        if (!this.memberList.Contains(member2.DocumentationId.ToString()))
                        {
                            Member member3 = this.d.Visit(member2) as Member;
                            if (member3 != null)
                            {
                                type.Members.Add(member3);
                                this.memberList.Add(member2.DocumentationId.ToString());
                            }
                        }
                        num7++;
                    }
                    this.d.TargetType = node6;
                }
                else
                {
                    if (this.d.TypesToBeDuplicated[node.UniqueKey] == null)
                    {
                        this.d.FindTypesToBeDuplicated(new TypeNodeList(new TypeNode[] { node }));
                    }
                    TypeNode t = this.d.Visit(node) as TypeNode;
                    if (t != null)
                    {
                        this.AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(t, assemblyComVisibleAttribute);
                        this.targetAssembly.Types.Add(t);
                        this.memberList = new ArrayList();
                        for (int i = 0; i < node.Members.Count; i++)
                        {
                            this.memberList.Add(t.Members[i].DocumentationId.ToString());
                        }
                        this.typeList.Add(t.DocumentationId.ToString(), this.memberList);
                    }
                }
                num5++;
            }
            if (this.copyattrs && (this.attributeAssembly == null))
            {
                AttributeList sourceList = this.d.VisitAttributeList(a.Attributes);
                this.MergeAttributeLists(this.targetAssembly.Attributes, sourceList, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
                SecurityAttributeList list2 = this.d.VisitSecurityAttributeList(a.SecurityAttributes);
                int num10 = 0;
                int num11 = list2.Count;
                while (num10 < num11)
                {
                    System.Compiler.SecurityAttribute attribute = list2[num10];
                    SecurityAction action = attribute.Action;
                    int num12 = 0;
                    int num13 = this.targetAssembly.SecurityAttributes.Count;
                    while (num12 < num13)
                    {
                        if (this.targetAssembly.SecurityAttributes[num12].Action == action)
                        {
                            break;
                        }
                        num12++;
                    }
                    if (num12 == num13)
                    {
                        this.targetAssembly.SecurityAttributes.Add(attribute);
                    }
                    else
                    {
                        AttributeList permissionAttributes = this.targetAssembly.SecurityAttributes[num12].PermissionAttributes;
                        AttributeList list4 = attribute.PermissionAttributes;
                        this.MergeAttributeLists(permissionAttributes, list4, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
                    }
                    num10++;
                }
                sourceList = this.d.VisitAttributeList(a.ModuleAttributes);
                this.MergeAttributeLists(this.targetAssembly.ModuleAttributes, sourceList, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
            }
            int num14 = 0;
            int num15 = a.Resources.Count;
            while (num14 < num15)
            {
                Resource resource = a.Resources[num14];
                if (!this.resourceList.Contains(resource.Name))
                {
                    this.targetAssembly.Resources.Add(resource);
                    this.resourceList.Add(resource.Name);
                }
                num14++;
            }
            return true;
        }

        protected virtual bool ProcessCommandLineOptions(string[] args)
        {
            bool flag = true;
            this.assemblyNames = new ArrayList(args.Length);
            int index = 0;
            int length = args.Length;
            while ((index < length) && flag)
            {
                string str2;
                string str3;
                string str4;
                string str = args[index];
                if ((str[0] != '-') && (str[0] != '/'))
                {
                    this.assemblyNames.Add(str);
                }
                else
                {
                    str2 = str.Substring(1);
                    int num3 = str2.IndexOf("=");
                    if (num3 < 0)
                    {
                        num3 = str2.IndexOf(":");
                    }
                    str3 = null;
                    str4 = null;
                    if (num3 < 0)
                    {
                        str3 = str2.Substring(0, str2.Length);
                        str4 = "";
                    }
                    else
                    {
                        str3 = str2.Substring(0, num3);
                        str4 = str2.Substring(num3 + 1);
                    }
                    if (string.Compare(str3, "lib", true) == 0)
                    {
                        if (str4 != "")
                        {
                            this.searchDirs.Add(str4);
                        }
                    }
                    else if (string.Compare(str3, "internalize", true) == 0)
                    {
                        this.internalize = true;
                        if (str4 != "")
                        {
                            this.excludeFile = str4;
                        }
                    }
                    else if (string.Compare(str3, "log", true) == 0)
                    {
                        this.log = true;
                        if (str4 != "")
                        {
                            this.logFile = str4;
                        }
                    }
                    else
                    {
                        if ((string.Compare(str3, "t", true) != 0) && (string.Compare(str3, "target", true) != 0))
                        {
                            goto Label_0212;
                        }
                        if (str4 == "")
                        {
                            goto Label_0201;
                        }
                        string str5 = str4;
                        if (str5 != null)
                        {
                            if (str5 != "library")
                            {
                                if (str5 == "exe")
                                {
                                    goto Label_01E9;
                                }
                                if (str5 == "winexe")
                                {
                                    goto Label_01F5;
                                }
                            }
                            else
                            {
                                this.targetKind = Kind.Dll;
                            }
                        }
                    }
                }
                goto Label_0890;
            Label_01E9:
                this.targetKind = Kind.Exe;
                goto Label_0890;
            Label_01F5:
                this.targetKind = Kind.WinExe;
                goto Label_0890;
            Label_0201:
                flag = false;
                Console.WriteLine("/target given without an accompanying kind.");
                goto Label_0890;
            Label_0212:
                if (string.Compare(str3, "ndebug", true) == 0)
                {
                    this.debugInfo = false;
                }
                else if (string.Compare(str3, "closed", true) == 0)
                {
                    this.closed = true;
                }
                else if (string.Compare(str3, "short", true) == 0)
                {
                    this.shortB = true;
                }
                else if (string.Compare(str3, "copyattrs", true) == 0)
                {
                    this.copyattrs = true;
                }
                else if (string.Compare(str3, "allowMultiple", true) == 0)
                {
                    if (!this.copyattrs)
                    {
                        Console.WriteLine("/allowMultiple specified without specifying /copyattrs");
                        flag = false;
                    }
                    else
                    {
                        this.allowMultipleAssemblyLevelAttributes = true;
                    }
                }
                else if (string.Compare(str3, "keepFirst", true) == 0)
                {
                    if (!this.copyattrs)
                    {
                        Console.WriteLine("/keepFirst specified without specifying /copyattrs");
                        flag = false;
                    }
                    else
                    {
                        this.keepFirstOfMultipleAssemblyLevelAttributes = true;
                    }
                }
                else if (string.Compare(str3, "xmldocs", true) == 0)
                {
                    this.copyXmlDocumentation = true;
                }
                else if (string.Compare(str3, "out", true) == 0)
                {
                    if (str4 != "")
                    {
                        this.outputFileName = str4;
                    }
                }
                else if (string.Compare(str3, "attr", true) == 0)
                {
                    if (str4 != "")
                    {
                        this.attributeFileName = str4;
                    }
                    else
                    {
                        Console.WriteLine("/attr given without an accompanying assembly name.");
                        flag = false;
                    }
                }
                else if ((string.Compare(str3, "reference", true) == 0) || (string.Compare(str3, "r", true) == 0))
                {
                    if (str4 != "")
                    {
                        this.assemblyNames.Add(str4);
                    }
                    else
                    {
                        Console.WriteLine("/reference given without an accompanying assembly name.");
                        flag = false;
                    }
                }
                else if (string.Compare(str3, "targetplatform", true) == 0)
                {
                    if (this.targetPlatformSpecified)
                    {
                        Console.WriteLine("Target platform already specified earlier on command line.");
                        flag = false;
                    }
                    else
                    {
                        this.targetPlatformSpecified = true;
                        if (str4 != "")
                        {
                            string str6 = str4;
                            int num4 = str6.IndexOf(",");
                            if (num4 > 0)
                            {
                                this.clrVersion = str6.Substring(0, num4);
                                this.clrDir = str6.Substring(num4 + 1);
                                if (!Directory.Exists(this.clrDir))
                                {
                                    Console.WriteLine("Error: cannot find target platform directory '{0}'.", this.clrDir);
                                    this.clrVersion = null;
                                    this.clrDir = null;
                                    flag = false;
                                }
                            }
                            else
                            {
                                this.clrVersion = str4;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Target platform needs at least a version specified.");
                            flag = false;
                        }
                    }
                }
                else if (string.Compare(str3, "v1", true) == 0)
                {
                    if (this.targetPlatformSpecified)
                    {
                        Console.WriteLine("Target platform already specified earlier on command line.");
                        flag = false;
                    }
                    else
                    {
                        this.targetPlatformSpecified = true;
                        this.clrVersion = "v1";
                    }
                }
                else if (string.Compare(str3, "v1.1", true) == 0)
                {
                    if (this.targetPlatformSpecified)
                    {
                        Console.WriteLine("Target platform already specified earlier on command line.");
                        flag = false;
                    }
                    else
                    {
                        this.targetPlatformSpecified = true;
                        this.clrVersion = "v1.1";
                    }
                }
                else if (string.Compare(str3, "v2", true) == 0)
                {
                    if (this.targetPlatformSpecified)
                    {
                        Console.WriteLine("Target platform already specified earlier on command line.");
                        flag = false;
                    }
                    else
                    {
                        this.targetPlatformSpecified = true;
                        this.clrVersion = "v2";
                    }
                }
                else if (string.Compare(str3, "v4", true) == 0)
                {
                    if (this.targetPlatformSpecified)
                    {
                        Console.WriteLine("Target platform already specified earlier on command line.");
                        flag = false;
                    }
                    else
                    {
                        this.targetPlatformSpecified = true;
                        this.clrVersion = "v4";
                    }
                }
                else if (string.Compare(str3, "useFullPublicKeyForReferences", true) == 0)
                {
                    this.usePublicKeyTokensForAssemblyReferences = false;
                }
                else if (string.Compare(str3, "zeroPeKind", true) == 0)
                {
                    this.allowZeroPeKind = true;
                }
                else if (string.Compare(str3, "wildcards", true) == 0)
                {
                    this.allowWildCards = true;
                }
                else if (string.Compare(str3, "keyfile", true) == 0)
                {
                    if (str4 != "")
                    {
                        this.keyfile = str4;
                    }
                    this.keyfileSpecified = true;
                }
                else if (string.Compare(str3, "ver", true) == 0)
                {
                    if (!string.IsNullOrEmpty(str4))
                    {
                        System.Version version = null;
                        try
                        {
                            version = new System.Version(str4);
                            if (version.Major >= 0xffff)
                            {
                                Console.WriteLine("Invalid major version '{0}' specified. It must be less than UInt16.MaxValue (0xffff).", version.Major);
                                flag = false;
                            }
                            else if (version.Minor >= 0xffff)
                            {
                                Console.WriteLine("Invalid minor version '{0}' specified. It must be less than UInt16.MaxValue (0xffff).", version.Minor);
                                flag = false;
                            }
                            else if (version.Build >= 0xffff)
                            {
                                Console.WriteLine("Invalid build '{0}' specified. It must be less than UInt16.MaxValue (0xffff).", version.Build);
                                flag = false;
                            }
                            else if (version.Revision >= 0xffff)
                            {
                                Console.WriteLine("Invalid revision '{0}' specified. It must be less than UInt16.MaxValue (0xffff).", version.Revision);
                                flag = false;
                            }
                            else
                            {
                                this.version = version;
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Console.WriteLine("Invalid version '{0}' specified. A major, minor, build, or revision component is less than zero.", str4);
                            flag = false;
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Invalid version '{0}' specified. It has fewer than two components or more than four components.", str4);
                            flag = false;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Invalid version '{0}' specified. At least one component of version does not parse to an integer.", str4);
                            flag = false;
                        }
                        catch (OverflowException)
                        {
                            Console.WriteLine("Invalid version '{0}' specified. At least one component of version represents a number greater than System.Int32.MaxValue.", str4);
                            flag = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("/ver option specified, but no version number.");
                        flag = false;
                    }
                }
                else if (string.Compare(str3, "delaysign", true) == 0)
                {
                    this.delaySign = true;
                }
                else if (string.Compare(str3, "allowDup", true) == 0)
                {
                    if (str4 != "")
                    {
                        this.typesToAllowDuplicatesOf[str4] = true;
                    }
                    else
                    {
                        this.allowAllDuplicates = true;
                    }
                }
                else if (string.Compare(str3, "union", true) == 0)
                {
                    this.unionMerge = true;
                }
                else if (string.Compare(str3, "align", true) == 0)
                {
                    if ((str4 != null) && (str4 != ""))
                    {
                        try
                        {
                            int num5 = int.Parse(str4);
                            this.FileAlignment = num5;
                        }
                        catch (FormatException)
                        {
                            flag = false;
                        }
                        catch (OverflowException)
                        {
                            flag = false;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Ignoring unknown option: '{0}'.", str2);
                    flag = false;
                }
            Label_0890:
                index++;
            }
            if (flag)
            {
                this.directories = new string[this.searchDirs.Count];
                this.searchDirs.CopyTo(this.directories);
            }
            return flag;
        }

        protected virtual void ScanAssemblies(Duplicator d, AssemblyNodeList assems)
        {
            int num = 0;
            int count = assems.Count;
            while (num < count)
            {
                AssemblyNode node = assems[num];
                d.FindTypesToBeDuplicated(node.Types);
                num++;
            }
        }

        public void SetInputAssemblies(string[] assems)
        {
            if ((assems != null) && (assems.Length > 0))
            {
                this.assemblyNames = new ArrayList(assems.Length);
                foreach (string str in assems)
                {
                    this.assemblyNames.Add(str);
                }
            }
        }

        public void SetSearchDirectories(string[] dirs)
        {
            if ((dirs != null) && (dirs.Length > 0))
            {
                this.directories = new string[dirs.Length];
                dirs.CopyTo(this.directories, 0);
            }
        }

        public void SetTargetPlatform(string platform, string dir)
        {
            System.Version version;
            switch (platform)
            {
                case "V1":
                case "v1":
                case "1":
                case "1.0":
                case "1_0":
                case "V1.1":
                case "v1.1":
                case "V1_1":
                case "v1_1":
                case "1.1":
                case "1_1":
                case "postV1":
                case "postv1":
                case "postV1.1":
                case "postv1.1":
                case "postV1_1":
                case "postv1_1":
                case "post1.1":
                case "post1_1":
                case "v2":
                case "V2":
                case "2":
                case "2.0":
                case "2_0":
                case "v4":
                case "V4":
                case "4":
                case "4.0":
                case "4_0":
                    if (((dir != null) && (dir != "")) && !Directory.Exists(dir))
                    {
                        throw new ArgumentException("Directory '" + dir + "' doesn't exist.");
                    }
                    switch (platform)
                    {
                        case "V1":
                        case "v1":
                        case "1":
                        case "1.0":
                        case "1_0":
                            version = new System.Version(1, 0, 0xce4);
                            goto Label_04FC;

                        case "V1.1":
                        case "v1.1":
                        case "1.1":
                        case "V1_1":
                        case "v1_1":
                        case "1_1":
                            version = new System.Version(1, 0, 0x1388);
                            goto Label_04FC;

                        case "postV1.1":
                        case "postv1.1":
                        case "postV1_1":
                        case "postv1_1":
                        case "post1.1":
                        case "post1_1":
                            if ((dir == null) || (dir == ""))
                            {
                                throw new ArgumentException("Directory must be specified for setting target platform to postv1.1.");
                            }
                            version = new System.Version(1, 1, 0x270f);
                            goto Label_04FC;

                        case "v2":
                        case "V2":
                        case "2":
                        case "2.0":
                        case "2_0":
                            version = new System.Version(2, 0);
                            goto Label_04FC;

                        case "v4":
                        case "V4":
                        case "4":
                        case "4.0":
                        case "4_0":
                            version = new System.Version(4, 0);
                            goto Label_04FC;
                    }
                    throw new ArgumentException("Platform '" + platform + "' not recognized.");

                default:
                    throw new ArgumentException("Platform '" + platform + "' not recognized.");
            }
        Label_04FC:
            if ((dir == null) || (dir == ""))
            {
                TargetPlatform.SetTo(version);
            }
            else
            {
                TargetPlatform.SetTo(version, dir);
            }
            if (TargetPlatform.PlatformAssembliesLocation == null)
            {
                if ((dir != null) && (dir != ""))
                {
                    throw new ArgumentException("Directory '" + dir + "' doesn't contain mscorlib.dll.");
                }
                throw new ArgumentException("Could not find the platform assemblies for '" + platform + "'.");
            }
            dir = TargetPlatform.PlatformAssembliesLocation;
            this.WriteToLog("Set platform to '{0}', using directory '{1}' for mscorlib.dll", new object[] { platform, dir });
        }

        protected virtual bool ValidateOptions()
        {
            bool flag = false;
            if (this.assemblyNames.Count <= 0)
            {
                Console.WriteLine("Must specify at least one input file!");
                flag = true;
            }
            if (this.outputFileName == null)
            {
                Console.WriteLine("Must specify an output file!");
                flag = true;
            }
            if (this.keyfileSpecified)
            {
                if (this.keyfile == null)
                {
                    flag = true;
                    Console.WriteLine("/keyfile option given, but no file name.");
                }
            }
            else if (this.delaySign)
            {
                flag = true;
                Console.WriteLine("/delaysign option given, but not the /keyfile option.");
            }
            if (this.targetPlatformSpecified && (this.clrVersion == null))
            {
                flag = true;
                Console.WriteLine("/targetplatform option given, but couldn't set it");
            }
            return !flag;
        }

        protected void WriteToLog(string s, params object[] args)
        {
            if (this.log)
            {
                if (this.logFile != null)
                {
                    StreamWriter writer = new StreamWriter(this.logFile, true);
                    writer.WriteLine(s, args);
                    writer.Close();
                }
                else
                {
                    Console.WriteLine(s, args);
                }
            }
        }

        public bool AllowMultipleAssemblyLevelAttributes
        {
            get => 
                this.allowMultipleAssemblyLevelAttributes;
            set
            {
                this.allowMultipleAssemblyLevelAttributes = value;
            }
        }

        public bool AllowWildCards
        {
            get => 
                this.allowWildCards;
            set
            {
                this.allowWildCards = value;
            }
        }

        public bool AllowZeroPeKind
        {
            get => 
                this.allowZeroPeKind;
            set
            {
                this.allowZeroPeKind = value;
            }
        }

        public string AttributeFile
        {
            get => 
                this.attributeFileName;
            set
            {
                this.attributeFileName = value;
            }
        }

        public bool Closed
        {
            get => 
                this.closed;
            set
            {
                this.closed = value;
            }
        }

        public bool CopyAttributes
        {
            get => 
                this.copyattrs;
            set
            {
                this.copyattrs = value;
            }
        }

        public bool DebugInfo
        {
            get => 
                this.debugInfo;
            set
            {
                this.debugInfo = value;
            }
        }

        public bool DelaySign
        {
            get => 
                this.delaySign;
            set
            {
                this.delaySign = value;
            }
        }

        public string ExcludeFile
        {
            get => 
                this.excludeFile;
            set
            {
                if (value == null)
                {
                    this.excludeFile = "";
                }
                else
                {
                    this.excludeFile = value;
                    this.internalize = true;
                }
            }
        }

        public int FileAlignment
        {
            get => 
                this.fileAlignment;
            set
            {
                if (value <= 0x200)
                {
                    this.fileAlignment = 0x200;
                }
                else
                {
                    int num = 0x200;
                    while (num < value)
                    {
                        num = num << 1;
                    }
                    if (value < num)
                    {
                        num = num >> 1;
                    }
                    this.fileAlignment = num;
                }
            }
        }

        public bool Internalize
        {
            get => 
                this.internalize;
            set
            {
                this.internalize = value;
            }
        }

        public bool KeepFirstOfMultipleAssemblyLevelAttributes
        {
            get => 
                this.keepFirstOfMultipleAssemblyLevelAttributes;
            set
            {
                this.keepFirstOfMultipleAssemblyLevelAttributes = value;
            }
        }

        public string KeyFile
        {
            get => 
                this.keyfile;
            set
            {
                this.keyfile = value;
            }
        }

        public bool Log
        {
            get => 
                this.log;
            set
            {
                this.log = value;
            }
        }

        public string LogFile
        {
            get => 
                this.logFile;
            set
            {
                if ((value != null) && (value != ""))
                {
                    this.logFile = value;
                    this.log = true;
                }
            }
        }

        public string OutputFile
        {
            get => 
                this.outputFileName;
            set
            {
                this.outputFileName = value;
            }
        }

        public bool PreserveShortBranches
        {
            get => 
                this.shortB;
            set
            {
                this.shortB = value;
            }
        }

        public bool PublicKeyTokens
        {
            get => 
                this.usePublicKeyTokensForAssemblyReferences;
            set
            {
                this.usePublicKeyTokensForAssemblyReferences = value;
            }
        }

        public bool StrongNameLost =>
            this.strongNameLost;

        public Kind TargetKind
        {
            get => 
                this.targetKind;
            set
            {
                this.targetKind = value;
            }
        }

        public bool UnionMerge
        {
            get => 
                this.unionMerge;
            set
            {
                this.unionMerge = value;
            }
        }

        protected virtual string UsageString =>
            "Usage: ilmerge [/lib:directory]* [/log[:filename]] [/keyfile:filename [/delaysign]] [/internalize[:filename]] [/t[arget]:(library|exe|winexe)] [/closed] [/ndebug] [/ver:version] [/copyattrs [/allowMultiple] [/keepFirst]] [/xmldocs] [/attr:filename] [/targetplatform:<version>[,<platformdir>] | /v1 | /v1.1 | /v2 | /v4] [/useFullPublicKeyForReferences] [/wildcards] [/zeroPeKind] [/allowDup:type]* [/union] [/align:n] /out:filename <primary assembly> [<other assemblies>...]";

        public System.Version Version
        {
            get => 
                this.version;
            set
            {
                this.version = value;
            }
        }

        public bool XmlDocumentation
        {
            get => 
                this.copyXmlDocumentation;
            set
            {
                this.copyXmlDocumentation = value;
            }
        }

        private class CloseAssemblies
        {
            internal Hashtable assembliesToBeAdded = new Hashtable();
            private Hashtable currentlyActiveAssemblies = new Hashtable();
            private AssemblyNodeList initialAssemblies;
            private Hashtable visitedAssemblies = new Hashtable();

            internal CloseAssemblies(AssemblyNodeList assems)
            {
                this.initialAssemblies = assems;
                int num = 0;
                int count = assems.Count;
                while (num < count)
                {
                    this.visitedAssemblies[assems[num].UniqueKey] = assems[num];
                    num++;
                }
            }

            internal void Visit(AssemblyNode a)
            {
                if (this.visitedAssemblies[a.UniqueKey] != null)
                {
                    return;
                }
                this.currentlyActiveAssemblies.Add(a.UniqueKey, a);
                if (a.AssemblyReferences != null)
                {
                    int num = 0;
                    int count = a.AssemblyReferences.Count;
                    while (num < count)
                    {
                        AssemblyNode assembly = a.AssemblyReferences[num].Assembly;
                        if (this.currentlyActiveAssemblies[assembly.UniqueKey] == null)
                        {
                            this.Visit(assembly);
                        }
                        num++;
                    }
                    int num3 = 0;
                    int num4 = a.AssemblyReferences.Count;
                    while (num3 < num4)
                    {
                        if (this.assembliesToBeAdded[a.AssemblyReferences[num3].Assembly.UniqueKey] != null)
                        {
                            this.assembliesToBeAdded[a.UniqueKey] = a;
                            goto Label_0182;
                        }
                        num3++;
                    }
                    int num5 = 0;
                    int num6 = a.AssemblyReferences.Count;
                    while (num5 < num6)
                    {
                        int num7 = 0;
                        int num8 = this.initialAssemblies.Count;
                        while (num7 < num8)
                        {
                            if (a.AssemblyReferences[num5].Assembly.StrongName.CompareTo(this.initialAssemblies[num7].StrongName) == 0)
                            {
                                this.assembliesToBeAdded[a.UniqueKey] = a;
                                break;
                            }
                            num7++;
                        }
                        num5++;
                    }
                }
            Label_0182:
                this.visitedAssemblies[a.UniqueKey] = a;
                this.currentlyActiveAssemblies.Remove(a.UniqueKey);
            }
        }

        public enum Kind
        {
            Dll,
            Exe,
            WinExe,
            SameAsPrimaryAssembly
        }
    }
}

