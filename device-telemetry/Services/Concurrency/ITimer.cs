using System;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Concurrency
{
    public interface ITimer
    {
        ITimer Start();

        ITimer StartIn(TimeSpan delay);

        void Stop();

        ITimer Setup(Action<object> action, object context, TimeSpan frequency);

        ITimer Setup(Action<object> action, object context, int frequency);
    }
}
