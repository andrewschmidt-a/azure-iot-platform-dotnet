using System;

namespace Mmm.Platform.IoT.Common.Services.Diagnostics
{
    public interface ILogger
    {
        // The following 4 methods allow to log a message, capturing the context
        // (i.e. the method where the log message is generated)

        void Debug(string message, Action context);
        void Info(string message, Action context);
        void Warn(string message, Action context);
        void Error(string message, Action context);

        // The following 4 methods allow to log a message and some data,
        // capturing the context (i.e. the method where the log message is generated)

        void Debug(string message, Func<object> context);
        void Info(string message, Func<object> context);
        void Warn(string message, Func<object> context);
        void Error(string message, Func<object> context);
    }
}