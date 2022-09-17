namespace System.Compiler
{
    using System;
    using System.IO;

    internal sealed class BetterPath
    {
        public static readonly char AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;
        public static readonly char DirectorySeparatorChar = Path.DirectorySeparatorChar;
        public static readonly char VolumeSeparatorChar = Path.VolumeSeparatorChar;

        public static string ChangeExtension(string path, string extension)
        {
            if (path == null)
            {
                return null;
            }
            string str = path;
            int length = path.Length;
            while (--length >= 0)
            {
                char ch = path[length];
                if (ch == '.')
                {
                    str = path.Substring(0, length);
                    break;
                }
                if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                {
                    break;
                }
            }
            if ((extension == null) || (path.Length == 0))
            {
                return str;
            }
            if ((extension.Length == 0) || (extension[0] != '.'))
            {
                str = str + ".";
            }
            return (str + extension);
        }

        public static string Combine(string path1, string path2)
        {
            if ((path1 == null) || (path1.Length == 0))
            {
                return path2;
            }
            if ((path2 == null) || (path2.Length == 0))
            {
                return path1;
            }
            char ch = path2[0];
            if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || ((path2.Length >= 2) && (path2[1] == VolumeSeparatorChar)))
            {
                return path2;
            }
            ch = path1[path1.Length - 1];
            if (((ch != DirectorySeparatorChar) && (ch != AltDirectorySeparatorChar)) && (ch != VolumeSeparatorChar))
            {
                return (path1 + DirectorySeparatorChar + path2);
            }
            return (path1 + path2);
        }

        public static string GetDirectoryName(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            while (--length >= 0)
            {
                char ch = path[length];
                if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                {
                    return path.Substring(0, length);
                }
            }
            return path;
        }

        public static string GetExtension(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            int startIndex = length;
            while (--startIndex >= 0)
            {
                char ch = path[startIndex];
                if (ch == '.')
                {
                    if (startIndex != (length - 1))
                    {
                        return path.Substring(startIndex, length - startIndex);
                    }
                    return string.Empty;
                }
                if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                {
                    break;
                }
            }
            return string.Empty;
        }

        public static string GetFileName(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            while (--length >= 0)
            {
                char ch = path[length];
                if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                {
                    return path.Substring(length + 1);
                }
            }
            return path;
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            path = GetFileName(path);
            if (path == null)
            {
                return null;
            }
            int length = path.LastIndexOf('.');
            if (length == -1)
            {
                return path;
            }
            return path.Substring(0, length);
        }

        public static char[] GetInvalidFileNameChars() => 
            Path.GetInvalidFileNameChars();

        public static char[] GetInvalidPathChars() => 
            Path.GetInvalidPathChars();

        public static string GetTempFileName() => 
            Path.GetTempFileName();

        public static bool HasExtension(string path)
        {
            if (path != null)
            {
                int length = path.Length;
                while (--length >= 0)
                {
                    char ch = path[length];
                    if (ch == '.')
                    {
                        return (length != (path.Length - 1));
                    }
                    if (((ch == DirectorySeparatorChar) || (ch == AltDirectorySeparatorChar)) || (ch == VolumeSeparatorChar))
                    {
                        break;
                    }
                }
            }
            return false;
        }
    }
}

