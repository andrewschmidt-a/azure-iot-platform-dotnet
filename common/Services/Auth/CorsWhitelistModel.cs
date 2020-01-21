namespace Mmm.Platform.IoT.Common.Services.Auth
{
    public class CorsWhitelistModel
    {
        public string[] Origins { get; set; }

        public string[] Methods { get; set; }

        public string[] Headers { get; set; }
    }
}
