using FileCreateWorkerService;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using FileCreateWorkerService.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
    {
       IConfiguration configuration = hostContext.Configuration;
        services.AddDbContext<AdventureWorks2019Context>(opt =>
        {
            opt.UseSqlServer(configuration.GetConnectionString("SqlConnection"));
        });
       services.Configure<RabbitMQSetting>(configuration.GetSection("RabbitMQ"));
       services.AddSingleton(sp => new ConnectionFactory()
        {
            HostName = sp.GetRequiredService<IOptions<RabbitMQSetting>>().Value.Host,
            Port = sp.GetRequiredService<IOptions<RabbitMQSetting>>().Value.Port,
            UserName = sp.GetRequiredService<IOptions<RabbitMQSetting>>().Value.UserName,
            Password = sp.GetRequiredService<IOptions<RabbitMQSetting>>().Value.Password,
            DispatchConsumersAsync = true,
        });
        services.AddSingleton<RabbitMQClientService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
