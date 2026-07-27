using Maintenance.API.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMaintenanceServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();


app.Run();


