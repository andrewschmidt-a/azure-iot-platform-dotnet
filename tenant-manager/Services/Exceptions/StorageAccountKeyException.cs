using System;

namespace Mmm.Platform.IoT.TenantManager.Services.Exceptions
{
    public class StorageAccountKeyException : Exception
    {
        public StorageAccountKeyException()
            : base()
        {
        }

        public StorageAccountKeyException(string message)
            : base(message)
        {
        }

        public StorageAccountKeyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}