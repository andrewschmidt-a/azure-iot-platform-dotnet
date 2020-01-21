using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Concurrency
{
    public class Timer : ITimer, IDisposable
    {
        private readonly ILogger logger;
        private bool disposedValue = false;
        private System.Threading.Timer timer;
        private int frequency;

        public Timer(ILogger<Timer> logger)
        {
            this.logger = logger;
            this.frequency = 0;
        }

        public ITimer Setup(Action<object> action, object context, TimeSpan frequency)
        {
            return this.Setup(action, context, (int)frequency.TotalMilliseconds);
        }

        public ITimer Setup(Action<object> action, object context, int frequency)
        {
            this.frequency = frequency;
            this.timer = new System.Threading.Timer(
                new TimerCallback(action),
                context,
                Timeout.Infinite,
                this.frequency);
            return this;
        }

        public ITimer Start()
        {
            return this.StartIn(TimeSpan.Zero);
        }

        public ITimer StartIn(TimeSpan delay)
        {
            if (this.timer == null)
            {
                logger.LogError("The timer is not initialized");
                throw new TimerNotInitializedException();
            }

            this.timer.Change((int)delay.TotalMilliseconds, this.frequency);
            return this;
        }

        public void Stop()
        {
            this.timer?.Change(Timeout.Infinite, Timeout.Infinite);
            this.timer?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }
    }
}