namespace AssemblyResolving
{
    using System;
    using System.Collections;
    using System.Compiler;
    using System.IO;

    public class AssemblyResolver
    {
        private bool debugInfo;
        private string[] directories;
        private static readonly string[] exts = new string[] { "dll", "exe", "winmd" };
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
            object[] args = new object[] { referencingModule.Name, assemblyReference.Name };
            this.WriteToLog("AssemblyResolver: Assembly '{0}' is referencing assembly '{1}'.", args);
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
                                object[] objArray2 = new object[] { assemblyReference.Name };
                                this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", objArray2);
                                debugInfo = false;
                            }
                            object[] objArray3 = new object[] { assemblyReference.Name, path };
                            this.WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used referencing Module's directory.)", objArray3);
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
                    foreach (string str3 in exts)
                    {
                        bool getDebugInfo = this.debugInfo;
                        string str4 = Path.Combine(this.inputDirectory, assemblyReference.Name + "." + str3);
                        if (File.Exists(str4))
                        {
                            object[] objArray4 = new object[] { assemblyReference.Name, str4 };
                            this.WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used the original input directory.)", objArray4);
                            if (getDebugInfo && !File.Exists(Path.Combine(referencingModule.Directory, assemblyReference.Name + ".pdb")))
                            {
                                object[] objArray5 = new object[] { assemblyReference.Name };
                                this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", objArray5);
                                getDebugInfo = false;
                            }
                            node = AssemblyNode.GetAssembly(str4, this.h, true, getDebugInfo, this.useGlobalCache, this.shortB);
                            break;
                        }
                    }
                    if (node == null)
                    {
                        this.WriteToLog("\tAssemblyResolver: Did not find assembly in input directory.", new object[0]);
                        this.WriteToLog("\tAssemblyResolver: Attempting user-supplied directories.", new object[0]);
                        if (this.directories != null)
                        {
                            foreach (string str5 in this.directories)
                            {
                                foreach (string str6 in exts)
                                {
                                    string[] textArray1 = new string[] { str5, @"\", assemblyReference.Name, ".", str6 };
                                    string str7 = string.Concat(textArray1);
                                    if (File.Exists(str7))
                                    {
                                        bool flag3 = this.debugInfo;
                                        object[] objArray6 = new object[] { assemblyReference.Name, str7 };
                                        this.WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used a client-supplied directory.)", objArray6);
                                        if (flag3 && !File.Exists(Path.Combine(referencingModule.Directory, assemblyReference.Name + ".pdb")))
                                        {
                                            object[] objArray7 = new object[] { assemblyReference.Name };
                                            this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", objArray7);
                                            flag3 = false;
                                        }
                                        node = AssemblyNode.GetAssembly(str7, this.h, true, flag3, this.useGlobalCache, this.shortB);
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
                                foreach (string str9 in exts)
                                {
                                    bool flag4 = this.debugInfo;
                                    string str10 = Path.Combine(platformAssembliesLocation, assemblyReference.Name + "." + str9);
                                    if (File.Exists(str10))
                                    {
                                        if (flag4 && !File.Exists(Path.Combine(platformAssembliesLocation, assemblyReference.Name + ".pdb")))
                                        {
                                            object[] objArray8 = new object[] { assemblyReference.Name };
                                            this.WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.", objArray8);
                                            flag4 = false;
                                        }
                                        object[] objArray9 = new object[] { assemblyReference.Name, str10 };
                                        this.WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used framework directory.)", objArray9);
                                        node = AssemblyNode.GetAssembly(str10, this.h, true, flag4, this.useGlobalCache, this.shortB);
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
                if (value.Length != 0)
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

