using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket;
using System.Text;

namespace Server.Commands;

public sealed class ADD : IAsyncCommand<StringPackageInfo>
{
    async ValueTask IAsyncCommand<IAppSession, StringPackageInfo>.ExecuteAsync(IAppSession session, StringPackageInfo package)
    {
        var result = package.Parameters.Select(p => int.Parse(p)).Sum();

        await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
    }
}

