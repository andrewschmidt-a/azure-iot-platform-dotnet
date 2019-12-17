using SendGrid;

namespace Mmm.Platform.IoT.IdentityGateway.WebService
{
    public interface ISendGridClientFactory
    {
        ISendGridClient CreateSendGridClient();
    }
}