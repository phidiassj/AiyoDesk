namespace AiyoCoveX.Host.Services;

public class HostedService : IHostedService
{
    public bool ServiceRunning { get; internal set; }
    public static Func<Dictionary<string, Dictionary<string, string>>>? RequestMenuItems;

    private IHost? _host;

    public HostedService()
    {
    }

    public async Task StartAsync(int port = 16888, CancellationToken cancellationToken = default)
    {
        if (_host != null)
            return;

        _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseKestrel()
                                  .UseUrls($"http://*:{port}/")
                                  .UseStartup<Startup>();
                    })
                    .Build();
        
        await _host.StartAsync(cancellationToken);
        ServiceRunning = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_host != null)
        {
            await _host.StopAsync(cancellationToken);
            _host.Dispose();
            _host = null;
        }
        ServiceRunning = false;
    }

    public async Task RestartAsync(int port = 16888, CancellationToken cancellationToken = default)
    {
        await StopAsync(cancellationToken);
        ServiceRunning = false;
        await StartAsync(port, cancellationToken);
        ServiceRunning = true;
    }

}

public interface IHostedService
{
    bool ServiceRunning { get; }

    Task RestartAsync(int port = 5000, CancellationToken cancellationToken = default);
    Task StartAsync(int port = 21999, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

