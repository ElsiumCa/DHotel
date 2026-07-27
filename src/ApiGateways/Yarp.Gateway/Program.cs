using MassTransit;
using Yarp.Gateway.Consumers;
using Yarp.Gateway.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy", policy => {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSignalR();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RoomEventsConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ReceiveEndpoint("gateway-signalr-events-queue", e =>
        {
            e.ConfigureConsumer<RoomEventsConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");
app.MapHub<RoomHub>("/hubs/room");
app.MapReverseProxy();

app.Run();
