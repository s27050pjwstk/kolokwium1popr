using kolokwium1popr.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();