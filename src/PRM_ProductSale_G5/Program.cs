using DotNetEnv;
using PRM_ProductSale_G5.Extensions;
using PRM_ProductSale_G5.Hubs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Load environment variables
DotNetEnv.Env.Load();
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("APP_NAME")}");

builder.Services.AddControllers();
// add serilog
builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwaggerDocumentation();

app.UseCors("_myAllowSpecificOrigins");
app.UseCors("SignalR");

app.UseApplicationMiddleware();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
