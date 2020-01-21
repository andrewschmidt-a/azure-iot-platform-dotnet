using System;

namespace Mmm.Platform.IoT.Common.Services.Wrappers
{
    public interface IExceptionChecker
    {
        bool IsConflictException(Exception exception);

        bool IsPreconditionFailedException(Exception exception);

        bool IsNotFoundException(Exception exception);
    }
}