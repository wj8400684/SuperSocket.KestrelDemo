using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket;
using System.Text;

namespace Server.Commands;

public sealed class MULT : IAsyncCommand<StringPackageInfo>
{
    async ValueTask IAsyncCommand<IAppSession, StringPackageInfo>.ExecuteAsync(IAppSession session, StringPackageInfo package)
    {
        //var result = package.Parameters
        //    .Select(p => int.Parse(p))
        //    .Aggregate((x, y) => x * y);

        var result = 10;

        var body = Encoding.UTF8.GetBytes($"MULT {result}\r\n");
        
        await session.SendAsync(body);
    }
}

