using System;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.Wrappers;

namespace Mmm.Platform.IoT.Common.TestHelpers
{
    public class MockExceptionChecker : IExceptionChecker
    {
        public bool IsConflictException(Exception exception)
        {
            return exception is ConflictingResourceException;
        }

        public bool IsPreconditionFailedException(Exception exception)
        {
            return exception is ConflictingResourceException;
        }

        public bool IsNotFoundException(Exception exception)
        {
            return exception is ResourceNotFoundException;
        }
    }
}