// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Mmm.Platform.IoT.Common.Services.Exceptions
{
    /// <summary>
    /// This exception is thrown when an external dependency returns any error
    /// </summary>
    public class ExternalDependencyException : Exception
    {
        public ExternalDependencyException() : base()
        {
        }

        public ExternalDependencyException(string message) : base(message)
        {
        }

        public ExternalDependencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ExternalDependencyException(Exception innerException)
            : base("An unexpected error happened while using an external dependency.", innerException)
        {
        }
    }
}
