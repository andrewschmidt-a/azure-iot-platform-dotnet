using Mmm.Platform.IoT.IdentityGateway.Services.Runtime;
using SendGrid;

namespace Mmm.Platform.IoT.IdentityGateway.WebService
{
    public class SendGridClientFactory : ISendGridClientFactory
    {
        private readonly IServicesConfig _servicesConfig;

        public SendGridClientFactory(IServicesConfig servicesConfig)
        {
            this._servicesConfig = servicesConfig;
        }

        public ISendGridClient CreateSendGridClient()
        {
            return new SendGridClient(_servicesConfig.SendGridAPIKey);
        }
    }
}