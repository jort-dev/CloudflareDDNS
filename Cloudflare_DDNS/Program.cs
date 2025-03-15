using Cloudflare_DDNS;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MainWorker>();
builder.Services.AddSingleton<MainService>();
builder.Services.AddHttpClient();


IHost host = builder.Build();
host.Run();