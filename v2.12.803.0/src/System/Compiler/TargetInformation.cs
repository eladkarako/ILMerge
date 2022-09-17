namespace System.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TargetInformation
    {
        public string Company;
        public string Configuration;
        public string Copyright;
        public string Culture;
        public string Description;
        public string Product;
        public string ProductVersion;
        public string Title;
        public string Trademark;
        public string Version;
    }
}

