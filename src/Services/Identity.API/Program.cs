using Identity.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Özel Extension Metodumuz ile tüm servisleri tek satırda ekliyoruz
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
