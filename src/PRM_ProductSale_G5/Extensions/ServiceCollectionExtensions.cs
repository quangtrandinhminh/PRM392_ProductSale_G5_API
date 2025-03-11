using Microsoft.Extensions.Options;
using Repositories.Base;
using Repositories.Models;
using Repositories.Repositories;
using Services.Config;
using Services.Mapper;
using Services.Services;

namespace PRM_ProductSale_G5.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register third-party services
        services.RegisterThirdPartyServices();

        // Register infrastructure services
        services.RegisterInfrastructureServices(configuration);

        // Register application-specific services
        RegisterApplicationServices(services);

        return services;
    }

    private static void RegisterThirdPartyServices(this IServiceCollection services)
    {
        // Configure settings
        services.Configure<SystemSettingModel>(options =>
        {
            options.Domain = Environment.GetEnvironmentVariable("SYSTEM_DOMAIN");
            options.SecretKey = GetEnvironmentVariableOrThrow("SYSTEM_SECRET_KEY");
            options.SecretCode = GetEnvironmentVariableOrThrow("SYSTEM_SECRET_CODE");
        });
        SystemSettingModel.Instance = services.BuildServiceProvider().GetService<IOptions<SystemSettingModel>>().Value;

        services.Configure<VnPaySetting>(options =>
        {
            options.TmnCode = GetEnvironmentVariableOrThrow("VNPAY_TMN_CODE");
            options.HashSecret = GetEnvironmentVariableOrThrow("VNPAY_HASH_SECRET");
            options.BaseUrl = GetEnvironmentVariableOrThrow("VNPAY_BASE_URL");
            options.Version = GetEnvironmentVariableOrThrow("VNPAY_VERSION");
            options.CurrCode = GetEnvironmentVariableOrThrow("VNPAY_CURR_CODE");
            options.Locale = GetEnvironmentVariableOrThrow("VNPAY_LOCALE");
        });
        VnPaySetting.Instance = services.BuildServiceProvider().GetService<IOptions<VnPaySetting>>().Value;

        services.Configure<SmtpSetting>(options =>
        {
            options.Host = GetEnvironmentVariableOrThrow("SMTP_HOST");
            options.Port = int.Parse(GetEnvironmentVariableOrThrow("SMTP_PORT"));
            options.EnableSsl = bool.Parse(GetEnvironmentVariableOrThrow("SMTP_ENABLE_SSL"));
            options.UsingCredential = bool.Parse(GetEnvironmentVariableOrThrow("SMTP_USING_CREDENTIAL"));
            options.Username = GetEnvironmentVariableOrThrow("SMTP_USERNAME");
            options.Password = GetEnvironmentVariableOrThrow("SMTP_PASSWORD");
        });

        services.Configure<MailSettingModel>(options =>
        {
            options.Smtp = services.BuildServiceProvider().GetService<IOptions<SmtpSetting>>().Value;
            options.FromAddress = GetEnvironmentVariableOrThrow("SMTP_FROM_ADDRESS");
            options.FromDisplayName = GetEnvironmentVariableOrThrow("SMTP_FROM_DISPLAY_NAME");
        });
        MailSettingModel.Instance = services.BuildServiceProvider().GetService<IOptions<MailSettingModel>>().Value;

        services.Configure<CloudinarySetting>(options =>
        {
            options.CloudinaryUrl = GetEnvironmentVariableOrThrow("CLOUDINARY_URL");
        });
        CloudinarySetting.Instance = services.BuildServiceProvider().GetService<IOptions<CloudinarySetting>>().Value;
    }

    private static void RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<AppDbContext>();

        // Add Cors
        const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
        services.AddCors(options =>
        {
            options.AddPolicy(myAllowSpecificOrigins, policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
            });
        });
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        // Register services
        services.AddScoped<MapperlyMapper>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVnPayService, VnPayService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IChatMessageService, ChatMessageService>();

        // Register repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
    }

    private static string GetEnvironmentVariableOrThrow(string key)
    {
        return Environment.GetEnvironmentVariable(key) 
               ?? throw new ArgumentNullException(key, $"Environment variable '{key}' is not set.");
    }
}
