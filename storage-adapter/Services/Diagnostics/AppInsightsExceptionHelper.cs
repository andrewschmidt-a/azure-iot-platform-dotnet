/*using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics
{
    public static class AppInsightsExceptionHelper
    {
        //prevent self referencing looping
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static void LogException(Exception exception)
        {
            try
            {
                ExceptionTelemetry telemetry = new ExceptionTelemetry(exception);

                Type exceptionType = exception.GetType();
                if (exceptionType != null)
                {
                    foreach (PropertyInfo property in exceptionType.GetProperties())
                    {
                        telemetry.Properties[$"{exceptionType.Name}.{property.Name}"] = JsonConvert.SerializeObject(property.GetValue(exception), jsonSettings);
                    }

                    TelemetryClient client = new TelemetryClient();
                    client.TrackException(telemetry);
                    client.Flush();
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static void SendInformationLogs(string message, int severity, Dictionary<string, string> traceDetails)
        {
            try
            {
                TelemetryClient client = new TelemetryClient();
                client.TrackTrace(message, (SeverityLevel)severity, traceDetails);
                client.Flush();
            }
            catch (Exception ex)
            {
            }
        }
    }
}*/
