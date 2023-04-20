using Microsoft.Extensions.Logging;
using SuperSocket.Command;

namespace SuperSocket.Console.Server;

/// <summary>
/// 具体响应包id的command
/// </summary>
/// <typeparam name="TPacket"></typeparam>
/// <typeparam name="TRespPacket"></typeparam>
public abstract class RpcAsyncRespIdentifierCommand<TPacket, TRespPacket> : IAsyncCommand<RpcSession, RpcPackageBase>
    where TPacket : RpcPackageWithIdentifier
    where TRespPacket : RpcRespPackageWithIdentifier, new()
{
    ValueTask IAsyncCommand<RpcSession, RpcPackageBase>.ExecuteAsync(RpcSession session, RpcPackageBase package) => SchedulerAsync(session, package, session.ConnectionToken);

    protected virtual async ValueTask SchedulerAsync(RpcSession session, RpcPackageBase package, CancellationToken cancellationToken)
    {
        TRespPacket respPacket;
        var request = (TPacket)package;

        try
        {
            respPacket = await ExecuteAsync(session, request, cancellationToken);
        }
        catch (Exception e)
        {
            respPacket = new TRespPacket
            {
                SuccessFul = false,
                Identifier = request.Identifier,
                ErrorMessage = "未知错误请稍后重试",
            };
            session.LogError(e, $"{session.RemoteAddress}-{package.Key} 抛出一个未知异常");
        }

        await session.SendPacketAsync(respPacket);
    }

    protected abstract ValueTask<TRespPacket> ExecuteAsync(RpcSession session, TPacket packet, CancellationToken cancellationToken);
}

