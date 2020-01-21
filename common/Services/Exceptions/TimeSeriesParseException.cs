// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Mmm.Platform.IoT.Common.Services.Exceptions
{
    public class TimeSeriesParseException : Exception
    {
        public TimeSeriesParseException()
            : base()
        {
        }

        public TimeSeriesParseException(string message)
            : base(message)
        {
        }

        public TimeSeriesParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
