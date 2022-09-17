namespace System.Compiler
{
    using System;
    using System.Collections;
    using System.Compiler.Metadata;
    using System.Globalization;
    using System.IO;

    internal sealed class TargetPlatform
    {
        private static TrivialHashtable assemblyReferenceFor;
        public static bool BusyWithClear;
        public static bool DoNotLockFiles;
        private static readonly string[] FxAssemblyNames = new string[] { 
            "Accessibility", "CustomMarshalers", "IEExecRemote", "IEHost", "IIEHost", "ISymWrapper", "Microsoft.JScript", "Microsoft.VisualBasic", "Microsoft.VisualBasic.Vsa", "Microsoft.VisualC", "Microsoft.Vsa", "Microsoft.Vsa.Vb.CodeDOMProcessor", "mscorcfg", "Regcode", "System", "System.Configuration.Install",
            "System.Data", "System.Design", "System.DirectoryServices", "System.Drawing", "System.Drawing.Design", "System.EnterpriseServices", "System.Management", "System.Messaging", "System.Runtime.Remoting", "System.Runtime.Serialization.Formatters.Soap", "System.Runtime.WindowsRuntime", "System.Security", "System.ServiceProcess", "System.Web", "System.Web.Mobile", "System.Web.RegularExpressions",
            "System.Web.Services", "System.Windows.Forms", "System.Xml", "TlbExpCode", "TlbImpCode", "cscompmgd", "vjswfchtml", "vjswfccw", "VJSWfcBrowserStubLib", "vjswfc", "vjslibcw", "vjslib", "vjscor", "VJSharpCodeProvider"
        };
        private static readonly string[] FxAssemblyToken = new string[] { 
            "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b77a5c561934e089", "b03f5f7f11d50a3a",
            "b77a5c561934e089", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b77a5c561934e089", "b03f5f7f11d50a3a", "b77a5c561934e089", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
            "b03f5f7f11d50a3a", "b77a5c561934e089", "b77a5c561934e089", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a"
        };
        private static readonly string[] FxAssemblyVersion1 = new string[] { 
            "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "7.0.3300.0", "7.0.3300.0", "7.0.3300.0", "7.0.3300.0", "7.0.3300.0", "7.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
            "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "4.0.0.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
            "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "7.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "7.0.3300.0"
        };
        private static readonly string[] FxAssemblyVersion1_1 = new string[] { 
            "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "7.0.5000.0", "7.0.5000.0", "7.0.5000.0", "7.0.5000.0", "7.0.5000.0", "7.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
            "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "4.0.0.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
            "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "7.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "7.0.5000.0"
        };
        private static string[] FxAssemblyVersion2 = new string[] { 
            "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "8.0.0.0", "8.0.0.0", "8.0.0.0", "8.0.0.0", "8.0.0.0", "8.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
            "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "4.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
            "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "8.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0"
        };
        private static string[] FxAssemblyVersion2Build3600 = new string[] { 
            "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "8.0.1200.0", "8.0.1200.0", "8.0.1200.0", "8.0.1200.0", "8.0.1200.0", "8.0.1200.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
            "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "4.0.0.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
            "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "8.0.1200.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "7.0.5000.0"
        };
        private static string[] FxAssemblyVersion4 = new string[] { 
            "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "10.0.0.0", "10.0.0.0", "10.0.0.0", "10.0.0.0", "10.0.0.0", "10.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0",
            "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0",
            "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "10.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0"
        };
        public static char GenericTypeNamesMangleChar = '_';
        public static bool GetDebugInfo;
        public static string PlatformAssembliesLocation = string.Empty;
        public static string TargetRuntimeVersion;
        public static Version TargetVersion = new Version(2, 0, 0xc627);
        private static bool useGenerics;

        private TargetPlatform()
        {
        }

        public static void Clear()
        {
            SystemAssemblyLocation.Location = null;
            SystemDllAssemblyLocation.Location = null;
            SystemRuntimeCollectionsAssemblyLocation.Location = null;
            SystemDiagnosticsDebugAssemblyLocation.Location = null;
            SystemDiagnosticsToolsAssemblyLocation.Location = null;
            SystemGlobalizationAssemblyLocation.Location = null;
            SystemReflectionAssemblyLocation.Location = null;
            SystemResourceManagerAssemblyLocation.Location = null;
            SystemRuntimeExtensionsAssemblyLocation.Location = null;
            SystemRuntimeInteropServicesAssemblyLocation.Location = null;
            SystemRuntimeWindowsRuntimeAssemblyLocation.Location = null;
            SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location = null;
            SystemRuntimeIOServicesAssemblyLocation.Location = null;
            SystemRuntimeSerializationAssemblyLocation.Location = null;
            SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location = null;
            SystemThreadingAssemblyLocation.Location = null;
            DoNotLockFiles = false;
            GetDebugInfo = false;
            PlatformAssembliesLocation = "";
            BusyWithClear = true;
            SystemTypes.Clear();
            BusyWithClear = false;
        }

        private static void InitializeStandardAssemblyLocationsWithDefaultValues(string platformAssembliesLocation)
        {
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, "mscorlib");
        }

        private static void InitializeStandardAssemblyLocationsWithDefaultValues(string platformAssembliesLocation, string mscorlibName)
        {
            SystemAssemblyLocation.Location = platformAssembliesLocation + @"\" + mscorlibName + ".dll";
            if (SystemDllAssemblyLocation.Location == null)
            {
                SystemDllAssemblyLocation.Location = platformAssembliesLocation + @"\system.dll";
            }
        }

        private static void InitializeStandardAssemblyLocationsWithDefaultValues(string platformAssembliesLocation, string mscorlibName, string alternateName)
        {
            if (File.Exists(platformAssembliesLocation + @"\" + mscorlibName + ".dll"))
            {
                InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, mscorlibName);
            }
            else
            {
                InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, alternateName);
            }
        }

        public static void ResetCci(string platformAssembliesLocation, Version targetVersion, bool doNotLockFile, bool getDebugInfo)
        {
            Clear();
            PlatformAssembliesLocation = platformAssembliesLocation;
            if ((targetVersion.Major == 4) && (targetVersion.Minor >= 5))
            {
                InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, "System.Runtime");
                targetVersion = new Version(4, 0, 0, 0);
            }
            else
            {
                InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            }
            TargetVersion = targetVersion;
            assemblyReferenceFor = null;
            SystemTypes.Initialize(doNotLockFile, getDebugInfo);
        }

        public static void SetTo(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException();
            }
            if (version.Major == 1)
            {
                if ((version.Minor == 0) && (version.Build == 0xce4))
                {
                    SetToV1();
                }
                else if ((version.Minor == 0) && (version.Build == 0x1388))
                {
                    SetToV1_1();
                }
                else if ((version.Minor == 1) && (version.Build == 0x270f))
                {
                    SetToPostV1_1(PlatformAssembliesLocation);
                }
            }
            else if (version.Major == 2)
            {
                if ((version.Minor == 0) && (version.Build == 0xe10))
                {
                    SetToV2Beta1();
                }
                else
                {
                    SetToV2();
                }
            }
            else if (version.Major == 4)
            {
                if (version.Minor == 5)
                {
                    SetToV4_5();
                }
                else
                {
                    SetToV4();
                }
            }
            else
            {
                SetToPostV2();
            }
        }

        public static void SetTo(Version version, string platformAssembliesLocation)
        {
            if ((version == null) || (platformAssembliesLocation == null))
            {
                throw new ArgumentNullException();
            }
            if (version.Major == 1)
            {
                if ((version.Minor == 0) && (version.Build == 0xce4))
                {
                    SetToV1(platformAssembliesLocation);
                }
                else if ((version.Minor == 0) && (version.Build == 0x1388))
                {
                    SetToV1_1(platformAssembliesLocation);
                }
                else if ((version.Minor == 1) && (version.Build == 0x270f))
                {
                    SetToPostV1_1(platformAssembliesLocation);
                }
            }
            else if (version.Major == 2)
            {
                if ((version.Minor == 0) && (version.Build == 0xe10))
                {
                    SetToV2Beta1(platformAssembliesLocation);
                }
                else
                {
                    SetToV2(platformAssembliesLocation);
                }
            }
            else if (version.Major == 4)
            {
                SetToV4(platformAssembliesLocation);
            }
            else
            {
                SetToPostV2(platformAssembliesLocation);
            }
        }

        public static void SetToPostV1_1(string platformAssembliesLocation)
        {
            PlatformAssembliesLocation = platformAssembliesLocation;
            TargetVersion = new Version(1, 1, 0x270f);
            TargetRuntimeVersion = "v1.1.9999";
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            assemblyReferenceFor = new TrivialHashtable(0x2e);
            foreach (string str in System.IO.Directory.GetFiles(platformAssembliesLocation, "*.dll"))
            {
                if (str != null)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(str);
                    if (Array.IndexOf<string>(FxAssemblyNames, fileNameWithoutExtension) >= 0)
                    {
                        AssemblyNode assembly = AssemblyNode.GetAssembly(Path.Combine(platformAssembliesLocation, str));
                        if (assembly != null)
                        {
                            assemblyReferenceFor[Identifier.For(assembly.Name).UniqueIdKey] = new AssemblyReference(assembly);
                        }
                    }
                }
            }
        }

        public static void SetToPostV2()
        {
            SetToPostV2(PlatformAssembliesLocation);
        }

        public static void SetToPostV2(string platformAssembliesLocation)
        {
            PlatformAssembliesLocation = platformAssembliesLocation;
            TargetVersion = new Version(2, 1, 0x270f);
            TargetRuntimeVersion = "v2.1.9999";
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            assemblyReferenceFor = new TrivialHashtable(0x2e);
            foreach (string str in System.IO.Directory.GetFiles(platformAssembliesLocation, "*.dll"))
            {
                if (str != null)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(str);
                    if (Array.IndexOf<string>(FxAssemblyNames, fileNameWithoutExtension) >= 0)
                    {
                        string str3 = Path.Combine(platformAssembliesLocation, str);
                        AssemblyReference reference = new AssemblyReference(fileNameWithoutExtension) {
                            Location = str3
                        };
                        assemblyReferenceFor[Identifier.For(fileNameWithoutExtension).UniqueIdKey] = reference;
                    }
                }
            }
        }

        public static void SetToV1()
        {
            SetToV1(PlatformAssembliesLocation);
        }

        public static void SetToV1(string platformAssembliesLocation)
        {
            TargetVersion = new Version(1, 0, 0xce4);
            TargetRuntimeVersion = "v1.0.3705";
            if ((platformAssembliesLocation == null) || (platformAssembliesLocation.Length == 0))
            {
                platformAssembliesLocation = PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), @"..\v1.0.3705");
            }
            else
            {
                PlatformAssembliesLocation = platformAssembliesLocation;
            }
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable hashtable = new TrivialHashtable(0x2e);
            int index = 0;
            int length = FxAssemblyNames.Length;
            while (index < length)
            {
                string name = FxAssemblyNames[index];
                string str2 = FxAssemblyVersion1[index];
                string str3 = FxAssemblyToken[index];
                AssemblyReference reference = new AssemblyReference(name + ", Version=" + str2 + ", Culture=neutral, PublicKeyToken=" + str3) {
                    Location = platformAssembliesLocation + @"\" + name + ".dll"
                };
                hashtable[Identifier.For(name).UniqueIdKey] = reference;
                index++;
            }
            assemblyReferenceFor = hashtable;
        }

        public static void SetToV1_1()
        {
            SetToV1_1(PlatformAssembliesLocation);
        }

        public static void SetToV1_1(string platformAssembliesLocation)
        {
            TargetVersion = new Version(1, 0, 0x1388);
            TargetRuntimeVersion = "v1.1.4322";
            if ((platformAssembliesLocation == null) || (platformAssembliesLocation.Length == 0))
            {
                platformAssembliesLocation = PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), @"..\v1.1.4322");
            }
            else
            {
                PlatformAssembliesLocation = platformAssembliesLocation;
            }
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable hashtable = new TrivialHashtable(0x2e);
            int index = 0;
            int length = FxAssemblyNames.Length;
            while (index < length)
            {
                string name = FxAssemblyNames[index];
                string str2 = FxAssemblyVersion1_1[index];
                string str3 = FxAssemblyToken[index];
                AssemblyReference reference = new AssemblyReference(name + ", Version=" + str2 + ", Culture=neutral, PublicKeyToken=" + str3) {
                    Location = platformAssembliesLocation + @"\" + name + ".dll"
                };
                hashtable[Identifier.For(name).UniqueIdKey] = reference;
                index++;
            }
            assemblyReferenceFor = hashtable;
        }

        public static void SetToV2()
        {
            SetToV2(PlatformAssembliesLocation);
        }

        public static void SetToV2(string platformAssembliesLocation)
        {
            TargetVersion = new Version(2, 0, 0xc627);
            TargetRuntimeVersion = "v2.0.50727";
            GenericTypeNamesMangleChar = '`';
            if ((platformAssembliesLocation == null) || (platformAssembliesLocation.Length == 0))
            {
                platformAssembliesLocation = PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), @"..\v2.0.50727");
            }
            else
            {
                PlatformAssembliesLocation = platformAssembliesLocation;
            }
            PlatformAssembliesLocation = platformAssembliesLocation;
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable hashtable = new TrivialHashtable(0x2e);
            int index = 0;
            int length = FxAssemblyNames.Length;
            while (index < length)
            {
                string name = FxAssemblyNames[index];
                string str2 = FxAssemblyVersion2[index];
                string str3 = FxAssemblyToken[index];
                AssemblyReference reference = new AssemblyReference(name + ", Version=" + str2 + ", Culture=neutral, PublicKeyToken=" + str3) {
                    Location = platformAssembliesLocation + @"\" + name + ".dll"
                };
                hashtable[Identifier.For(name).UniqueIdKey] = reference;
                index++;
            }
            assemblyReferenceFor = hashtable;
        }

        public static void SetToV2Beta1()
        {
            SetToV2Beta1(PlatformAssembliesLocation);
        }

        public static void SetToV2Beta1(string platformAssembliesLocation)
        {
            TargetVersion = new Version(2, 0, 0xe10);
            GenericTypeNamesMangleChar = '!';
            string path = null;
            if ((platformAssembliesLocation == null) || (platformAssembliesLocation.Length == 0))
            {
                DirectoryInfo parent = new FileInfo(new Uri(typeof(object).Assembly.Location).LocalPath).Directory.Parent;
                path = parent.FullName;
                if (path != null)
                {
                    path = path.ToUpper(CultureInfo.InvariantCulture);
                }
                DateTime minValue = DateTime.MinValue;
                foreach (DirectoryInfo info2 in parent.GetDirectories("v2.0*"))
                {
                    if ((info2 != null) && (info2.CreationTime >= minValue))
                    {
                        FileInfo[] files = info2.GetFiles("mscorlib.dll");
                        if ((files != null) && (files.Length == 1))
                        {
                            platformAssembliesLocation = info2.FullName;
                            minValue = info2.CreationTime;
                        }
                    }
                }
            }
            else
            {
                PlatformAssembliesLocation = platformAssembliesLocation;
            }
            if (((path != null) && ((platformAssembliesLocation == null) || (platformAssembliesLocation.Length == 0))) && ((path.IndexOf("FRAMEWORK") > 0) && (path.IndexOf("FRAMEWORK64") < 0)))
            {
                path = path.Replace("FRAMEWORK", "FRAMEWORK64");
                if (System.IO.Directory.Exists(path))
                {
                    DirectoryInfo info3 = new DirectoryInfo(path);
                    DateTime creationTime = DateTime.MinValue;
                    foreach (DirectoryInfo info4 in info3.GetDirectories("v2.0*"))
                    {
                        if ((info4 != null) && (info4.CreationTime >= creationTime))
                        {
                            FileInfo[] infoArray4 = info4.GetFiles("mscorlib.dll");
                            if ((infoArray4 != null) && (infoArray4.Length == 1))
                            {
                                platformAssembliesLocation = info4.FullName;
                                creationTime = info4.CreationTime;
                            }
                        }
                    }
                }
            }
            PlatformAssembliesLocation = platformAssembliesLocation;
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable hashtable = new TrivialHashtable(0x2e);
            int index = 0;
            int length = FxAssemblyNames.Length;
            while (index < length)
            {
                string name = FxAssemblyNames[index];
                string str3 = FxAssemblyVersion2Build3600[index];
                string str4 = FxAssemblyToken[index];
                AssemblyReference reference = new AssemblyReference(name + ", Version=" + str3 + ", Culture=neutral, PublicKeyToken=" + str4) {
                    Location = platformAssembliesLocation + @"\" + name + ".dll"
                };
                hashtable[Identifier.For(name).UniqueIdKey] = reference;
                index++;
            }
            assemblyReferenceFor = hashtable;
        }

        public static void SetToV4()
        {
            SetToV4(PlatformAssembliesLocation);
        }

        public static void SetToV4(string platformAssembliesLocation)
        {
            TargetVersion = new Version(4, 0);
            TargetRuntimeVersion = "v4.0.30319";
            GenericTypeNamesMangleChar = '`';
            if ((platformAssembliesLocation == null) || (platformAssembliesLocation.Length == 0))
            {
                platformAssembliesLocation = PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), @"..\v4.0.30319");
            }
            else
            {
                PlatformAssembliesLocation = platformAssembliesLocation;
            }
            PlatformAssembliesLocation = platformAssembliesLocation;
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
            TrivialHashtable hashtable = new TrivialHashtable(0x2e);
            int index = 0;
            int length = FxAssemblyNames.Length;
            while (index < length)
            {
                string name = FxAssemblyNames[index];
                string str2 = FxAssemblyVersion4[index];
                string str3 = FxAssemblyToken[index];
                AssemblyReference reference = new AssemblyReference(name + ", Version=" + str2 + ", Culture=neutral, PublicKeyToken=" + str3) {
                    Location = platformAssembliesLocation + @"\" + name + ".dll"
                };
                hashtable[Identifier.For(name).UniqueIdKey] = reference;
                index++;
            }
            assemblyReferenceFor = hashtable;
        }

        public static void SetToV4_5()
        {
            SetToV4_5(PlatformAssembliesLocation);
        }

        public static void SetToV4_5(string platformAssembliesLocation)
        {
            TargetVersion = new Version(4, 0);
            TargetRuntimeVersion = "v4.0.30319";
            GenericTypeNamesMangleChar = '`';
            if ((platformAssembliesLocation == null) || (platformAssembliesLocation.Length == 0))
            {
                platformAssembliesLocation = PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), @"..\v4.0.30319");
            }
            else
            {
                PlatformAssembliesLocation = platformAssembliesLocation;
            }
            PlatformAssembliesLocation = platformAssembliesLocation;
            InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, "System.Runtime", "mscorlib");
            TrivialHashtable hashtable = new TrivialHashtable(0x2e);
            int index = 0;
            int length = FxAssemblyNames.Length;
            while (index < length)
            {
                string name = FxAssemblyNames[index];
                string str2 = FxAssemblyVersion4[index];
                string str3 = FxAssemblyToken[index];
                AssemblyReference reference = new AssemblyReference(name + ", Version=" + str2 + ", Culture=neutral, PublicKeyToken=" + str3) {
                    Location = platformAssembliesLocation + @"\" + name + ".dll"
                };
                hashtable[Identifier.For(name).UniqueIdKey] = reference;
                index++;
            }
            assemblyReferenceFor = hashtable;
        }

        private static void SetupAssemblyReferenceFor()
        {
            Version targetVersion = TargetVersion;
            if (targetVersion == null)
            {
                targetVersion = typeof(object).Assembly.GetName().Version;
            }
            SetTo(targetVersion);
        }

        public static TrivialHashtable AssemblyReferenceFor
        {
            get
            {
                if (assemblyReferenceFor == null)
                {
                    SetupAssemblyReferenceFor();
                }
                return assemblyReferenceFor;
            }
            set
            {
                assemblyReferenceFor = value;
            }
        }

        internal static bool AssemblyReferenceForInitialized =>
            (assemblyReferenceFor != null);

        public static int Build =>
            TargetVersion.Build;

        public static int LinkerMajorVersion
        {
            get
            {
                switch (TargetVersion.Major)
                {
                    case 1:
                        return 7;

                    case 2:
                        return 8;

                    case 4:
                        return 8;
                }
                return 6;
            }
        }

        public static int LinkerMinorVersion =>
            TargetVersion.Minor;

        public static int MajorVersion =>
            TargetVersion.Major;

        public static int MinorVersion =>
            TargetVersion.Minor;

        public static IDictionary StaticAssemblyCache =>
            Reader.StaticAssemblyCache;

        public static bool UseGenerics
        {
            get
            {
                if (useGenerics)
                {
                    return true;
                }
                Version targetVersion = TargetVersion;
                return (((targetVersion?.Major > 1) || (targetVersion.Minor > 2)) || ((targetVersion.Minor == 2) && (targetVersion.Build >= 0xce4)));
            }
            set
            {
                useGenerics = value;
            }
        }
    }
}

