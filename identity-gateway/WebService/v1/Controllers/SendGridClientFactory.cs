using IdentityGateway.Services.Runtime;
using SendGrid;

namespace IdentityGateway.WebService.v1.Controllers
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