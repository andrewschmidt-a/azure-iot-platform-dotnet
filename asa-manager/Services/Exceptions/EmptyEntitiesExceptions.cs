using System;

namespace Mmm.Platform.IoT.AsaManager.Services.Exceptions
{
    public class EmptyEntitesException : Exception
    {
        public EmptyEntitesException() : base() { }
        public EmptyEntitesException(string message) : base(message) { }
        public EmptyEntitesException(string message, Exception inner) : base(message, inner) { }
    }
}