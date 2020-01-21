using System;

namespace Mmm.Platform.IoT.Common.Services.Exceptions
{
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException()
            : base()
        {
        }

        public NotAuthorizedException(string message)
            : base(message)
        {
        }

        public NotAuthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}