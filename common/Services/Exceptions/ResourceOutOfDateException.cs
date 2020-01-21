using System;

namespace Mmm.Platform.IoT.Common.Services.Exceptions
{
    public class ResourceOutOfDateException : Exception
    {
        public ResourceOutOfDateException()
            : base()
        {
        }

        public ResourceOutOfDateException(string message)
            : base(message)
        {
        }

        public ResourceOutOfDateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}