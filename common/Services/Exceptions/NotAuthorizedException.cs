// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Mmm.Platform.IoT.Common.Services.Exceptions
{
    /// <summary>
    /// This exception is thrown when the user or the application
    /// is not authorized to perform the action.
    /// </summary>
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException() : base()
        {
        }

        public NotAuthorizedException(string message) : base(message)
        {
        }

        public NotAuthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
