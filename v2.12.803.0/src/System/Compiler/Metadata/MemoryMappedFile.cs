namespace System.Compiler.Metadata
{
    using System;
    using System.Compiler;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

    internal sealed class MemoryMappedFile : IDisposable, ISourceTextBuffer, ISourceText
    {
        private unsafe byte* buffer;
        private int length;

        public MemoryMappedFile(string fileName)
        {
            this.OpenMap(fileName);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern bool CloseHandle(IntPtr hObject);
        private unsafe void CloseMap()
        {
            if (this.buffer != null)
            {
                UnmapViewOfFile((void*) this.buffer);
                this.buffer = null;
            }
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, PageAccess flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);
        public void Dispose()
        {
            this.CloseMap();
            GC.SuppressFinalize(this);
        }

        ~MemoryMappedFile()
        {
            this.CloseMap();
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern unsafe void* MapViewOfFile(IntPtr hFileMappingObject, FileMapAccess dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);
        private unsafe void OpenMap(string filename)
        {
            int length;
            IntPtr ptr;
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > 0x7fffffffL)
                {
                    throw new FileLoadException(ExceptionStrings.FileTooBig, filename);
                }
                length = (int) stream.Length;
                ptr = CreateFileMapping(stream.SafeFileHandle.DangerousGetHandle(), IntPtr.Zero, PageAccess.PAGE_READONLY, 0, length, null);
                if (ptr == IntPtr.Zero)
                {
                    int num2 = Marshal.GetLastWin32Error();
                    throw new FileLoadException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.CreateFileMappingReturnedErrorCode, new object[] { num2.ToString() }), filename);
                }
            }
            this.buffer = (byte*) MapViewOfFile(ptr, FileMapAccess.FILE_MAP_READ, 0, 0, (IntPtr) length);
            CloseHandle(ptr);
            if (this.buffer == null)
            {
                int num3 = Marshal.GetLastWin32Error();
                throw new FileLoadException(string.Format(CultureInfo.CurrentCulture, ExceptionStrings.MapViewOfFileReturnedErrorCode, new object[] { num3.ToString() }), filename);
            }
            this.length = length;
        }

        void ISourceText.MakeCollectible()
        {
        }

        string ISourceText.Substring(int start, int length) => 
            null;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern unsafe bool UnmapViewOfFile(void* lpBaseAddress);

        public byte* Buffer =>
            this.buffer;

        public int Length =>
            this.length;

        char ISourceText.this[int index] =>
            ' ';

        private enum FileMapAccess
        {
            FILE_MAP_READ = 4
        }

        private enum PageAccess
        {
            PAGE_READONLY = 2
        }
    }
}

