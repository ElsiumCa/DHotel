using MassTransit;
using Microsoft.EntityFrameworkCore;
using RoomLifecycle.Saga;
using RoomLifecycle.Saga.Data;
using RoomLifecycle.Saga.StateMachine;
using RoomLifecycle.Saga.StateMaps;

var builder = Host.CreateApplicationBuilder(args);

var connectionstrings = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<RoomStateDbContext>(
    options => options.UseMySql(connectionstrings, ServerVersion.AutoDetect(connectionstrings)));

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<RoomLifecycleStateMachine, RoomStateData>()
    .EntityFrameworkRepository(r =>
    {
        r.ExistingDbContext<RoomStateDbContext>();
        r.UseMySql();

    });
    x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("localhost","/", h =>
             {
                h.Username("guest");
                h.Password("guest");
            });
            cfg.ConfigureEndpoints(context);


        });


});
var host = builder.Build();
host.Run();
