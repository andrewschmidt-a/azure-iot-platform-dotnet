using Newtonsoft.Json.Serialization;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.Controllers
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}