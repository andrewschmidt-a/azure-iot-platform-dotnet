using System.Collections.Generic;

namespace Mmm.Platform.IoT.Common.Services.Runtime
{
    public interface IConfigData
    {
        string AppConfigurationConnectionString { get; }
        string AadAppId { get; }
        string AadAppSecret { get; }
        string AadTenantId { get; }
        string KeyVaultName { get; }
        Dictionary<string, List<string>> UserPermissions { get; }

        string GetString(string key, string defaultValue = "");
        bool GetBool(string key, bool defaultValue = false);
        int GetInt(string key, int defaultValue = 0);
    }
}