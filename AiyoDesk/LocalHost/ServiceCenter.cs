
using AiyoDesk.AppPackages;
using System.Threading.Tasks;

namespace AiyoDesk.LocalHost;

public class ServiceCenter
{
    public HostedHttpService hostedHttpService { get; set; } = new();

    public ServiceCenter()
    {

    }



}
