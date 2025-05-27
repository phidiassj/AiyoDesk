
namespace AiyoCoveX.Shared;

public interface IHostedService
{
    bool ServiceRunning { get; }

    Task RestartAsync(int port = 5000, CancellationToken cancellationToken = default);
    Task StartAsync(int port = 21999, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
