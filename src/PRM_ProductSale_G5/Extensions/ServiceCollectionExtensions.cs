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
        services.AddScoped<AuthService>();
        services.AddScoped<EmailService>();
        services.AddScoped<CloudinaryService>();
        services.AddScoped<UserService>();
        services.AddScoped<VnPayService>();
        // services.AddScoped<CartService>();
        // services.AddScoped<OrderService>();
        // services.AddScoped<NotificationService>();
        // services.AddScoped<ChatMessageService>();
        
        // Register repositories
        services.AddScoped<UnitOfWork>();
        services.AddScoped<AuthRepository>();
        services.AddScoped<UserRepository>();
        // services.AddScoped<CartRepository>();
        // services.AddScoped<CartItemRepository>();
        // services.AddScoped<ProductRepository>();
        // services.AddScoped<OrderRepository>();
        // services.AddScoped<NotificationRepository>();
        // services.AddScoped<ChatMessageRepository>();
    }

    private static string GetEnvironmentVariableOrThrow(string key)
    {
        return Environment.GetEnvironmentVariable(key) 
               ?? throw new ArgumentNullException(key, $"Environment variable '{key}' is not set.");
    }
}
