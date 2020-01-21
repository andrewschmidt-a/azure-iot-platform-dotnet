// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Mmm.Platform.IoT.Common.Services.Exceptions
{
    public class ResourceNotSupportedException : Exception
    {
        public ResourceNotSupportedException()
        {
        }

        public ResourceNotSupportedException(string message)
            : base(message)
        {
        }

        public ResourceNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
