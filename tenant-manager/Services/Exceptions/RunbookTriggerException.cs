using System;

namespace Mmm.Platform.IoT.TenantManager.Services.Exceptions
{
    public class RunbookTriggerException : Exception
    {
        public RunbookTriggerException()
            : base()
        {
        }

        public RunbookTriggerException(string message)
            : base(message)
        {
        }

        public RunbookTriggerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}