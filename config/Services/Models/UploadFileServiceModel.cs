using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    public class UploadFileServiceModel
    {
        [JsonProperty(PropertyName = "SoftwarePackageURL")]
        public string SoftwarePackageURL { get; set; }

        [JsonProperty(PropertyName = "CheckSum")]
        public CheckSumModel CheckSum { get; set; }
    }

    public class CheckSumModel
    {
        [JsonProperty(PropertyName = "MD5")]
        public string MD5 { get; set; }
        [JsonProperty(PropertyName = "SHA1")]
        public string SHA1 { get; set; }
    }
}
