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
            private const string BaseEndpoint = "~/" + AreaName + "/order";
            public const string GetOrders = BaseEndpoint;
            public const string GetOrder = BaseEndpoint + "/{id}";
            public const string CreateOrder = BaseEndpoint;
            public const string UpdateOrder = BaseEndpoint;
            public const string DeleteOrder = BaseEndpoint + "/{id}";
        }

        public static class Cart
        {
            private const string BaseEndpoint = "~/" + AreaName + "/cart";
            public const string GetCarts = BaseEndpoint;
            public const string GetCart = BaseEndpoint + "/{id}";
            public const string CreateCart = BaseEndpoint;
            public const string UpdateCart = BaseEndpoint;
            public const string DeleteCart = BaseEndpoint + "/{id}";
        }

        public static class CartItem
        {
            private const string BaseEndpoint = "~/" + AreaName + "/cartItem";
            public const string GetCartItems = BaseEndpoint;
            public const string GetCartItem = BaseEndpoint + "/{id}";
            public const string CreateCartItem = BaseEndpoint;
            public const string UpdateCartItem = BaseEndpoint;
            public const string DeleteCartItem = BaseEndpoint + "/{id}";
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
            private const string BaseEndpoint = "~/" + AreaName + "/chatMessage";
            public const string GetChatMessages = BaseEndpoint;
            public const string GetChatMessage = BaseEndpoint + "/{id}";
            public const string CreateChatMessage = BaseEndpoint;
            public const string UpdateChatMessage = BaseEndpoint;
            public const string DeleteChatMessage = BaseEndpoint + "/{id}";
        }

        public static class Notification
        {
            private const string BaseEndpoint = "~/" + AreaName + "/notification";
            public const string GetNotifications = BaseEndpoint;
            public const string CreateNotification = BaseEndpoint;
        }

        public static class Payment
        {
            private const string BaseEndpoint = "~/" + AreaName + "/payment";
            public const string VnpayUrl = BaseEndpoint + "/vnpay/payment-url";
            public const string VnpayExecute = BaseEndpoint + "vnpay/payment-execute";
        }
    }
}