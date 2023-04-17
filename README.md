# SuperSocket.Kestrel

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    var serverOptions = context.Configuration.GetSection("ServerOptions").Get<ServerOptions>()!;

    foreach (var listeners in serverOptions.Listeners)
    {
        options.Listen(listeners.GetListenEndPoint(), listenOptions =>
        {
            listenOptions.UseConnectionHandler<KestrelChannelCreator>();
        });
    }
});

builder.Host.AsSuperSocketHostBuilder<StringPackageInfo, CommandLinePipelineFilter>()
    .UseKestrelChannelCreatorFactory()
    .AsMinimalApiHostBuilder()
    .ConfigureHostBuilder();

var app = builder.Build();

await app.RunAsync();
