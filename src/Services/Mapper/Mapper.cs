using System.Text.Json;
using Repositories.Extensions;
using Repositories.Models;
using Riok.Mapperly.Abstractions;
using Services.ApiModels.Cart;
using Services.ApiModels.Category;
using Services.ApiModels.Order;
using Services.ApiModels.PaginatedList;
using Services.ApiModels.Product;
using Services.ApiModels.User;

namespace Services.Mapper;

[Mapper]
public partial class MapperlyMapper
{
    // user
    public partial User Map(RegisterRequest request);
    public partial LoginResponse Map(User entity);


    public partial User Map(UserCreateRequest request);
    public partial void Map(UserUpdateRequest request, User entity);

    public partial IList<UserResponse> Map(IList<User> entity);
    public partial IQueryable<UserResponse> Map(IQueryable<User> entity);
    public partial PaginatedListResponse<UserResponse> Map(PaginatedList<User> entity);
    public partial UserResponse MapToUserResponse(User entity);
    public partial void Map(RegisterRequest request, User entity);

    // product
    public partial Product Map(ProductCreateRequest request);
    public partial void Map(ProductUpdateRequest request, Product entity);
    public partial ProductResponse Map(Product entity);
    public partial PaginatedListResponse<ProductResponse> Map(PaginatedListResponse<Product> entity);

    // category
    public partial Category Map(CategoryCreateRequest request);
    public partial void Map(CategoryUpdateRequest request, Category entity);
    public partial CategoryResponse Map(Category entity);

    // order
    public partial Order Map(OrderRequest request);
    public partial OrderResponse Map(Order entity);
    public partial PaginatedListResponse<OrderResponse> Map(PaginatedList<Order> entity);



    // cart
    public partial Cart Map(CartRequest request);
    public partial CartResponse Map(Cart entity);
    public partial PaginatedListResponse<CartResponse> Map(PaginatedList<Cart> entity);


    // datetimeoffset to dateonly
    public DateOnly Map(DateTimeOffset dateTimeOffset)
    {
        return DateOnly.FromDateTime(dateTimeOffset.DateTime);
    }

    // datetime to dateonly
    public DateOnly Map(DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }

    // jsonstring to list double
    public IList<double> Map(string jsonString)
    {
        return JsonSerializer.Deserialize<List<double>>(jsonString);
    }
}