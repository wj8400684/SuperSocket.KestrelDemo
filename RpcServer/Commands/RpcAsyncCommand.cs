using SuperSocket;
using SuperSocket.Command;

namespace RpcServer.Abp.Commands;

public abstract class RpcAsyncCommand<TPacket> : IAsyncCommand<RpcSession, RpcPackageBase>
    where TPacket : RpcPackageBase
{
    ValueTask IAsyncCommand<RpcSession, RpcPackageBase>.ExecuteAsync(RpcSession session, RpcPackageBase package) => SchedulerAsync(session, package, session.ConnectionToken);

    protected virtual async ValueTask SchedulerAsync(RpcSession session, RpcPackageBase package, CancellationToken cancellationToken)
    {
        var request = (TPacket)package;

        try
        {
            await ExecuteAsync(session, request, cancellationToken);
        }
        catch (Exception e)
        {
            session.LogError(e, $"{session.RemoteAddress}-{package.Key} 抛出一个未知异常");
        }
    }

    protected abstract ValueTask ExecuteAsync(RpcSession session, TPacket packet, CancellationToken cancellationToken);
}


public abstract class RpcAsyncRespCommand<TPacket, TRespPacket> : IAsyncCommand<RpcSession, RpcPackageBase>
    where TPacket : RpcPackageBase
    where TRespPacket : RpcRespPackage, new()
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

