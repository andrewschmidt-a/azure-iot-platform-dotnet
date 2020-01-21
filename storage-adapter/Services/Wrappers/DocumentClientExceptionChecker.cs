using System;
using System.Net;
using Microsoft.Azure.Documents;
using Mmm.Platform.IoT.Common.Services.Wrappers;

namespace Mmm.Platform.IoT.StorageAdapter.Services.Wrappers
{
    public class DocumentClientExceptionChecker : IExceptionChecker
    {
        public bool IsConflictException(Exception exception)
        {
            var ex = exception as DocumentClientException;
            return ex != null && ex.StatusCode == HttpStatusCode.Conflict;
        }

        public bool IsPreconditionFailedException(Exception exception)
        {
            var ex = exception as DocumentClientException;
            return ex != null && ex.StatusCode == HttpStatusCode.PreconditionFailed;
        }

        public bool IsNotFoundException(Exception exception)
        {
            var ex = exception as DocumentClientException;
            return ex != null && ex.StatusCode == HttpStatusCode.NotFound;
        }
    }
}