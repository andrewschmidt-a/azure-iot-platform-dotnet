using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService.Helpers
{
    public class IotHubHelper
    {
        public static string CreateIotHub(string token, string tenantGuid, string subscriptionId, string rgName)
        {
            /* Creates an IoT Hub with the given configurations and returns the connection string. An empty
                string is returned on failure. */

            string tenantGuidPrefix = tenantGuid.Substring(0, 5);
            string iotHubName = "iothub-odin-" + tenantGuidPrefix;

            // Create an Http Client
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // IoT Hub description
            var description = new
            {   
                name = iotHubName,
                location = "East US",
                sku = new
                {
                    name = "S1",
                    tier = "Standard",
                    capacity = 1
                }
            };
            var json = JsonConvert.SerializeObject(description, Formatting.Indented);

            // Submit the REST API request to create the IoT Hub
            var content = new StringContent(JsonConvert.SerializeObject(description), Encoding.UTF8, "application/json");
            var requestUri = string.Format("https://management.azure.com/subscriptions/{0}/resourcegroups/{1}/providers/Microsoft.devices/IotHubs/{2}?api-version=2016-02-03", subscriptionId, rgName, iotHubName);
            var result = client.PutAsync(requestUri, content).Result;

            // Check the result status code
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed {0}", result.Content.ReadAsStringAsync().Result);
                // Return an empty string to indicate failure
                return "";
            }

            // Get the url to check the status of the IoT Hub deployment
            var asyncStatusUri = result.Headers.GetValues("Azure-AsyncOperation").First();

            // Wait for the deployment to complete (checking every 10 seconds)
            string body;
            do
            {
                Thread.Sleep(10000);
                HttpResponseMessage deploymentstatus = client.GetAsync(asyncStatusUri).Result;
                body = deploymentstatus.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Waiting on IoT Hub");
            } while (body == "{\"status\":\"Running\"}");

            // Get the connection string from the newly deployed IoT Hub
            var listKeysUri = string.Format("https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Devices/IotHubs/{2}/IoTHubKeys/listkeys?api-version=2016-02-03", subscriptionId, rgName, iotHubName);
            var keys = client.PostAsync(listKeysUri, null).Result;
            var keysString = keys.Content.ReadAsStringAsync().Result;
            var keyObj = JObject.Parse(keysString);
            string primaryIothubownerKey = (keyObj["value"][0]["primaryKey"]).ToString();
            string connectionString = string.Format("HostName={0}.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey={1}", iotHubName, primaryIothubownerKey);

            return connectionString;
        }
    }
}