using System;

namespace Mmm.Platform.IoT.Common.Services.Exceptions
{
    public class NoAuthorizationException : Exception
    {
        public NoAuthorizationException()
            : base()
        {
        }

        public NoAuthorizationException(string message)
            : base(message)
        {
        }

        public NoAuthorizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}