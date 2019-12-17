// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Concurrency
{
    public class TimerNotInitializedException : Exception
    {
        public TimerNotInitializedException()
            : base("Timer object not initialized. Call 'Setup()' first.")
        {
        }
    }
}
