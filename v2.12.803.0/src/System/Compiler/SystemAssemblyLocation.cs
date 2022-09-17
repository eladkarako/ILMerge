namespace System.Compiler
{
    using System;
    using System.Collections;

    internal sealed class SystemAssemblyLocation
    {
        private static string location;
        public static AssemblyNode ParsedAssembly;
        public static IDictionary SystemAssemblyCache = null;

        public static string Location
        {
            get => 
                location;
            set
            {
                location = value;
            }
        }
    }
}

