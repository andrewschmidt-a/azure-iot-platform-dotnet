using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers
{
    public static class AppInsightsExceptionHelper
    {
        private static TelemetryConfiguration configuration;
        private static TelemetryClient client;

        public static void Initialize(string instrumentationKey)
        {
            configuration = new TelemetryConfiguration(instrumentationKey);
            var observer = new ApplicationInsightsKubernetesDiagnosticObserver(DiagnosticLogLevel.Trace);
            ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);

            configuration.AddApplicationInsightsKubernetesEnricher(applyOptions: null);
            client = new TelemetryClient();
            client.InstrumentationKey = instrumentationKey;
        }

        //prevent self referencing looping
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static void LogException(Exception exception, Dictionary<string, string> traceDetails)
        {
            try
            {
                //Initialize();
                ExceptionTelemetry telemetry = new ExceptionTelemetry(exception);

                Type exceptionType = exception.GetType();
                if (exceptionType != null)
                {
                    foreach (PropertyInfo property in exceptionType.GetProperties())
                    {
                        telemetry.Properties[$"{exceptionType.Name}.{property.Name}"] = JsonConvert.SerializeObject(property.GetValue(exception), jsonSettings);
                    }

                    foreach (KeyValuePair<string, string> entry in traceDetails)
                    {
                        telemetry.Properties[entry.Key] = entry.Value;
                    }

                    telemetry.Message = exception.Message;
                    telemetry.Exception = exception;
                    client.TrackException(telemetry);
                    client.Flush();
                }
            }
            catch (Exception)
            {
            }
        }
        public static void LogTrace(string message, int severity, Dictionary<string, string> traceDetails)
        {
            try
            {
                //Initialize();
                client.TrackTrace(message, (SeverityLevel)severity, traceDetails);
                client.Flush();
            }
            catch (Exception)
            {
            }
        }

        public static void LogCustomEvent(string message, Dictionary<string, string> traceDetails)
        {
            try
            {
                //Initialize();
                client.TrackEvent(message, traceDetails);
                client.Flush();
            }
            catch (Exception)
            {
            }
        }
    }
}
