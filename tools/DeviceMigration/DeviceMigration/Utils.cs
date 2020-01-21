using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceMigration
{
    public class Utils
    {
        public async Task<List<Twin>> GetDevices()
        {
            List<Twin> result = null;
            try
            {
                RegistryManager registryManager;
                string connectionString = ConfigurationManager.AppSettings["IoTHubConnectionString"];
                registryManager = RegistryManager.CreateFromConnectionString(connectionString);
                var query = registryManager.CreateQuery("SELECT * FROM devices");
                while (query.HasMoreResults)
                {
                    IEnumerable<Twin> twins = await query.GetNextAsTwinAsync().ConfigureAwait(false);
                    result = twins.ToList();
                }
            }
            catch (Exception )
            {
                throw;
            }
            return result;
        }

        public async Task<bool> SendTwinsToEH()
        {
            bool exceptionOccurred = false;
            try
            {

                EventHubClient eventHubClient;
                string EhConnectionString = ConfigurationManager.AppSettings["EHHubConnectionString"];
                string EhEntityPath = ConfigurationManager.AppSettings["EHName"];
                List<Task> list = new List<Task>();

                EventHubsConnectionStringBuilder connectionStringBuilder = new EventHubsConnectionStringBuilder(EhConnectionString)
                {
                    EntityPath = EhEntityPath
                };
                eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
                Console.WriteLine("Device Fetch Started");
                List<Twin> results = await GetDevices();
                Console.WriteLine($"Device Fetch Completed, count of devices: {results.Count}");
                Console.WriteLine("Push of device twins started");
                foreach (var twin in results)
                {
                    var document = JObject.Parse(twin.ToJson().ToString());
                    //Since the Functions work based on the tenant details, adding the tenant information to the body of the message
                    document["tenant"] = ConfigurationManager.AppSettings["tenant"];
                    byte[] payloadBytes = Encoding.UTF8.GetBytes(document.ToString());
                    var sendEvent = new EventData(payloadBytes);
                    await eventHubClient.SendAsync(sendEvent);
                    Thread.Sleep(100);
                }
                exceptionOccurred = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurrred : {ex.Message} StackTrace: {ex.StackTrace}  Inner Exception: {(string.IsNullOrEmpty(ex.StackTrace) ? string.Empty : ex.StackTrace)}");
                exceptionOccurred = false;
            }
            Console.WriteLine("Push of device twins completed");
            return exceptionOccurred;
        }
    }
}
