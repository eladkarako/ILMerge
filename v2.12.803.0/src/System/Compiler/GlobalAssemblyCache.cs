namespace System.Compiler
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class GlobalAssemblyCache
    {
        private static bool FusionLoaded;
        private static readonly object Lock = new object();
        public static bool probeGAC = true;

        private GlobalAssemblyCache()
        {
        }

        public static bool Contains(Uri codeBaseUri)
        {
            if (codeBaseUri == null)
            {
                return false;
            }
            lock (Lock)
            {
                IAssemblyEnum enum2;
                IAssemblyName name;
                AssemblyName name2;
                IApplicationContext context;
                if (!FusionLoaded)
                {
                    FusionLoaded = true;
                    LoadLibrary(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "fusion.dll"));
                }
                if ((CreateAssemblyEnum(out enum2, null, null, 2, 0) >= 0) && (enum2 != null))
                {
                    goto Label_00D3;
                }
                return false;
            Label_007A:
                name2 = new AssemblyName(name);
                string scheme = codeBaseUri.Scheme;
                if ((scheme != null) && name2.CodeBase.StartsWith(scheme))
                {
                    try
                    {
                        Uri uri = new Uri(name2.CodeBase);
                        if (codeBaseUri.Equals(uri))
                        {
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            Label_00D3:
                if (enum2.GetNextAssembly(out context, out name, 0) == 0)
                {
                    goto Label_007A;
                }
                return false;
            }
        }

        [DllImport("fusion.dll", CharSet=CharSet.Auto)]
        private static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum, IApplicationContext pAppCtx, IAssemblyName pName, uint dwFlags, int pvReserved);
        public static string GetLocation(AssemblyReference assemblyReference)
        {
            if (!probeGAC)
            {
                return null;
            }
            if (assemblyReference == null)
            {
                return null;
            }
            lock (Lock)
            {
                IAssemblyEnum enum2;
                IAssemblyName name;
                AssemblyName name2;
                IApplicationContext context;
                if (!FusionLoaded)
                {
                    FusionLoaded = true;
                    LoadLibrary(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "fusion.dll"));
                }
                CreateAssemblyEnum(out enum2, null, null, 2, 0);
                if (enum2 != null)
                {
                    goto Label_00E5;
                }
                return null;
            Label_0077:
                name2 = new AssemblyName(name);
                if (assemblyReference.Matches(name2.Name, name2.Version, name2.Culture, name2.PublicKeyToken))
                {
                    string codeBase = name2.CodeBase;
                    if ((codeBase != null) && codeBase.StartsWith("file:///"))
                    {
                        return codeBase.Substring(8);
                    }
                    return name2.GetLocation();
                }
            Label_00E5:
                if (enum2.GetNextAssembly(out context, out name, 0) == 0)
                {
                    goto Label_0077;
                }
                return null;
            }
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        private class ASM_CACHE
        {
            public const uint DOWNLOAD = 4;
            public const uint GAC = 2;
            public const uint ZAP = 1;

            private ASM_CACHE()
            {
            }
        }
    }
}

