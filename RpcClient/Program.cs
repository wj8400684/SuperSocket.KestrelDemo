﻿using RpcCore;
using SuperSocket.Client;
using System.Diagnostics;
using System.Net;

IEasyClient<RpcPackageBase, RpcPackageBase> easyClient = new SuperSocket.IOCPEasyClient.IOCPTcpEasyClient<RpcPackageBase, RpcPackageBase>(new RpcPipeLineFilter(), new RpcPackageEncode());

await easyClient.ConnectAsync(new DnsEndPoint("127.0.0.1", 4040, System.Net.Sockets.AddressFamily.InterNetwork), CancellationToken.None);

var watch = new Stopwatch();
watch.Start();

Console.WriteLine("请输入发送次数，不输入默认为100w次按enter ");

var count = 1000 * 1000;

var input = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(input)) 
    _ = int.TryParse(input, out count);

Console.WriteLine($"开始执行");

for (int i = 0; i < count; i++)
{
    await easyClient.SendAsync(new LoginPackage
    {
        Username = "sss",
        Password = "password",
    });

    var reply = await easyClient.ReceiveAsync();
}

watch.Stop();
Console.WriteLine($"执行完成{watch.ElapsedMilliseconds/1000}秒");

Console.ReadKey();