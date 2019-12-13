// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Mmm.Platform.IoT.TenantManager.Services.Exceptions
{
    public class IotHubKeyException : Exception
    {
        public IotHubKeyException() : base()
        {
        }

        public IotHubKeyException(string message) : base(message)
        {
        }

        public IotHubKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
