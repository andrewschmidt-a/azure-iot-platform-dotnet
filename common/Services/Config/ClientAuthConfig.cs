namespace Mmm.Platform.IoT.Common.Services.Config
{
    public partial class ClientAuthConfig
    {
        public bool CorsEnabled => !string.IsNullOrEmpty(CorsWhitelist?.Trim());
    }
}