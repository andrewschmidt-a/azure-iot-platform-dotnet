using Mmm.Platform.IoT.Common.Services.Config;
using SendGrid;

namespace Mmm.Platform.IoT.IdentityGateway.WebService
{
    public class SendGridClientFactory : ISendGridClientFactory
    {
        private readonly AppConfig config;

        public SendGridClientFactory(AppConfig config)
        {
            this.config = config;
        }

        public ISendGridClient CreateSendGridClient()
        {
            return new SendGridClient(config.IdentityGatewayService.SendGridApiKey);
        }
    }
}