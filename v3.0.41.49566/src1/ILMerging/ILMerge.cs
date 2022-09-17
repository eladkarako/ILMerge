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
        private StandardVisitor externalVisitor;
        private int fileAlignment = 0x200;
        private Class hiddenClass;
        private bool internalize;
        private bool keepFirstOfMultipleAssemblyLevelAttributes;
        private string keyContainer;
        private bool keyContainerSpecified;
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
                        bool flag = (meth.Flags & MethodFlags.NewSlot) > MethodFlags.CompilerControlled;
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
                    ExpressionList.Enumerator enumerator;
                    ArrayList list = new ArrayList();
                    ArrayList list2 = new ArrayList();
                    if (possiblyDuplicateAttr.Expressions != null)
                    {
                        enumerator = possiblyDuplicateAttr.Expressions.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            list.Add(enumerator.Current.ToString());
                        }
                    }
                    if (targetList[num].Expressions != null)
                    {
                        enumerator = targetList[num].Expressions.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            list2.Add(enumerator.Current.ToString());
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
            XmlDocument document = new XmlDocument {
                XmlResolver = null
            };
            declaringModule.Documentation = document;
            declaringModule.AssemblyReferences = new AssemblyReferenceList();
            this.hiddenClass = new Class(declaringModule, null, null, TypeFlags.Public, Identifier.Empty, Identifier.For("<Module>"), null, new InterfaceList(), new MemberList(0));
            (declaringModule.Types = new TypeNodeList()).Add(this.hiddenClass);
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
                        TypeNode[] elements = new TypeNode[] { node };
                        this.d.FindTypesToBeDuplicated(new TypeNodeList(elements));
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
            (GetAttributeByName(t, "System.Runtime.CompilerServices.CompilerGeneratedAttribute") > null);

        private static bool IsPortablePdb(string pdb)
        {
            bool flag;
            using (FileStream stream = File.OpenRead(pdb))
            {
                long position = stream.Position;
                try
                {
                    flag = new BinaryReader(stream).ReadUInt32() == 0x424a5342;
                }
                finally
                {
                    stream.Position = position;
                }
            }
            return flag;
        }

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
            merge.WriteToLog("=============================================", new object[0]);
            merge.WriteToLog("Timestamp (UTC): " + DateTime.UtcNow.ToString(), new object[0]);
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
            ModuleKindFlags dynamicallyLinkedLibrary;
            string[] directories;
            int num;
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
                    string[] textArray1 = new string[] { "/target not specified, but output file, '", this.outputFileName, "', has a different extension than the primary assembly, '", (string) this.assemblyNames[0], "'." };
                    string s = string.Concat(textArray1);
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
                this.SetTargetPlatform("v4", null);
            }
            TargetPlatform.ResetCci(TargetPlatform.PlatformAssembliesLocation, TargetPlatform.TargetVersion, true, this.DebugInfo, null);
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
            if ((this.directories != null) && (this.directories.Length != 0))
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
                        string str8;
                        while ((str8 = reader.ReadLine()) != null)
                        {
                            this.exemptionList.Add(new Regex(str8));
                            num2++;
                        }
                        object[] objArray1 = new object[] { num2, this.excludeFile };
                        this.WriteToLog("Read {0} lines from the exclusion file '{1}'.", objArray1);
                    }
                }
                catch (Exception exception2)
                {
                    object[] objArray2 = new object[] { this.excludeFile, num2 };
                    this.WriteToLog("Something went wrong reading the exclusion file '{0}'; read in {1} lines, continuing processing.", objArray2);
                    this.WriteToLog(exception2.Message, new object[0]);
                }
            }
            this.WriteToLog("The list of input assemblies is:", new object[0]);
            foreach (string str9 in this.assemblyNames)
            {
                this.WriteToLog("\t" + str9, new object[0]);
            }
            if (!this.UnionMerge)
            {
                Hashtable hashtable = new Hashtable(this.assemblyNames.Count);
                foreach (string str10 in this.assemblyNames)
                {
                    if (hashtable.ContainsKey(str10))
                    {
                        string str11 = "Duplicate assembly name '" + str10 + "'.";
                        this.WriteToLog(str11, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str11);
                    }
                    hashtable.Add(str10, true);
                }
            }
            new Hashtable();
            AssemblyNodeList assems = new AssemblyNodeList();
            int num3 = 0;
            int count = this.assemblyNames.Count;
            while (num3 < count)
            {
                string str12 = (string) this.assemblyNames[num3];
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
                        if (Path.IsPathRooted(str12))
                        {
                            files = Directory.GetFiles(Path.GetDirectoryName(str12), Path.GetFileName(str12));
                        }
                        else
                        {
                            files = Directory.GetFiles(currentDirectory, str12);
                        }
                        object[] objArray3 = new object[] { str12, files.Length };
                        this.WriteToLog("The number of files matching the pattern {0} is {1}.", objArray3);
                        directories = files;
                        num = 0;
                        while (num < directories.Length)
                        {
                            string str14 = directories[num];
                            this.WriteToLog(str14, new object[0]);
                            num++;
                        }
                        if ((files != null) && (files.Length != 0))
                        {
                            goto Label_0727;
                        }
                        goto Label_0952;
                    }
                    string str15 = Path.Combine(currentDirectory, str12);
                    if (!File.Exists(str15))
                    {
                        goto Label_0952;
                    }
                    files = new string[] { str15 };
                Label_0727:
                    directories = files;
                    for (num = 0; num < directories.Length; num++)
                    {
                        string str16 = directories[num];
                        object[] objArray4 = new object[] { str16 };
                        this.WriteToLog("Trying to read assembly from the file '{0}'.", objArray4);
                        bool debugInfo = this.debugInfo;
                        if (debugInfo)
                        {
                            string str17 = Path.ChangeExtension(str16, ".pdb");
                            if (!File.Exists(str17))
                            {
                                object[] objArray5 = new object[] { (string) this.assemblyNames[num3] };
                                this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", objArray5);
                                debugInfo = false;
                            }
                            else if (IsPortablePdb(str17))
                            {
                                object[] objArray6 = new object[] { (string) this.assemblyNames[num3] };
                                this.WriteToLog("Can not use portable PDB file. Debug info will not be available for assembly '{0}'.", objArray6);
                                debugInfo = false;
                            }
                        }
                        AssemblyNode node6 = AssemblyNode.GetAssembly(str16, staticAssemblyCache, true, debugInfo, true, this.shortB);
                        if (node6 == null)
                        {
                            string str18 = "Could not load assembly from the location '" + str16 + "'. Skipping and processing rest of arguments.";
                            this.WriteToLog(str18, new object[0]);
                            throw new InvalidOperationException("ILMerge.Merge: " + str18);
                        }
                        this.WriteToLog("\tSuccessfully read in assembly.", new object[0]);
                        node6.AssemblyReferenceResolution += new System.Compiler.Module.AssemblyReferenceResolver(resolver.Resolve);
                        assems.Add(node6);
                        if ((node6.MetadataImportErrors != null) && (node6.MetadataImportErrors.Count > 0))
                        {
                            string str19 = "\tThere were errors reported in " + node6.Name + "'s metadata.\n";
                            foreach (Exception exception3 in node6.MetadataImportErrors)
                            {
                                str19 = str19 + "\t" + exception3.Message;
                            }
                            this.WriteToLog(str19, new object[0]);
                            throw new InvalidOperationException("ILMerge.Merge: " + str19);
                        }
                        object[] objArray7 = new object[] { node6.Name };
                        this.WriteToLog("\tThere were no errors reported in {0}'s metadata.", objArray7);
                        if (this.UnionMerge)
                        {
                            if (node6.Name != null)
                            {
                                staticAssemblyCache.Remove(node6.Name);
                            }
                            if (node6.StrongName != null)
                            {
                                staticAssemblyCache.Remove(node6.StrongName);
                            }
                        }
                    }
                    break;
                Label_0952:
                    num6++;
                }
                if (num5 == assems.Count)
                {
                    string str20 = "Could not find the file '" + str12 + "'.";
                    this.WriteToLog(str20, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str20);
                }
                num3++;
            }
            if (assems.Count <= 0)
            {
                string str21 = "There are no assemblies to merge in. Must have been an error in reading them in?";
                this.WriteToLog(str21, new object[0]);
                throw new InvalidOperationException("ILMerge.Merge: " + str21);
            }
            if (this.closed)
            {
                CloseAssemblies assemblies = new CloseAssemblies(assems);
                int num8 = 0;
                int num9 = assems.Count;
                while (num8 < num9)
                {
                    AssemblyNode node7 = assems[num8];
                    int num10 = 0;
                    int num11 = node7.AssemblyReferences.Count;
                    while (num10 < num11)
                    {
                        assemblies.Visit(node7.AssemblyReferences[num10].Assembly);
                        num10++;
                    }
                    num8++;
                }
                object[] objArray8 = new object[] { assemblies.assembliesToBeAdded.Count };
                this.WriteToLog("In order to close the target assembly, the number of assemblies to be added to the input is {0}.", objArray8);
                foreach (AssemblyNode node8 in assemblies.assembliesToBeAdded.Values)
                {
                    object[] objArray9 = new object[] { node8.Name };
                    this.WriteToLog("\tAdding assembly '{0}' to the input list.", objArray9);
                    assems.Add(node8);
                }
            }
            this.WriteToLog("Checking to see that all of the input assemblies have a compatible PeKind.", new object[0]);
            PEKindFlags pEKind = assems[0].PEKind;
            this.WriteToLog(string.Concat(new object[] { "\t", assems[0].Name, ".PeKind = ", assems[0].PEKind }), new object[0]);
            if ((pEKind & PEKindFlags.ILonly) == 0)
            {
                if (!this.allowZeroPeKind)
                {
                    string str22 = "The assembly '" + assems[0].Name + "' is not marked as containing only managed code.\n(Consider using the /zeroPeKind option -- but read the documentation first!)";
                    this.WriteToLog(str22, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str22);
                }
                pEKind |= PEKindFlags.ILonly;
                this.WriteToLog(string.Concat(new object[] { "\tThe effective PeKind for '", assems[0].Name, "' will be considered to be: ", pEKind }), new object[0]);
            }
            int num12 = 1;
            int num13 = assems.Count;
            while (num12 < num13)
            {
                AssemblyNode node9 = assems[num12];
                PEKindFlags flags3 = node9.PEKind;
                this.WriteToLog(string.Concat(new object[] { "\t", node9.Name, ".PeKind = ", flags3 }), new object[0]);
                if ((flags3 & PEKindFlags.ILonly) == 0)
                {
                    if (!this.allowZeroPeKind)
                    {
                        string str24 = "The assembly '" + node9.Name + "' is not marked as containing only managed code.\n(Consider using the /zeroPeKind option -- but read the documentation first!)";
                        this.WriteToLog(str24, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str24);
                    }
                    flags3 |= PEKindFlags.ILonly;
                    this.WriteToLog(string.Concat(new object[] { "\tThe effective PeKind for '", node9.Name, "' will be considered to be: ", flags3 }), new object[0]);
                }
                if (flags3 != pEKind)
                {
                    if (pEKind == PEKindFlags.ILonly)
                    {
                        pEKind = flags3;
                    }
                    else if (flags3 != PEKindFlags.ILonly)
                    {
                        object[] objArray14 = new object[] { "The assembly '", node9.Name, "' has a value for its PeKind flag, '", flags3, "' that is not compatible with '", pEKind, "'." };
                        string str23 = string.Concat(objArray14);
                        this.WriteToLog(str23, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str23);
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
            AssemblyNode node = assems[0];
            AssemblyNode node2 = null;
            if (this.attributeFileName != null)
            {
                object[] objArray15 = new object[] { this.attributeFileName };
                this.WriteToLog("Trying to read attribute assembly from the file '{0}'.", objArray15);
                node2 = AssemblyNode.GetAssembly(this.attributeFileName, staticAssemblyCache, true, false, true, false);
                if (node2 == null)
                {
                    string str25 = "The assembly '" + this.attributeFileName + "' could not be read in to be used for assembly-level information.";
                    this.WriteToLog(str25, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str25);
                }
                int num14 = 0;
                int num15 = (node2.Attributes == null) ? 0 : node2.Attributes.Count;
                while (num14 < num15)
                {
                    AttributeNode node10 = node2.Attributes[num14];
                    if (node10 != null)
                    {
                        if (node10.Type == SystemTypes.ComVisibleAttribute)
                        {
                            node2.Attributes[num14] = null;
                        }
                        else if (((node10.Type == SystemTypes.SecurityCriticalAttribute) || (node10.Type == SystemTypes.SecurityTransparentAttribute)) || ((node10.Type == SystemTypes.AllowPartiallyTrustedCallersAttribute) || node10.Type.FullName.Equals("System.Security.SecurityRules")))
                        {
                            object[] objArray16 = new object[] { node10.Type.FullName, node2.Name };
                            this.WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly", objArray16);
                            node2.Attributes[num14] = null;
                        }
                    }
                    num14++;
                }
            }
            AssemblyNode a = (node2 != null) ? node2 : node;
            if (!this.copyattrs)
            {
                object[] objArray17 = new object[] { a.Name };
                this.WriteToLog("Using assembly '{0}' for assembly-level attributes for the target assembly.", objArray17);
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
                AssemblyNode node11 = assems[num16];
                object[] objArray18 = new object[] { node11.Name };
                this.WriteToLog("Merging assembly '{0}' into target assembly.", objArray18);
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
                        int num19 = (node11.AssemblyReferences == null) ? 0 : node11.AssemblyReferences.Count;
                        while (num18 < num19)
                        {
                            list2.Add(node11.AssemblyReferences[num18].Assembly);
                            num18++;
                        }
                        this.d.FindTypesToBeDuplicated(node11.Types);
                    }
                    else
                    {
                        int num20 = 0;
                        int num21 = (node11.AssemblyReferences == null) ? 0 : node11.AssemblyReferences.Count;
                        while (num20 < num21)
                        {
                            AssemblyNode assembly = node11.AssemblyReferences[num20].Assembly;
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
                                list2.Add(node11.AssemblyReferences[num20].Assembly);
                            }
                            num20++;
                        }
                    }
                    flag4 = this.MergeInAssembly_Union(node11, targetAssemblyIsComVisible);
                }
                else
                {
                    flag4 = this.MergeInAssembly(node11, makeNonPublic, targetAssemblyIsComVisible);
                }
                if (!flag4)
                {
                    this.WriteToLog("Could not merge in assembly. Skipping and processing rest of arguments.", new object[0]);
                }
                num16++;
            }
            TypeNode node4 = null;
            if ((a.Types != null) && (a.Types.Count > 0))
            {
                node4 = a.Types[0];
            }
            if (((node4 == null) && (node.Types != null)) && (node.Types.Count > 0))
            {
                node4 = node.Types[0];
            }
            if (node4 != null)
            {
                this.hiddenClass.Flags = node4.Flags;
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
                object[] objArray19 = new object[] { a.Name };
                this.WriteToLog("No Win32 Resources found in assembly '{0}'; target assembly will (also) not have any.", objArray19);
            }
            else
            {
                int capacity = a.Win32Resources.Count;
                object[] objArray20 = new object[] { a.Name, capacity };
                this.WriteToLog("Copying {1} Win32 Resources from assembly '{0}' into target assembly.", objArray20);
                this.targetAssembly.Win32Resources = new Win32ResourceList(capacity);
                int num25 = 0;
                int num26 = capacity;
                while (num25 < num26)
                {
                    this.targetAssembly.Win32Resources.Add(a.Win32Resources[num25]);
                    num25++;
                }
            }
            TypeNode[] types = new TypeNode[] { SystemTypes.Boolean };
            InstanceInitializer constructor = SystemTypes.ComVisibleAttribute.GetConstructor(types);
            Expression[] elements = new Expression[] { new Literal(targetAssemblyIsComVisible, SystemTypes.Boolean) };
            AttributeNode element = new AttributeNode(new MemberBinding(null, constructor), new ExpressionList(elements));
            this.targetAssembly.Attributes.Add(element);
            if ((this.targetAssembly.Kind == ModuleKindFlags.ConsoleApplication) || (this.targetAssembly.Kind == ModuleKindFlags.WindowsApplication))
            {
                if (node.EntryPoint == null)
                {
                    object[] objArray21 = new object[] { node.Name };
                    this.WriteToLog("Trying to make the target assembly an executable, but cannot find an entry point in the primary assembly, '{0}'.", objArray21);
                    this.WriteToLog("Converting target assembly into a dll.", new object[0]);
                    this.targetAssembly.Kind = ModuleKindFlags.DynamicallyLinkedLibrary;
                    this.outputFileName = Path.ChangeExtension(this.outputFileName, "dll");
                }
                else
                {
                    System.Compiler.Method entryPoint = node.EntryPoint;
                    string[] textArray2 = new string[] { "entry point '", entryPoint.FullName, "' from assembly '", node.Name, "' to assembly '", this.targetAssembly.Name, "'." };
                    string str26 = string.Concat(textArray2);
                    this.WriteToLog("Transferring " + str26, new object[0]);
                    Class declaringType = (Class) entryPoint.DeclaringType;
                    int num1 = ((Class) this.d.DuplicateFor[declaringType.UniqueKey]).Members.Count;
                    this.targetAssembly.EntryPoint = (System.Compiler.Method) this.d.DuplicateFor[entryPoint.UniqueKey];
                    if (this.targetAssembly.EntryPoint == null)
                    {
                        this.WriteToLog("Error in transferring " + str26, new object[0]);
                        throw new InvalidOperationException("Error in transferring " + str26);
                    }
                }
            }
            if (this.keyContainer != null)
            {
                this.targetAssembly.KeyContainerName = this.keyContainer;
            }
            else if (this.keyfile != null)
            {
                if (!File.Exists(this.keyfile))
                {
                    object[] objArray22 = new object[] { this.keyfile };
                    this.WriteToLog("ILMerge: Cannot open key file: '{0}'. Not trying to sign output.", objArray22);
                }
                else
                {
                    FileStream stream = File.Open(this.keyfile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    int length = (int) stream.Length;
                    byte[] buffer = new byte[length];
                    if (stream.Read(buffer, 0, length) != length)
                    {
                        object[] objArray23 = new object[] { this.keyfile };
                        this.WriteToLog("ILMerge: Error reading contents of key file: '{0}'. Not trying to sign output.", objArray23);
                    }
                    else
                    {
                        object[] objArray24 = new object[] { this.keyfile };
                        this.WriteToLog("ILMerge: Signing assembly with the key file '{0}'.", objArray24);
                        this.targetAssembly.KeyBlob = new byte[length];
                        Array.Copy(buffer, 0, this.targetAssembly.KeyBlob, 0, length);
                    }
                    stream.Close();
                }
            }
            else if ((node.PublicKeyOrToken != null) && (node.PublicKeyOrToken.Length != 0))
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
            if (this.externalVisitor != null)
            {
                this.externalVisitor.Visit(this.targetAssembly);
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
            object[] args = new object[] { this.outputFileName };
            this.WriteToLog("ILMerge: Writing target assembly '{0}'.", args);
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
                        object[] objArray27 = new object[] { this.outputFileName };
                        this.WriteToLog("ILMerge: Signed assembly '{0}' with a strong name.", objArray27);
                    }
                }
                catch (AssemblyCouldNotBeSignedException exception5)
                {
                    if (exception5.Message == "Assembly could not be signed.")
                    {
                        this.WriteToLog("ILMerge error: The target assembly was not able to be strongly named (did you forget to use the /delaysign option?).", new object[0]);
                        if (exception5.InnerException != null)
                        {
                            this.WriteToLog(exception5.InnerException.Message, new object[0]);
                        }
                    }
                    else
                    {
                        this.WriteToLog("ILMerge error: The target assembly was not able to be strongly named. " + exception5.Message, new object[0]);
                    }
                }
            }
            else
            {
                if (this.targetAssembly.Attributes == null)
                {
                    this.targetAssembly.Attributes = new AttributeList(1);
                }
                int num28 = 0;
                int num29 = this.targetAssembly.Attributes.Count;
                while (num28 < num29)
                {
                    AttributeNode node14 = this.targetAssembly.Attributes[num28];
                    if ((node14 != null) && (node14.Type == SystemTypes.AssemblyDelaySignAttribute))
                    {
                        break;
                    }
                    num28++;
                }
                if (num28 == num29)
                {
                    TypeNode[] nodeArray2 = new TypeNode[] { SystemTypes.Boolean };
                    InstanceInitializer boundMember = SystemTypes.AssemblyDelaySignAttribute.GetConstructor(nodeArray2);
                    if (boundMember != null)
                    {
                        Expression[] expressionArray2 = new Expression[] { Literal.True };
                        AttributeNode node15 = new AttributeNode(new MemberBinding(null, boundMember), new ExpressionList(expressionArray2));
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
                object[] objArray26 = new object[] { this.outputFileName };
                this.WriteToLog("ILMerge: Delay signed assembly '{0}'.", objArray26);
            }
            int num30 = 0;
            int num31 = this.targetAssembly.AssemblyReferences.Count;
            while (num30 < num31)
            {
                AssemblyNode node16 = this.targetAssembly.AssemblyReferences[num30].Assembly;
                if (string.Compare(node16.Name, this.targetAssembly.Name, true) == 0)
                {
                    string str27 = "The target assembly '" + this.targetAssembly.Name + "' lists itself as an external reference.";
                    this.WriteToLog(str27, new object[0]);
                    throw new InvalidOperationException("ILMerge.Merge: " + str27);
                }
                int num32 = 0;
                int num33 = assems.Count;
                while (num32 < num33)
                {
                    if (node16 == assems[num32])
                    {
                        string str28 = "The assembly '" + assems[num32].Name + "' was not merged in correctly. It is still listed as an external reference in the target assembly.";
                        this.WriteToLog(str28, new object[0]);
                        throw new InvalidOperationException("ILMerge.Merge: " + str28);
                    }
                    num32++;
                }
                num30++;
            }
            try
            {
                for (int i = 0; i < this.targetAssembly.ModuleReferences.Count; i++)
                {
                    System.Compiler.Module module = this.targetAssembly.ModuleReferences[i].Module;
                    object[] objArray28 = new object[] { module.Name, module.Location };
                    this.WriteToLog("Location for referenced module '{0}' is '{1}'", objArray28);
                }
                for (int j = 0; j < this.targetAssembly.AssemblyReferences.Count; j++)
                {
                    AssemblyNode node17 = this.targetAssembly.AssemblyReferences[j].Assembly;
                    object[] objArray29 = new object[] { node17.Name, node17.Location };
                    this.WriteToLog("Location for referenced assembly '{0}' is '{1}'", objArray29);
                    if ((node17.MetadataImportErrors != null) && (node17.MetadataImportErrors.Count > 0))
                    {
                        object[] objArray30 = new object[] { node17.Name };
                        this.WriteToLog("\tThere were errors reported in {0}'s metadata.", objArray30);
                        foreach (Exception exception6 in node17.MetadataImportErrors)
                        {
                            this.WriteToLog("\t" + exception6.Message, new object[0]);
                        }
                    }
                    else
                    {
                        object[] objArray31 = new object[] { node17.Name };
                        this.WriteToLog("\tThere were no errors reported in  {0}'s metadata.", objArray31);
                    }
                }
            }
            catch (Exception exception7)
            {
                this.WriteToLog("Exception occurred while trying to print out information on references.", new object[0]);
                this.WriteToLog(exception7.ToString(), new object[0]);
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
                    AttributeNode current = enumerator.Current;
                    if (current != null)
                    {
                        if (flag)
                        {
                            switch (current.Type.FullName)
                            {
                                case "System.Security.AllowPartiallyTrustedCallersAttribute":
                                case "System.Security.SecurityCriticalAttribute":
                                case "System.Security.SecurityTransparentAttribute":
                                {
                                    continue;
                                }
                            }
                        }
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
                        string s = $"ERROR!!: Duplicate type '{t.FullName}' found in assembly '{t.DeclaringModule.Name}'. Do you want to use the /allowDup option?";
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
                    object[] args = new object[] { t.FullName, t.DeclaringModule.Name, str2 };
                    this.WriteToLog("Duplicate type name: modifying name of the type '{0}' (from assembly '{1}') to '{2}'", args);
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
                            object[] objArray2 = new object[] { resource.Name, t.DeclaringModule.Name, str2 };
                            this.WriteToLog("Duplicate resource name: modifying name of the resource '{0}' (from assembly '{1}') to '{2}.resources'", objArray2);
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
                TypeNode[] types = new TypeNode[] { SystemTypes.Boolean };
                InstanceInitializer constructor = SystemTypes.ComVisibleAttribute.GetConstructor(types);
                Expression[] elements = new Expression[] { new Literal(comVisibleSettingForAssembly, SystemTypes.Boolean) };
                assemblyComVisibleAttribute = new AttributeNode(new MemberBinding(null, constructor), new ExpressionList(elements));
            }
            int num8 = 0;
            int num9 = (a.Attributes == null) ? 0 : a.Attributes.Count;
            while (num8 < num9)
            {
                AttributeNode node5 = a.Attributes[num8];
                if (node5 != null)
                {
                    if (node5.Type == SystemTypes.ComVisibleAttribute)
                    {
                        a.Attributes[num8] = null;
                    }
                    else if (((node5.Type == SystemTypes.SecurityCriticalAttribute) || (node5.Type == SystemTypes.SecurityTransparentAttribute)) || ((node5.Type == SystemTypes.AllowPartiallyTrustedCallersAttribute) || node5.Type.FullName.Equals("System.Security.SecurityRules")))
                    {
                        object[] objArray3 = new object[] { node5.Type.FullName, a.Name };
                        this.WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly", objArray3);
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
                TypeNode node7 = this.d.Visit(node) as TypeNode;
                if (node7 != null)
                {
                    if ((makeNonPublic && (node7.DeclaringType == null)) && (!this.ExemptType(node) && ((node7.Flags & TypeFlags.NestedFamORAssem) == TypeFlags.Public)))
                    {
                        node7.Flags &= ~TypeFlags.Public;
                    }
                    this.AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(node7, assemblyComVisibleAttribute);
                    this.targetAssembly.Types.Add(node7);
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
                TypeNode[] types = new TypeNode[] { SystemTypes.Boolean };
                InstanceInitializer constructor = SystemTypes.ComVisibleAttribute.GetConstructor(types);
                Expression[] elements = new Expression[] { new Literal(comVisibleSettingForAssembly, SystemTypes.Boolean) };
                assemblyComVisibleAttribute = new AttributeNode(new MemberBinding(null, constructor), new ExpressionList(elements));
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
                        object[] args = new object[] { node3.Type.FullName, a.Name };
                        this.WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly", args);
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
                        TypeNode[] nodeArray2 = new TypeNode[] { node };
                        this.d.FindTypesToBeDuplicated(new TypeNodeList(nodeArray2));
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
            while ((index < length) & flag)
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
                            goto Label_01DA;
                        }
                        if (str4 == "")
                        {
                            goto Label_01C9;
                        }
                        if (str4 != "library")
                        {
                            if (str4 == "exe")
                            {
                                goto Label_01B1;
                            }
                            if (str4 == "winexe")
                            {
                                goto Label_01BD;
                            }
                        }
                        else
                        {
                            this.targetKind = Kind.Dll;
                        }
                    }
                }
                goto Label_07F0;
            Label_01B1:
                this.targetKind = Kind.Exe;
                goto Label_07F0;
            Label_01BD:
                this.targetKind = Kind.WinExe;
                goto Label_07F0;
            Label_01C9:
                flag = false;
                Console.WriteLine("/target given without an accompanying kind.");
                goto Label_07F0;
            Label_01DA:
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
                            string str5 = str4;
                            int num4 = str5.IndexOf(",");
                            if (num4 > 0)
                            {
                                this.clrVersion = str5.Substring(0, num4);
                                this.clrDir = str5.Substring(num4 + 1);
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
                else if (string.Compare(str3, "keycontainer", true) == 0)
                {
                    if (str4 != "")
                    {
                        this.keyContainer = str4;
                    }
                    this.keyContainerSpecified = true;
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
            Label_07F0:
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
            if ((assems != null) && (assems.Length != 0))
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
            if ((dirs != null) && (dirs.Length != 0))
            {
                this.directories = new string[dirs.Length];
                dirs.CopyTo(this.directories, 0);
            }
        }

        public void SetTargetPlatform(string platform, string dir)
        {
            switch (<PrivateImplementationDetails>.ComputeStringHash(platform))
            {
                case 0x281e0ec3:
                    if (platform == "2.0")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x2ed6cf7a:
                    if (platform == "V4.5")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x1cf6d253:
                    if (platform == "4_5")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x1ff6d70c:
                    if (platform == "4_0")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x310ca263:
                    if (platform == "4")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x340ca71c:
                    if (platform == "1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x370cabd5:
                    if (platform == "2")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x4105d40a:
                    if (platform == "2_0")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x706c3b58:
                    if (platform == "1_1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x716c3ceb:
                    if (platform == "1_0")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x46c2dd56:
                    if (platform == "postv1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x5f67ee9b:
                    if (platform == "postv1.1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x93fa9488:
                    if (platform == "V1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x944a4668:
                    if (platform == "v1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x96fa9941:
                    if (platform == "V2")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x974a4b21:
                    if (platform == "v2")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xa120f026:
                    if (platform == "4.5")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xa28344ef:
                    if (platform == "post1.1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x98fa9c67:
                    if (platform == "V4")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0x994a4e47:
                    if (platform == "v4")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xa5bf16a2:
                    if (platform == "post1_1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xa620f805:
                    if (platform == "4.0")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xaeaa835a:
                    if (platform == "v4.5")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xc673f4f6:
                    if (platform == "postV1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xe254d7ee:
                    if (platform == "postv1_1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xeb421bf2:
                    if (platform == "1.0")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xd2eea9bb:
                    if (platform == "postV1.1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xd67a2d4e:
                    if (platform == "postV1_1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xec421d85:
                    if (platform == "1.1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xf97fe4b9:
                    if (platform == "v1.1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xfaea2ed9:
                    if (platform == "V1.1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xfd5b1a2c:
                    if (platform == "v1_1")
                    {
                        goto Label_04D9;
                    }
                    break;

                case 0xff63350c:
                    if (platform == "V1_1")
                    {
                        goto Label_04D9;
                    }
                    break;
            }
            throw new ArgumentException("Platform '" + platform + "' not recognized.");
        Label_04D9:
            if (((dir != null) && (dir != "")) && !Directory.Exists(dir))
            {
                throw new ArgumentException("Directory '" + dir + "' doesn't exist.");
            }
            switch (<PrivateImplementationDetails>.ComputeStringHash(platform))
            {
                case 0x1cf6d253:
                    if (platform == "4_5")
                    {
                        goto Label_09FC;
                    }
                    goto Label_0A06;

                case 0x1ff6d70c:
                    if (platform == "4_0")
                    {
                        goto Label_09F2;
                    }
                    goto Label_0A06;

                case 0x281e0ec3:
                    if (platform == "2.0")
                    {
                        goto Label_09E8;
                    }
                    goto Label_0A06;

                case 0x340ca71c:
                    if (platform == "1")
                    {
                        break;
                    }
                    goto Label_0A06;

                case 0x370cabd5:
                    if (platform == "2")
                    {
                        goto Label_09E8;
                    }
                    goto Label_0A06;

                case 0x2ed6cf7a:
                    if (platform == "V4.5")
                    {
                        goto Label_09FC;
                    }
                    goto Label_0A06;

                case 0x310ca263:
                    if (platform == "4")
                    {
                        goto Label_09F2;
                    }
                    goto Label_0A06;

                case 0x706c3b58:
                    if (platform == "1_1")
                    {
                        goto Label_09AF;
                    }
                    goto Label_0A06;

                case 0x716c3ceb:
                    if (platform == "1_0")
                    {
                        break;
                    }
                    goto Label_0A06;

                case 0x4105d40a:
                    if (platform == "2_0")
                    {
                        goto Label_09E8;
                    }
                    goto Label_0A06;

                case 0x5f67ee9b:
                    if (platform == "postv1.1")
                    {
                        goto Label_09BE;
                    }
                    goto Label_0A06;

                case 0x93fa9488:
                    if (platform == "V1")
                    {
                        break;
                    }
                    goto Label_0A06;

                case 0x944a4668:
                    if (platform == "v1")
                    {
                        break;
                    }
                    goto Label_0A06;

                case 0x96fa9941:
                    if (platform == "V2")
                    {
                        goto Label_09E8;
                    }
                    goto Label_0A06;

                case 0x974a4b21:
                    if (platform == "v2")
                    {
                        goto Label_09E8;
                    }
                    goto Label_0A06;

                case 0xa120f026:
                    if (platform == "4.5")
                    {
                        goto Label_09FC;
                    }
                    goto Label_0A06;

                case 0xa28344ef:
                    if (platform == "post1.1")
                    {
                        goto Label_09BE;
                    }
                    goto Label_0A06;

                case 0x98fa9c67:
                    if (platform == "V4")
                    {
                        goto Label_09F2;
                    }
                    goto Label_0A06;

                case 0x994a4e47:
                    if (platform == "v4")
                    {
                        goto Label_09F2;
                    }
                    goto Label_0A06;

                case 0xa5bf16a2:
                    if (platform == "post1_1")
                    {
                        goto Label_09BE;
                    }
                    goto Label_0A06;

                case 0xa620f805:
                    if (platform == "4.0")
                    {
                        goto Label_09F2;
                    }
                    goto Label_0A06;

                case 0xaeaa835a:
                    if (platform == "v4.5")
                    {
                        goto Label_09FC;
                    }
                    goto Label_0A06;

                case 0xd2eea9bb:
                    if (platform == "postV1.1")
                    {
                        goto Label_09BE;
                    }
                    goto Label_0A06;

                case 0xeb421bf2:
                    if (platform == "1.0")
                    {
                        break;
                    }
                    goto Label_0A06;

                case 0xec421d85:
                    if (platform == "1.1")
                    {
                        goto Label_09AF;
                    }
                    goto Label_0A06;

                case 0xd67a2d4e:
                    if (platform == "postV1_1")
                    {
                        goto Label_09BE;
                    }
                    goto Label_0A06;

                case 0xe254d7ee:
                    if (platform == "postv1_1")
                    {
                        goto Label_09BE;
                    }
                    goto Label_0A06;

                case 0xf97fe4b9:
                    if (platform == "v1.1")
                    {
                        goto Label_09AF;
                    }
                    goto Label_0A06;

                case 0xfaea2ed9:
                    if (platform == "V1.1")
                    {
                        goto Label_09AF;
                    }
                    goto Label_0A06;

                case 0xfd5b1a2c:
                    if (platform == "v1_1")
                    {
                        goto Label_09AF;
                    }
                    goto Label_0A06;

                case 0xff63350c:
                    if (platform == "V1_1")
                    {
                        goto Label_09AF;
                    }
                    goto Label_0A06;

                default:
                    goto Label_0A06;
            }
            System.Version version = new System.Version(1, 0, 0xce4);
            goto Label_0A1C;
        Label_09AF:
            version = new System.Version(1, 0, 0x1388);
            goto Label_0A1C;
        Label_09BE:
            if ((dir == null) || (dir == ""))
            {
                throw new ArgumentException("Directory must be specified for setting target platform to postv1.1.");
            }
            version = new System.Version(1, 1, 0x270f);
            goto Label_0A1C;
        Label_09E8:
            version = new System.Version(2, 0);
            goto Label_0A1C;
        Label_09F2:
            version = new System.Version(4, 0);
            goto Label_0A1C;
        Label_09FC:
            version = new System.Version(4, 5);
            goto Label_0A1C;
        Label_0A06:
            throw new ArgumentException("Platform '" + platform + "' not recognized.");
        Label_0A1C:
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
            object[] args = new object[] { platform, dir };
            this.WriteToLog("Set platform to '{0}', using directory '{1}' for mscorlib.dll", args);
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
            if (this.keyfileSpecified && (this.keyfile == null))
            {
                flag = true;
                Console.WriteLine("/keyfile option given, but no file name.");
            }
            if (this.keyContainerSpecified && (this.keyContainer == null))
            {
                flag = true;
                Console.WriteLine("/keycontainer option given, but no container name.");
            }
            if ((this.delaySign && !this.keyfileSpecified) && !this.keyContainerSpecified)
            {
                flag = true;
                Console.WriteLine("/delaysign option given, but not the /keyfile or /keycontainer options.");
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
                    StreamWriter writer1 = new StreamWriter(this.logFile, true);
                    writer1.WriteLine(s, args);
                    writer1.Close();
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

        public StandardVisitor ExternalVisitor
        {
            set
            {
                this.externalVisitor = value;
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

        public string KeyContainer
        {
            get => 
                this.keyContainer;
            set
            {
                this.keyContainer = value;
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
            "Usage: ilmerge [/lib:directory]* [/log[:filename]] [/keyfile:filename | /keycontainer:containername [/delaysign]] [/internalize[:filename]] [/t[arget]:(library|exe|winexe)] [/closed] [/ndebug] [/ver:version] [/copyattrs [/allowMultiple] [/keepFirst]] [/xmldocs] [/attr:filename] [/targetplatform:<version>[,<platformdir>] | /v1 | /v1.1 | /v2 | /v4] [/useFullPublicKeyForReferences] [/wildcards] [/zeroPeKind] [/allowDup:type]* [/union] [/align:n] /out:filename <primary assembly> [<other assemblies>...]";

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
                            goto Label_0167;
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
            Label_0167:
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

