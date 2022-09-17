namespace System.Compiler
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Specialized;

    internal class CompilerOptions : CompilerParameters
    {
        public StringList AdditionalSearchPaths;
        public StringCollection AliasesForReferencedAssemblies;
        public bool AllowUnsafeCode;
        public string AssemblyKeyFile;
        public string AssemblyKeyName;
        public long BaseAddress;
        public string BugReportFileName;
        public bool CheckedArithmetic;
        public object CodePage;
        public bool CompileAndExecute;
        public StringList DefinedPreProcessorSymbols;
        public bool DelaySign;
        public bool DisplayCommandLineHelp;
        public bool EmitManifest;
        public bool EncodeOutputInUTF8;
        public string ExplicitOutputExtension;
        public int FileAlignment;
        public bool FullyQualifyPaths;
        public bool HeuristicReferenceResolution;
        public bool IncrementalCompile;
        public bool MayLockFiles;
        public ModuleKindFlags ModuleKind;
        public bool NoStandardLibrary;
        public bool Optimize;
        public string OutputPath;
        public bool PDBOnly;
        public string RecursiveWildcard;
        public StringList ReferencedModules;
        public string RootNamespace;
        public string ShadowedAssembly;
        public CompilerSite Site;
        public Int32List SpecificWarningsNotToTreatAsErrors;
        public Int32List SpecificWarningsToTreatAsErrors;
        public string StandardLibraryLocation;
        public Int32List SuppressedWarnings;
        public bool SuppressLogo;
        public AppDomain TargetAppDomain;
        public System.Compiler.TargetInformation TargetInformation;
        public PlatformType TargetPlatform;
        public string TargetPlatformLocation;
        public ProcessorType TargetProcessor;
        public object UserLocaleId;
        public bool UseStandardConfigFile;
        public string Win32Icon;
        public string XMLDocFileName;

        public CompilerOptions()
        {
            this.EmitManifest = true;
        }

        public CompilerOptions(CompilerOptions source)
        {
            this.EmitManifest = true;
            if (source != null)
            {
                this.AdditionalSearchPaths = source.AdditionalSearchPaths;
                this.AliasesForReferencedAssemblies = source.AliasesForReferencedAssemblies;
                this.AllowUnsafeCode = source.AllowUnsafeCode;
                this.AssemblyKeyFile = source.AssemblyKeyFile;
                this.AssemblyKeyName = source.AssemblyKeyName;
                this.BaseAddress = source.BaseAddress;
                this.BugReportFileName = source.BugReportFileName;
                this.CheckedArithmetic = source.CheckedArithmetic;
                this.CodePage = source.CodePage;
                this.CompileAndExecute = source.CompileAndExecute;
                base.CompilerOptions = source.CompilerOptions;
                this.DefinedPreProcessorSymbols = source.DefinedPreProcessorSymbols;
                this.DelaySign = source.DelaySign;
                this.DisplayCommandLineHelp = source.DisplayCommandLineHelp;
                if (source.EmbeddedResources != null)
                {
                    foreach (string str in source.EmbeddedResources)
                    {
                        base.EmbeddedResources.Add(str);
                    }
                }
                this.EmitManifest = source.EmitManifest;
                this.EncodeOutputInUTF8 = source.EncodeOutputInUTF8;
                base.Evidence = source.Evidence;
                this.ExplicitOutputExtension = source.ExplicitOutputExtension;
                this.FileAlignment = source.FileAlignment;
                this.FullyQualifyPaths = source.FullyQualifyPaths;
                base.GenerateExecutable = source.GenerateExecutable;
                base.GenerateInMemory = source.GenerateInMemory;
                this.HeuristicReferenceResolution = source.HeuristicReferenceResolution;
                base.IncludeDebugInformation = source.IncludeDebugInformation;
                this.IncrementalCompile = source.IncrementalCompile;
                if (source.LinkedResources != null)
                {
                    foreach (string str2 in source.LinkedResources)
                    {
                        base.LinkedResources.Add(str2);
                    }
                }
                base.MainClass = source.MainClass;
                this.MayLockFiles = source.MayLockFiles;
                this.ModuleKind = source.ModuleKind;
                this.NoStandardLibrary = source.NoStandardLibrary;
                this.Optimize = source.Optimize;
                base.OutputAssembly = source.OutputAssembly;
                this.OutputPath = source.OutputPath;
                this.PDBOnly = source.PDBOnly;
                this.RecursiveWildcard = source.RecursiveWildcard;
                if (source.ReferencedAssemblies != null)
                {
                    foreach (string str3 in source.ReferencedAssemblies)
                    {
                        base.ReferencedAssemblies.Add(str3);
                    }
                }
                this.ReferencedModules = source.ReferencedModules;
                this.RootNamespace = source.RootNamespace;
                this.ShadowedAssembly = source.ShadowedAssembly;
                this.SpecificWarningsToTreatAsErrors = source.SpecificWarningsToTreatAsErrors;
                this.StandardLibraryLocation = source.StandardLibraryLocation;
                this.SuppressLogo = source.SuppressLogo;
                this.SuppressedWarnings = source.SuppressedWarnings;
                this.TargetAppDomain = source.TargetAppDomain;
                this.TargetInformation = source.TargetInformation;
                this.TargetPlatform = source.TargetPlatform;
                this.TargetPlatformLocation = source.TargetPlatformLocation;
                base.TreatWarningsAsErrors = source.TreatWarningsAsErrors;
                this.UserLocaleId = source.UserLocaleId;
                base.UserToken = source.UserToken;
                base.WarningLevel = source.WarningLevel;
                this.Win32Icon = source.Win32Icon;
                base.Win32Resource = source.Win32Resource;
                this.XMLDocFileName = source.XMLDocFileName;
            }
        }

        public virtual CompilerOptions Clone() => 
            ((CompilerOptions) base.MemberwiseClone());

        public virtual string GetOptionHelp() => 
            null;
    }
}

