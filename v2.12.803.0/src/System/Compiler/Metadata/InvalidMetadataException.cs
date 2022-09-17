namespace System.Compiler.Metadata
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class InvalidMetadataException : Exception
    {
        public InvalidMetadataException()
        {
        }

        public InvalidMetadataException(string message) : base(message)
        {
        }

        private InvalidMetadataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidMetadataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

