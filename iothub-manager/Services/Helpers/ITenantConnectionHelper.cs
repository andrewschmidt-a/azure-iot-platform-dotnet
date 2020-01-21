using Microsoft.Azure.Devices;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Helpers
{
    public interface ITenantConnectionHelper
    {
        string GetIotHubName();

        RegistryManager GetRegistry();

        string GetIotHubConnectionString();

        JobClient GetJobClient();
    }
}
