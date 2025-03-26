using Cloudflare_DDNS;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddSimpleConsole(options => { options.TimestampFormat = "yyyy-MM-dd HH:mm:ss "; });

builder.Services.AddHostedService<MainWorker>();
builder.Services.AddSingleton<MainService>();
builder.Services.AddHttpClient();


IHost host = builder.Build();
host.Run();