using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using Repositories.Repositories;
using Services.Mapper;
using Microsoft.AspNetCore.Http;
using Repositories.Base;
using Serilog;
using Services.Constants;
using Services.Exceptions;
using Services.ApiModels.Cart;
using Service.Utils;
using Services.ApiModels.CartItem;
using Services.Enum;

namespace Services.Services;

public interface ICartService
{
    Task<CartResponse> GetCartByUserIdAsync();
    Task<int> AddProductToCartAsync(CartItemRequest request);
    Task<int> UpdateProductQuantityAsync(UpdateCartRequest request);
    Task<bool> DeleteCartItemAsync(int cartItemId);
}

public class CartService(IServiceProvider serviceProvider) : ICartService
{
    private readonly ICartRepository _cartRepository = serviceProvider.GetRequiredService<ICartRepository>();
    private readonly ICartItemRepository _cartItemRepository = serviceProvider.GetRequiredService<ICartItemRepository>();
    private readonly IProductRepository _productRepository = serviceProvider.GetRequiredService<IProductRepository>();
    private readonly IUserRepository _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly MapperlyMapper _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();
    private readonly IHttpContextAccessor _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

    public async Task<CartResponse> GetCartByUserIdAsync()
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var userId = JwtClaimUltils.GetUserId(currentUser);
        _logger.Information("Get cart by userId: {UserId}", userId);

        // Eagerly load CartItems and then the Product for each CartItem
        var cart = await _cartRepository.GetCartByUserIdAsync(userId, CartEnum.Active.ToString());
        if (cart == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, "Cart not found", StatusCodes.Status404NotFound);
        }

        var response = _mapper.Map(cart);
        return response;
    }

    public async Task<int> AddProductToCartAsync(CartItemRequest request)
    {
        // Get current logged in user
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var userId = JwtClaimUltils.GetUserId(currentUser);
        _logger.Information("Add product {productId} with quantity {quantity} to cart by userId: {UserId}",
                            request.ProductId, request.Quantity, userId);

        // Ensure product exists
        var product = await _productRepository.GetSingleAsync(p => p.ProductId == request.ProductId);
        if (product == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, "Product does not exist", StatusCodes.Status400BadRequest);
        }

        // Fetch the cart including its items
        var cart = await _cartRepository.GetSingleAsync(c => c.UserId == userId, c => c.CartItems);
        var result = 0;

        // If cart does not exist, create a new one with an empty CartItems list.
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                Status = CartEnum.Active.ToString(),
                TotalPrice = 0,
                CartItems = new List<CartItem>()
            };

            cart = await _cartRepository.AddAsync(cart);
            result += await _cartRepository.SaveChangeAsync();
        }

        // Check if the cart already contains this product
        var existingCartItem =
            await _cartItemRepository.GetSingleAsync(_ => _.CartId == cart.CartId && _.ProductId == request.ProductId);
        if (existingCartItem != null)
        {
            // Update the quantity and price for the existing item
            existingCartItem.Quantity += request.Quantity;
            existingCartItem.Price = existingCartItem.Quantity * product.Price;

            cart.TotalPrice += existingCartItem.Price;
            _cartItemRepository.Update(existingCartItem);
        }
        else
        {
            // Create a new cart item and add it to the cart
            var newCartItem = new CartItem
            {
                CartId = cart.CartId,  // This should be 0 if cart is new; repository will handle it.
                ProductId = product.ProductId,
                Quantity = request.Quantity,
                Price = product.Price * request.Quantity
            };

            cart.CartItems.Add(newCartItem);
            //var cartItem = await _cartItemRepository.AddAsync(newCartItem);
            // log cartItem
        }

        // Recalculate the cart's total price
        cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price);
        _cartRepository.Update(cart);

        result += (await _cartRepository.SaveChangeAsync() + await _cartItemRepository.SaveChangeAsync());
        return result;
    }

    public async Task<int> UpdateProductQuantityAsync(UpdateCartRequest request)
    {
        // Get the current logged in user
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var userId = JwtClaimUltils.GetUserId(currentUser);

        _logger.Information("Update cartItem {cartItemId} for user {userId}", request.CartItemId, userId);

        var cartItem = await _cartItemRepository.GetSingleAsync(ci => ci.CartItemId == request.CartItemId, 
            ci => ci.Cart, ci => ci.Product);

        if (cartItem == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND,
                ResponseMessageConstraintsCart.NOT_FOUND_ITEM, StatusCodes.Status404NotFound);
        }

        if (cartItem.Cart.UserId != userId)
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN,
               ResponseMessageConstraintsCart.NOT_ALLOWED, StatusCodes.Status403Forbidden);
        }

        // Ensure product exists
        var product = await _productRepository.GetSingleAsync(p => p.ProductId == cartItem.ProductId);
        if (product == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, "Product does not exist", StatusCodes.Status404NotFound);
        }

        // update quantity and calculate price
        cartItem.Quantity = request.Quantity;
        cartItem.Price = product.Price * cartItem.Quantity;
        _cartItemRepository.Update(cartItem);
        var result = await _cartItemRepository.SaveChangeAsync();

        // Recalculate the cart's total price based on the (updated) cart items
        var cart = await _cartRepository.GetSingleAsync(c => c.CartId == cartItem.CartId, c => c.CartItems);
        cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price);
        _cartRepository.Update(cart); // Mark the cart as modified

        result += await _cartRepository.SaveChangeAsync();

        return result;
    }

    // Remove product from cart
    public async Task<bool> DeleteCartItemAsync(int cartItemId)
    {
        // Get the current logged in user
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var userId = JwtClaimUltils.GetUserId(currentUser);

        _logger.Information("Delete cartItem {cartItemId} for user {userId}", cartItemId, userId);

        var cartItem = await _cartItemRepository.GetSingleAsync(ci => ci.CartItemId == cartItemId,
            ci => ci.Cart, ci => ci.Product);

        if (cartItem == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, 
                ResponseMessageConstraintsCart.NOT_FOUND_ITEM, StatusCodes.Status404NotFound);
        }

        if (cartItem.Cart.UserId != userId)
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN,
                              ResponseMessageConstraintsCart.NOT_ALLOWED, StatusCodes.Status403Forbidden);
        }

        _cartItemRepository.Remove(cartItem);
        var result = await _cartItemRepository.SaveChangeAsync();

        // Recalculate the cart's total price based on the (updated) cart items
        var cart = await _cartRepository.GetSingleAsync(c => c.CartId == cartItem.CartId, c => c.CartItems);
        cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price);
        _cartRepository.Update(cart); // Mark the cart as modified

        result += await _cartRepository.SaveChangeAsync();

        return result > 0;
    }
}
