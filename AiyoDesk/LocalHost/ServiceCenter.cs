
using AiyoCoveX.Host.Services;

namespace AiyoDesk.LocalHost;

public class ServiceCenter
{
    public ServiceCenter()
    {
        hostedService = new HostedService();
    }

    public static HostedService hostedService { get; private set; } = default!;


}
