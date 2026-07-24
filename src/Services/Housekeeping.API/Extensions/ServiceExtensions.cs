using Housekeeping.API.Consumers;
using MassTransit;
using MongoDB.Driver;

namespace Housekeeping.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddHousekeepingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. MongoDB Konfigürasyonu
        var mongoConnectionString = configuration.GetValue<string>("DatabaseSettings:ConnectionString") ?? "mongodb://localhost:27017";
        var mongoDatabaseName = configuration.GetValue<string>("DatabaseSettings:DatabaseName") ?? "HousekeepingDb";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoDatabaseName);
        });

        // 2. MassTransit & RabbitMQ Konfigürasyonu (Consumer Dinleyicisi)
        services.AddMassTransit(x =>
        {
            x.AddConsumer<AssignCleaningTaskConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // Saga'dan gelen AssignCleaningTaskCommand mesajlarını bu kuyruktan dinle
                cfg.ReceiveEndpoint("assign-cleaning-task-queue", e =>
                {
                    e.ConfigureConsumer<AssignCleaningTaskConsumer>(context);
                });
            });
        });

        return services;
    }
}
