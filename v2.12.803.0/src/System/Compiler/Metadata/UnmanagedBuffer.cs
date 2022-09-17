namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class UnmanagedBuffer : IDisposable
    {
        internal IntPtr Pointer;

        internal UnmanagedBuffer(int length)
        {
            this.Pointer = Marshal.AllocHGlobal(length);
        }

        public void Dispose()
        {
            if (this.Pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.Pointer);
            }
            this.Pointer = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        ~UnmanagedBuffer()
        {
            this.Dispose();
        }
    }
}

