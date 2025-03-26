using System.Text.Json;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Serilog;
using Services.Config;

namespace Services.Services;

public interface IFirebaseService
{
    Task<bool> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string> data);
    Task<bool> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string> data);
}

public class FirebaseService : IFirebaseService
{
    private readonly FirebaseMessaging _messaging;
    private readonly ILogger _logger;
    
    public FirebaseService(ILogger logger)
    {
        _logger = logger;
        
        // Khởi tạo Firebase Admin SDK nếu chưa được khởi tạo
        if (FirebaseApp.DefaultInstance == null)
        {
            var credentials = GoogleCredential.FromJson(JsonSerializer.Serialize(new
            {
                type = "service_account",
                project_id = FirebaseSetting.Instance.ProjectId,
                private_key_id = "", // Không cần thiết
                private_key = FirebaseSetting.Instance.PrivateKey,
                client_email = FirebaseSetting.Instance.ClientEmail,
                client_id = "", // Không cần thiết
                auth_uri = "https://accounts.google.com/o/oauth2/auth",
                token_uri = "https://oauth2.googleapis.com/token",
                auth_provider_x509_cert_url = "https://www.googleapis.com/oauth2/v1/certs",
                client_x509_cert_url = $"https://www.googleapis.com/robot/v1/metadata/x509/{FirebaseSetting.Instance.ClientEmail.Replace("@", "%40")}"
            }));
            
            FirebaseApp.Create(new AppOptions
            {
                Credential = credentials
            });
        }
        
        _messaging = FirebaseMessaging.DefaultInstance;
    }
    
    public async Task<bool> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string> data)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };
            
            var response = await _messaging.SendAsync(message);
            _logger.Information("Thông báo đã được gửi thành công đến thiết bị {DeviceToken}, MessageId: {MessageId}", deviceToken, response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Lỗi khi gửi thông báo đến thiết bị {DeviceToken}", deviceToken);
            return false;
        }
    }
    
    public async Task<bool> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string> data)
    {
        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };
            
            var response = await _messaging.SendAsync(message);
            _logger.Information("Thông báo đã được gửi thành công đến chủ đề {Topic}, MessageId: {MessageId}", topic, response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Lỗi khi gửi thông báo đến chủ đề {Topic}", topic);
            return false;
        }
    }
}
