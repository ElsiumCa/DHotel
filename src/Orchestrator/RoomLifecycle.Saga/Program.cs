using EventBus.Messages.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RoomLifecycle.Saga.Data;
using RoomLifecycle.Saga.StateMachine;
using RoomLifecycle.Saga.StateMaps;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy", policy => {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

EndpointConvention.Map<CreateMaintenanceTicketCommand>(new Uri("queue:create-maintenance-ticket-queue"));
EndpointConvention.Map<AssignCleaningTaskCommand>(new Uri("queue:assign-cleaning-task-queue"));

var app = builder.Build();

app.UseCors("CorsPolicy");

// MariaDB SagaDb veritabanındaki tüm canlı oda durumlarını döndüren Minimal API
app.MapGet("/api/saga/rooms", async (RoomStateDbContext db) =>
{
    var states = await db.Set<RoomStateData>().ToListAsync();
    return Results.Ok(states);
});

app.Run();
