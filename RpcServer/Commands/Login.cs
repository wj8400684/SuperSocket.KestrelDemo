
namespace RpcServer.Abp.Commands;

[RpcCommand(CommandKey.Login)]
public sealed class Login : RpcAsyncRespIdentifierCommand<LoginPackage, LoginRespPackage>
{
    private readonly LoginRespPackage _loginRespPackage = new()
    {
        SuccessFul = true,
        Identifier = 10,
    };


    protected override ValueTask<LoginRespPackage> ExecuteAsync(RpcSession session, LoginPackage packet, CancellationToken cancellationToken)
    {
        //return ValueTask.FromResult(_loginRespPackage);
        return new ValueTask<LoginRespPackage>(new LoginRespPackage
        {
            SuccessFul = true,
            Identifier = packet.Identifier,
        });
    }
}
