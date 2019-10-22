using SendGrid;

namespace WebService
{
    public interface ISendGridClientFactory
    {
        ISendGridClient CreateSendGridClient();
    }
}