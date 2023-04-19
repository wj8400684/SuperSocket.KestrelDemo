using Client;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using System.Diagnostics;
using System.Net;
using System.Text;

var easyClient = new EasyClient<StringPackageInfo>(new CommandLinePipelineFilter { Decoder = new DefaultStringPackageDecoder() }).AsClient();

await easyClient.ConnectAsync(new DnsEndPoint("127.0.0.1", 4040, System.Net.Sockets.AddressFamily.InterNetwork), CancellationToken.None);

var watch = new Stopwatch();
watch.Start();

Console.WriteLine("请输入发送次数，不输入默认为10w次按enter ");

var count = 1000 * 1000;

var input = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(input))
    _ = int.TryParse(input, out count);

Console.WriteLine($"开始执行");

var buffer = Encoding.UTF8.GetBytes("ADD 1 2 3\r\n");

for (int i = 0; i < count; i++)
{
    await easyClient.SendAsync(buffer);

    var reply = await easyClient.ReceiveAsync();
}

watch.Stop();
Console.WriteLine($"执行完成{watch.ElapsedMilliseconds/1000}秒");

Console.ReadKey();