using RpcCore;
using SuperSocket.Client;
using System.Diagnostics;
using System.Net;

Console.WriteLine("请输入连接数");

var connectCount = 1000 * 1000;

var input = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(input))
    _ = int.TryParse(input, out connectCount);

Console.WriteLine($"开始执行");

var count = 1000 * 1000;

var taskList = new List<Task>();

for (int i = 0; i < connectCount; i++)
{
    var task = Task.Run(async () =>
    {
        IEasyClient<RpcPackageBase, RpcPackageBase> easyClient = new SuperSocket.IOCPEasyClient.IOCPTcpEasyClient<RpcPackageBase, RpcPackageBase>(new RpcPipeLineFilter { Decoder = new RpcPackageDecoder(new DefaultPacketFactoryPool()) }, new RpcPackageEncode());

        bool connecteed;

        try
        {
            connecteed = await easyClient.ConnectAsync(new DnsEndPoint("127.0.0.1", 4040, System.Net.Sockets.AddressFamily.InterNetwork), CancellationToken.None);
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync($"连接失败：{ex.Message}");
            return;
        }

        await Console.Out.WriteLineAsync($"连接状态：{connecteed}");

        for (int i = 0; i < count; i++)
        {
            await easyClient.SendAsync(new LoginPackage
            {
                Username = "sss",
                Password = "password",
            });

            var reply = await easyClient.ReceiveAsync();
        }
    });

    taskList.Add(task);
}

await Task.WhenAll(taskList.ToArray());
Console.WriteLine($"执行完成");
Console.ReadKey();

//IEasyClient<RpcPackageBase, RpcPackageBase> easyClient = new SuperSocket.IOCPEasyClient.IOCPTcpEasyClient<RpcPackageBase, RpcPackageBase>(new RpcPipeLineFilter { Decoder = new RpcPackageDecoder(new DefaultPacketFactoryPool()) }, new RpcPackageEncode());

//await easyClient.ConnectAsync(new DnsEndPoint("127.0.0.1", 4040, System.Net.Sockets.AddressFamily.InterNetwork), CancellationToken.None);

//var watch = new Stopwatch();
//watch.Start();

//Console.WriteLine("请输入发送次数，不输入默认为100w次按enter ");

//var count = 1000 * 1000;

//var input = Console.ReadLine();

//if (!string.IsNullOrWhiteSpace(input))
//    _ = int.TryParse(input, out count);

//Console.WriteLine($"开始执行");

//for (int i = 0; i < count; i++)
//{
//    await easyClient.SendAsync(new LoginPackage
//    {
//        Username = "sss",
//        Password = "password",
//    });

//    var reply = await easyClient.ReceiveAsync();
//}

//watch.Stop();
//Console.WriteLine($"执行完成{watch.ElapsedMilliseconds/1000}秒");

//Console.ReadKey();