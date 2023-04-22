﻿using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket;
using System.Text;

namespace Server.Commands;

public sealed class ADD : IAsyncCommand<StringPackageInfo>
{
    async ValueTask IAsyncCommand<IAppSession, StringPackageInfo>.ExecuteAsync(IAppSession session, StringPackageInfo package)
    {
        var result = package.Parameters.Select(p => int.Parse(p)).Sum();

        var body = Encoding.UTF8.GetBytes($"{nameof(ADD)} {result} 5\r\n");
        
        await session.SendAsync(body);
    }
}

