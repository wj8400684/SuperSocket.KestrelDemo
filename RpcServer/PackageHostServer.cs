using RpcServer.Abp.Commands;
using SuperSocket;

namespace RpcServer.Abp;

internal sealed class PackageHostServer : BackgroundService
{
    private readonly ISessionContainer _sessionContainer;

    internal static int PackageCount;

    public PackageHostServer(ISessionContainer sessionContainer)
    {
        _sessionContainer = sessionContainer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var lastCount = PackageCount;
            await Task.Delay(1000);
            var nowCount = PackageCount;

            await Console.Out.WriteLineAsync($"session count : {_sessionContainer.GetSessionCount()} | package : {nowCount-lastCount}/s");
        }
    }
}
