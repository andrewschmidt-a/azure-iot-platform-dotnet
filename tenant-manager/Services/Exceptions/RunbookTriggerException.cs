// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.TenantManager.Services.Exceptions
{
    /// <summary>
    /// This exception is thrown when a runbook fails to succesfully execute from the TenantRunbookHelper class
    /// </summary>
    public class RunbookTriggerException : Exception
    {
        public RunbookTriggerException() : base()
        {
        }

        public RunbookTriggerException(string message) : base(message)
        {
        }

        public RunbookTriggerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
