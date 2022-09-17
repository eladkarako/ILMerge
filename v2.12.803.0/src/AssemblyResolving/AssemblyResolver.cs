namespace AssemblyResolving
{
    using System;
    using System.Collections;
    using System.Compiler;
    using System.IO;

    internal class AssemblyResolver
    {
        private bool debugInfo;
        private string[] directories;
        private static readonly string[] exts = new string[] { "dll", "exe" };
        private IDictionary h;
        private string inputDirectory;
        private bool log;
        private string logFile;
        private bool shortB;
        private bool useGlobalCache;

        public AssemblyResolver()
        {
            this.inputDirectory = "";
            this.log = true;
            this.useGlobalCache = true;
            this.debugInfo = true;
        }

        public AssemblyResolver(IDictionary AssemblyCache)
        {
            this.inputDirectory = "";
            this.log = true;
            this.useGlobalCache = true;
            this.debugInfo = true;
            this.h = AssemblyCache;
        }

        public AssemblyResolver(string InputDirectory)
        {
            this.inputDirectory = "";
            this.log = true;
            this.useGlobalCache = true;
            this.debugInfo = true;
            if (InputDirectory == null)
            {
                throw new ArgumentNullException();
            }
            this.inputDirectory = InputDirectory;
        }

        public AssemblyNode Resolve(AssemblyReference assemblyReference, Module referencingModule)
        {
            this.WriteToLog("AssemblyResolver: Assembly '{0}' is referencing assembly '{1}'.", new object[] { referencingModule.Name, assemblyReference.Name });
            AssemblyNode node = null;
            try
            {
                this.WriteToLog("\tAssemblyResolver: Attempting referencing assembly's directory.", new object[0]);
                if (referencingModule.Directory != null)
                {
                    foreach (string str in exts)
                    {
                        bool debugInfo = this.debugInfo;
                        string path = Path.Combine(referencingModule.Directory, assemblyReference.Name + "." + str);
                        if (File.Exists(path))
                        {
                            if (debugInfo && !File.Exists(Path.Combine(referencingModule.Directory, assemblyReference.Name + ".pdb")))
                            {
                                this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", new object[] { assemblyReference.Name });
                                debugInfo = false;
                            }
                            this.WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used referencing Module's directory.)", new object[] { assemblyReference.Name, path });
                            node = AssemblyNode.GetAssembly(path, this.h, true, debugInfo, this.useGlobalCache, this.shortB);
                            break;
                        }
                    }
                }
                else
                {
                    this.WriteToLog("\t\tAssemblyResolver: Referencing assembly's directory is null.", new object[0]);
                }
                if (node == null)
                {
                    if (referencingModule.Directory != null)
                    {
                        this.WriteToLog("\tAssemblyResolver: Did not find assembly in referencing assembly's directory.", new object[0]);
                    }
                    this.WriteToLog("\tAssemblyResolver: Attempting input directory.", new object[0]);
                    foreach (string str4 in exts)
                    {
                        bool getDebugInfo = this.debugInfo;
                        string str5 = Path.Combine(this.inputDirectory, assemblyReference.Name + "." + str4);
                        if (File.Exists(str5))
                        {
                            this.WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used the original input directory.)", new object[] { assemblyReference.Name, str5 });
                            if (getDebugInfo && !File.Exists(Path.Combine(referencingModule.Directory, assemblyReference.Name + ".pdb")))
                            {
                                this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", new object[] { assemblyReference.Name });
                                getDebugInfo = false;
                            }
                            node = AssemblyNode.GetAssembly(str5, this.h, true, getDebugInfo, this.useGlobalCache, this.shortB);
                            break;
                        }
                    }
                    if (node == null)
                    {
                        this.WriteToLog("\tAssemblyResolver: Did not find assembly in input directory.", new object[0]);
                        this.WriteToLog("\tAssemblyResolver: Attempting user-supplied directories.", new object[0]);
                        if (this.directories != null)
                        {
                            foreach (string str7 in this.directories)
                            {
                                foreach (string str8 in exts)
                                {
                                    string str9 = str7 + @"\" + assemblyReference.Name + "." + str8;
                                    if (File.Exists(str9))
                                    {
                                        bool flag3 = this.debugInfo;
                                        this.WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used a client-supplied directory.)", new object[] { assemblyReference.Name, str9 });
                                        if (flag3 && !File.Exists(Path.Combine(referencingModule.Directory, assemblyReference.Name + ".pdb")))
                                        {
                                            this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", new object[] { assemblyReference.Name });
                                            flag3 = false;
                                        }
                                        node = AssemblyNode.GetAssembly(str9, this.h, true, flag3, this.useGlobalCache, this.shortB);
                                        break;
                                    }
                                }
                                if (node != null)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            this.WriteToLog("\tAssemblyResolver: No user-supplied directories.", new object[0]);
                        }
                        if (node == null)
                        {
                            if (this.directories != null)
                            {
                                this.WriteToLog("\tAssemblyResolver: Did not find assembly in user-supplied directories.", new object[0]);
                            }
                            this.WriteToLog("\tAssemblyResolver: Attempting framework directory.", new object[0]);
                            if (TargetPlatform.PlatformAssembliesLocation != null)
                            {
                                string platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation;
                                foreach (string str12 in exts)
                                {
                                    bool flag4 = this.debugInfo;
                                    string str13 = Path.Combine(platformAssembliesLocation, assemblyReference.Name + "." + str12);
                                    if (File.Exists(str13))
                                    {
                                        if (flag4 && !File.Exists(Path.Combine(platformAssembliesLocation, assemblyReference.Name + ".pdb")))
                                        {
                                            this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", new object[] { assemblyReference.Name });
                                            flag4 = false;
                                        }
                                        this.WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used framework directory.)", new object[] { assemblyReference.Name, str13 });
                                        node = AssemblyNode.GetAssembly(str13, this.h, true, flag4, this.useGlobalCache, this.shortB);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                this.WriteToLog("\t\tAssemblyResolver: Platform assemblies location is null.", new object[0]);
                            }
                            if ((node == null) && (referencingModule.Directory != null))
                            {
                                this.WriteToLog("\tAssemblyResolver: Did not find assembly in framework directory.", new object[0]);
                            }
                        }
                    }
                }
                if (node == null)
                {
                    this.WriteToLog("AssemblyResolver: Unable to resolve reference. (It still might be found, e.g., in the GAC.)", new object[0]);
                }
            }
            catch (Exception exception)
            {
                this.WriteToLog("AssemblyResolver: Exception occurred. Unable to resolve reference.", new object[0]);
                this.WriteToLog("Inner exception: " + exception.ToString(), new object[0]);
            }
            return node;
        }

        private void WriteToLog(string s, params object[] args)
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

        public IDictionary AssemblyCache
        {
            set
            {
                this.h = value;
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

        public string InputDirectory
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                this.inputDirectory = value;
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
                if (this.logFile != null)
                {
                    throw new InvalidOperationException("AssemblyResolver: Can set the log only once.");
                }
                this.logFile = value;
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

        public string[] SearchDirectories
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (value.Length > 0)
                {
                    this.directories = new string[value.Length];
                    value.CopyTo(this.directories, 0);
                }
            }
        }

        public bool UseGlobalCache
        {
            get => 
                this.useGlobalCache;
            set
            {
                this.useGlobalCache = value;
            }
        }
    }
}

