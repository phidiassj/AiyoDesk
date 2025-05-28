
using AiyoCoveX.Host.Services;
using System.Threading.Tasks;

namespace AiyoDesk.LocalHost;

public class ServiceCenter
{
    private HostedService hostedService { get; set; } = new();
    public bool HttpServiceRunning { get; internal set; } = false;

    public ServiceCenter()
    {
        hostedService = new HostedService();
    }

    public async Task HttpServiceActivate()
    {
        if (!HttpServiceRunning)
        {
            await hostedService.RestartAsync();
            HttpServiceRunning = true;
        }
    }

    public async Task HttpServiceStop()
    {
        if (HttpServiceRunning)
        {
            await hostedService.StopAsync();
            HttpServiceRunning = false;
        }
    }


}
