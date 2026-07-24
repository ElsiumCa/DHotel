using FrontDesk.API.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace FrontDesk.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddFrontDeskServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. DB Context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<FrontDeskDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        //2.MassTransit
        services.AddMassTransit(mt => 
        {
            mt.AddEntityFrameworkOutbox<FrontDeskDbContext>(
                o => 
                {
                    o.UseMySql();
                    o.UseBusOutbox();
                }
            );
            mt.UsingRabbitMq((context, cfg) => 
            {
                cfg.Host("localhost", "/", h => 
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                cfg.ConfigureEndpoints(context);
            });
        });


        return services;
    }
}