using System;

namespace Mmm.Platform.IoT.Common.Services.Exceptions
{
    public class ExternalDependencyException : Exception
    {
        public ExternalDependencyException()
            : base()
        {
        }

        public ExternalDependencyException(string message)
            : base(message)
        {
        }

        public ExternalDependencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ExternalDependencyException(Exception innerException)
            : base("An unexpected error happened while using an external dependency.", innerException)
        {
        }
    }
}