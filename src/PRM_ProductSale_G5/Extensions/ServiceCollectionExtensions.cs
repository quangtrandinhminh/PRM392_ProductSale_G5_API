using Microsoft.Extensions.Options;
using Repositories.Base;
using Repositories.Models;
using Repositories.Repositories;
using Services.Config;
using Services.Mapper;
using Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using PRM_ProductSale_G5.Hubs;
using Services.Enum;

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

        services.Configure<FirebaseSetting>(options =>
        {
            options.ProjectId = GetEnvironmentVariableOrThrow("FIREBASE_PROJECT_ID");
            options.ClientEmail = GetEnvironmentVariableOrThrow("FIREBASE_CLIENT_EMAIL");
            options.PrivateKey = GetEnvironmentVariableOrThrow("FIREBASE_PRIVATE_KEY");
        });
        FirebaseSetting.Instance = services.BuildServiceProvider().GetService<IOptions<FirebaseSetting>>().Value;
    }

    private static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<AppDbContext>();

        // Add Cors
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
            
            options.AddPolicy("SignalR", builder =>
            {
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true);
            });
        });

        // Đăng ký IUserIdProvider
        services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        
        // Đảm bảo đặt trước dòng AddSignalR
        services.AddSignalR();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("APP_NAME"),
                    ValidAudience = Environment.GetEnvironmentVariable("APP_NAME"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SYSTEM_SECRET_KEY")))
                };
            });

        // add required role
        services.AddAuthorization(options =>
        {
            options.AddPolicy(UserRoleEnum.Admin.ToString(), policy => policy.RequireRole(UserRoleEnum.Admin.ToString()));
            options.AddPolicy(UserRoleEnum.Customer.ToString(), policy => policy.RequireRole(UserRoleEnum.Customer.ToString()));
        });

        services.AddHttpContextAccessor();

        return services;
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
        services.AddScoped<IFirebaseService, FirebaseService>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
        services.AddScoped<IUserDeviceService, UserDeviceService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStoreLocationService, StoreLocationService>();

        // Register repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<IStoreLocationRepository, StoreLocationRepository>();
    }

    private static string GetEnvironmentVariableOrThrow(string key)
    {
        return Environment.GetEnvironmentVariable(key) 
               ?? throw new ArgumentNullException(key, $"Environment variable '{key}' is not set.");
    }
}
