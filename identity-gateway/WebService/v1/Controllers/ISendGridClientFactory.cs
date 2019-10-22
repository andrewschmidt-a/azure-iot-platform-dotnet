using SendGrid;

namespace IdentityGateway.WebService.v1.Controllers
{
    public interface ISendGridClientFactory
    {
        ISendGridClient CreateSendGridClient();
    }
}