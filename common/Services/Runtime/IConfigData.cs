using System.Collections.Generic;

namespace Mmm.Platform.IoT.Common.Services.Runtime
{
    public interface IConfigData
    {
        string GetString(string key, string defaultValue = "");
        bool GetBool(string key, bool defaultValue = false);
        int GetInt(string key, int defaultValue = 0);
        Dictionary<string, List<string>> GetUserPermissions();
        string GetSecretsFromKeyVault(string key);
    }
}