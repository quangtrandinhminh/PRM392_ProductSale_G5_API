using Microsoft.Extensions.FileProviders;
using PRM_ProductSale_G5.Extensions;
using PRM_ProductSale_G5.Hubs;
using Serilog;
using Serilog.Events;
using Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Load environment variables
DotNetEnv.Env.Load();
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("APP_NAME")}");

// Cấu hình Serilog để loại bỏ logs từ Entity Framework
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database", LogEventLevel.Warning)
    .CreateLogger();

// Đăng ký ILogger vào DI container
builder.Services.AddSingleton(Log.Logger);

builder.Services.AddControllers();
// add serilog
builder.Host.UseSerilog();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// Thiết lập mức độ log tối thiểu cho Entity Framework Core
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Warning);

var app = builder.Build();

// Khởi tạo Firebase một lần duy nhất khi ứng dụng khởi động
FirebaseInitializer.Initialize();

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
