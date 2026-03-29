using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var storageConnection = context.Configuration
            .GetSection("AzureStorage")["ConnectionString"]
            ?? throw new InvalidOperationException("Storage connection not configured.");

        var queueName = context.Configuration
            .GetSection("AzureStorage")["QueueName"]
            ?? "api-health-events";

        services.AddSingleton(new QueueClient(storageConnection, queueName));
        services.AddHostedService<ApiHealth.Worker.Worker>();
    })
    .Build();

await host.RunAsync();
