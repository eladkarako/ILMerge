namespace System.Compiler
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal static class PlatformHelpers
    {
        internal static int StringCompareOrdinalIgnoreCase(string strA, string strB) => 
            string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);

        internal static int StringCompareOrdinalIgnoreCase(string strA, int indexA, string strB, int indexB, int length) => 
            string.Compare(strA, indexA, strB, indexB, length, StringComparison.OrdinalIgnoreCase);

        internal static bool TryParseInt32(string s, out int result) => 
            int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
    }
}

