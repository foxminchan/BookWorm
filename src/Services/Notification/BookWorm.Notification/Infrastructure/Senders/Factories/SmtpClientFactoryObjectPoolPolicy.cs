namespace BookWorm.Notification.Infrastructure.Senders.Factories;

public sealed class SmtpClientFactoryObjectPoolPolicy(ISmtpClientFactory factory)
    : IPooledObjectPolicy<SmtpClient>
{
    public SmtpClient Create()
    {
        return factory.CreateClient();
    }

    public bool Return(SmtpClient obj)
    {
        try
        {
            if (obj.IsConnected)
            {
                obj.Disconnect(true);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
