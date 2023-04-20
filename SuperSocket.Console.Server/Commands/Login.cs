namespace SuperSocket.Console.Server.Commands;

[RpcCommand(CommandKey.Login)]
public sealed class Login : RpcAsyncRespIdentifierCommand<LoginPackage, LoginRespPackage>
{
    protected override ValueTask<LoginRespPackage> ExecuteAsync(RpcSession session, LoginPackage packet, CancellationToken cancellationToken)
    {
        return new ValueTask<LoginRespPackage>(new LoginRespPackage
        {
            SuccessFul = true,
            Identifier = packet.Identifier,
        });
    }
}
