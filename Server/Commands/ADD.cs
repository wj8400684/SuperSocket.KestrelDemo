using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket;
using System.Text;

namespace Server.Commands;

public sealed class ADD : IAsyncCommand<StringPackageInfo>
{
    async ValueTask IAsyncCommand<IAppSession, StringPackageInfo>.ExecuteAsync(IAppSession session, StringPackageInfo package)
    {
        var result = 10;//package.Parameters.Select(p => int.Parse(p)).Sum();

        var body = Encoding.UTF8.GetBytes($"ADD {result}\r\n");
        
        await session.SendAsync(body);
    }
}

