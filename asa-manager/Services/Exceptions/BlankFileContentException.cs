using System;

namespace Mmm.Platform.IoT.AsaManager.Services.Exceptions
{
    public class BlankFileContentException : Exception
    {
        public BlankFileContentException() : base() { }
        public BlankFileContentException(string message) : base(message) { }
        public BlankFileContentException(string message, Exception inner) : base(message, inner) { }
    }
}