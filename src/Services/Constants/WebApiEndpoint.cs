namespace Services.Constants
{
    public static class WebApiEndpoint
    {
        public const string AreaName = "api";

        public static class Authentication
        {
            private const string BaseEndpoint = "~/" + AreaName + "/auth";
            public const string Hello = BaseEndpoint + "/hello";
            public const string Register = BaseEndpoint + "/register";
            public const string Login = BaseEndpoint + "/authentication";
            public const string GetAdminId = BaseEndpoint + "/admin-id";
        }

        public static class User
        {
            private const string BaseEndpoint = "~/" + AreaName + "/user";
            public const string GetUsers = BaseEndpoint;
            public const string GetUser = BaseEndpoint + "/{id}";
            public const string CreateUser = BaseEndpoint;
            public const string UpdateUser = BaseEndpoint;
            public const string DeleteUser = BaseEndpoint + "/{id}";
        }

        public static class Product
        {
            private const string BaseEndpoint = "~/" + AreaName + "/product";
            public const string GetProducts = BaseEndpoint;
            public const string GetProduct = BaseEndpoint + "/{id}";
            public const string CreateProduct = BaseEndpoint;
            public const string UpdateProduct = BaseEndpoint;
            public const string DeleteProduct = BaseEndpoint + "/{id}";
            public const string Search = BaseEndpoint + "/search";
        }

        public static class Category
        {
            private const string BaseEndpoint = "~/" + AreaName + "/category";
            public const string GetCategories = BaseEndpoint;
            public const string GetCategory = BaseEndpoint + "/{id}";
            public const string CreateCategory = BaseEndpoint;
            public const string UpdateCategory = BaseEndpoint;
            public const string DeleteCategory = BaseEndpoint + "/{id}";
        }

        public static class Order
        {
            private const string BaseEndpoint = "~/" + AreaName + "/orders";
            public const string GetOrders = BaseEndpoint;
            public const string GetOrder = BaseEndpoint + "/{orderId}";
            public const string CreateOrder = BaseEndpoint;
            public const string DeleteOrder = BaseEndpoint + "/{orderId}";
            public const string GetOrdersByUser = BaseEndpoint + "/user/status";
            public const string UpdateOrderStatus = BaseEndpoint + "/{orderId}/newStatus";
            public const string GetOrdersByStatus = BaseEndpoint + "/status";
            public const string CustomerChangeOrderStatus = BaseEndpoint + "customer/{orderId}";
            public const string AdminChangeOrderStatus = BaseEndpoint + "admin/{orderId}";
            public const string CustomerCancelOrder = BaseEndpoint + "customer/{orderId}";
        }

        public static class Cart
        {
            private const string BaseEndpoint = "~/" + AreaName + "/cart";
            public const string GetCarts = BaseEndpoint;
            public const string AddToCart = BaseEndpoint + "/cartItem";
            public const string UpdateCart = BaseEndpoint + "/cartItem";
            public const string DeleteCart = BaseEndpoint + "/cartItem/{cartItemId}";
        }

        public static class ProductCategory
        {
            private const string BaseEndpoint = "~/" + AreaName + "/productCategory";
            public const string GetProductCategories = BaseEndpoint;
            public const string GetProductCategory = BaseEndpoint + "/{id}";
            public const string CreateProductCategory = BaseEndpoint;
            public const string UpdateProductCategory = BaseEndpoint;
            public const string DeleteProductCategory = BaseEndpoint + "/{id}";
        }

        public static class OrderItem
        {
            private const string BaseEndpoint = "~/" + AreaName + "/orderItem";
            public const string GetOrderItems = BaseEndpoint;
            public const string GetOrderItem = BaseEndpoint + "/{id}";
            public const string CreateOrderItem = BaseEndpoint;
            public const string UpdateOrderItem = BaseEndpoint;
            public const string DeleteOrderItem = BaseEndpoint + "/{id}";
        }

        public static class ChatMessage
        {
            private const string Prefix = "api/chat-messages";
            
            public const string GetChatMessages = Prefix;
            public const string GetChatMessagesBetweenUsers = $"{Prefix}/conversation/{{otherUserId}}";
            public const string GetChatMessage = $"{Prefix}/{{id}}";
            public const string CreateChatMessage = Prefix;
            public const string UpdateChatMessage = Prefix;
            public const string DeleteChatMessage = $"{Prefix}/{{id}}";
            public const string GetUnreadCount = $"{Prefix}/unread-count";
            public const string MarkAsRead = $"{Prefix}/{{id}}/read";
            public const string GetConversationInfo = $"{Prefix}/conversation-info/{{otherUserId}}";
            public const string GetConversationList = $"{Prefix}/conversations";
        }

        public static class Notification
        {
            private const string Prefix = "api/notifications";
            
            public const string GetNotifications = Prefix;
            public const string GetNotification = $"{Prefix}/{{id}}";
            public const string CreateNotification = Prefix;
            public const string UpdateNotification = Prefix;
            public const string DeleteNotification = $"{Prefix}/{{id}}";
            public const string GetUnreadCount = $"{Prefix}/unread-count";
            public const string MarkAsRead = $"{Prefix}/{{id}}/read";
            public const string MarkAllAsRead = $"{Prefix}/read-all";
            public const string CreateCartNotification = $"{Prefix}/cart";
            public const string SendNotificationToAllUsers = $"{Prefix}/send-to-all";
            public const string GetBroadcastNotifications = $"{Prefix}/broadcasts";
        }

        public static class Payment
        {
            private const string BaseEndpoint = "~/" + AreaName + "/payment";
            public const string VnpayUrl = BaseEndpoint + "/vnpay/payment-url";
            public const string VnpayExecute = BaseEndpoint + "vnpay/payment-execute";
        }

        public static class UserDevice
        {
            private const string Prefix = "api/user-devices";
            
            public const string RegisterDevice = $"{Prefix}/register";
            public const string UnregisterDevice = $"{Prefix}/unregister";
            public const string GetUserDevices = Prefix;
        }

        public static class StoreLocation
        {
            private const string BaseEndpoint = "~/" + AreaName + "/store-location";
            public const string GetStoreLocations = BaseEndpoint;
            public const string CreateStoreLocation = BaseEndpoint;
            public const string UpdateStoreLocation = BaseEndpoint;
            public const string DeleteStoreLocation = BaseEndpoint + "/{id}";
        }
    }
}