

using Maintenance.API.Consumers;
using Maintenance.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.API.Extensions;

public static class ServiceExtension
{
   public static IServiceCollection AddMaintenanceServices(this IServiceCollection services, IConfiguration configuration){

        //mariadb bağlantısı
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<MaintenanceDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        );

        //MASSTRANSİT
        services.AddMassTransit(x =>
        { x.AddConsumer<CreateMaintenanceTicketConsumer>();
        x.AddEntityFrameworkOutbox<MaintenanceDbContext>(o =>
        {
            o.UseMySql();
            o.UseBusOutbox();
        });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                cfg.ReceiveEndpoint("create-maintenance-ticket-queue", e => {

                    e.ConfigureConsumer<CreateMaintenanceTicketConsumer>(context);
                });

            });

        }
        );

        return services;
    }
}