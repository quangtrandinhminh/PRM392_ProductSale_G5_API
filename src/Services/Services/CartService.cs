using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using Repositories.Repositories;
using Services.Mapper;
using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Base;
using Repositories.Models;
using Repositories.Repositories;
using Serilog;
using Services.ApiModels.PaginatedList;
using Services.ApiModels.User;
using Services.Constants;
using Services.Exceptions;
using Services.ApiModels.Cart;
using Microsoft.EntityFrameworkCore;
using Service.Utils;
using Services.ApiModels.CartItem;
using Services.Enum;


namespace Services.Services;

public interface ICartService
{
    // get cart by user id, including cart items

    // add product to cart
    Task<CartResponse> GetCartByUserIdAsync();
    Task<int> AddProductToCartAsync(CartItemRequest request);
    Task<int> UpdateProductQuantityAsync(UpdateCartRequest request);
    // Task<bool> DeleteCartItemAsync(int userId, int productId);

}

//public class CartService(IServiceProvider serviceProvider) : ICartService
//{
//    private readonly ICartRepository _cartRepository = serviceProvider.GetRequiredService<ICartRepository>();
//}

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


    // Get cart by user ID, including cart items


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

        // Ensure Product is loaded.
        /*if (cart.CartItems != null)
        {
            foreach (var cartItem in cart.CartItems)
            {
                // Load the Product for each CartItem.  This is necessary because the original query only loads the CartItem
                if (cartItem.Product == null)
                {
                    // Assuming you have a _productRepository or similar
                    cartItem.Product = await _productRepository.GetSingleAsync(p => p.ProductId == cartItem.ProductId);
                }
            }
        }*/

        var response = _mapper.Map(cart);

        // ✅ Ensure CartItems include product details
        /*response.CartItems = cart.CartItems?.Select(ci => new CartItemResponse
        {
            ProductId = ci.ProductId ?? 0,
            Quantity = ci.Quantity,
            Price = ci.Price,
            ProductName = ci.Product?.ProductName ?? "Unknown"
        }).ToList() ?? new List<CartItemResponse>();*/

        return response;
    }

    public async Task<int> AddProductToCartAsync(CartItemRequest request)
    {
        try
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
            }
            else if (cart.CartItems == null)
            {
                cart.CartItems = new List<CartItem>();
            }

            // Check if the cart already contains this product
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            if (existingCartItem != null)
            {
                // Update the quantity and price for the existing item
                existingCartItem.Quantity += request.Quantity;
                existingCartItem.Price = existingCartItem.Quantity * product.Price;
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
                await _cartItemRepository.AddAsync(newCartItem);
            }

            // Recalculate the cart's total price
            cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price);

            // If the cart is new (CartId == 0), create it; otherwise, update it.
            if (cart.CartId == null)
            {
                await _cartRepository.AddAsync(cart);
            }
            else
            {
                _cartRepository.Update(cart);
            }

            var result = await _cartRepository.SaveChangeAsync() + await _cartItemRepository.SaveChangeAsync();
            return result;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.Error(dbEx, "Database update failed when adding product to cart");
            throw new Exception("Database update failed. Possible constraint violation.", dbEx);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while adding product to cart");
            throw new Exception("An error occurred while adding product to cart.", ex);
        }
    }

    public async Task<int> UpdateProductQuantityAsync(UpdateCartRequest request)
    {
        // Get the current logged in user
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var userId = JwtClaimUltils.GetUserId(currentUser);

        _logger.Information("Update cart {cartId}", request.CartId);

        // Retrieve cart with tracking enabled, including its CartItems
        var cart = await _cartRepository.GetSingleAsync(
            c => c.CartId == request.CartId,
            c => c.CartItems
        );

        if (cart == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, 
                ResponseMessageConstraintsCart.NOT_FOUND, StatusCodes.Status404NotFound);
        }

        if (cart.UserId != userId)
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN, 
               ResponseMessageConstraintsCart.NOT_ALLOWED, StatusCodes.Status403Forbidden);
        }

        // Iterate through the request's cart items
        foreach (var item in request.CartItems)
        {
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == item.ProductId);
            if (cartItem == null)
            {
                throw new AppException(ResponseCodeConstants.NOT_FOUND, "Product not found in cart", StatusCodes.Status404NotFound);
            }

            // Ensure product exists
            var product = await _productRepository.GetSingleAsync(p => p.ProductId == item.ProductId);
            if (product == null)
            {
                throw new AppException(ResponseCodeConstants.NOT_FOUND, "Product does not exist", StatusCodes.Status404NotFound);
            }

            // If quantity is 0, remove the cart item; else, update quantity and calculate price
            if (item.Quantity == 0)
            {
                _cartItemRepository.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = item.Quantity;
                cartItem.Price = product.Price * item.Quantity;
                _cartItemRepository.Update(cartItem); // Mark the cart item as modified
            }
        }

        // Recalculate the cart's total price based on the (updated) cart items
        cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price);
        _cartRepository.Update(cart); // Mark the cart as modified

        try
        {
            return await _cartRepository.SaveChangeAsync() + await _cartItemRepository.SaveChangeAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to update the cart");
            throw new AppException(ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                                   "Failed to update the cart",
                                   StatusCodes.Status500InternalServerError);
        }
    }

    // Remove product from cart
    /*public async Task<bool> DeleteCartItemAsync(int userId, int productId)
    {
        // Retrieve cart with tracking enabled
        var cart = await _cartRepository.GetSingleAsync(
            c => c.UserId == userId,
            c => c.CartItems
        );

        if (cart == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, "Cart not found", StatusCodes.Status404NotFound);
        }

        // Find the cart item
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (cartItem == null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND, "Product not found in cart", StatusCodes.Status404NotFound);
        }

        // ✅ Remove the cart item
        _cartItemRepository.Remove(cartItem);
        await _cartItemRepository.SaveChangeAsync(); // Save immediately

        // Recalculate total price
        cart.TotalPrice = cart.CartItems.Where(ci => ci.ProductId != productId).Sum(ci => ci.Price);

        // If no items left, delete the cart
        if (!cart.CartItems.Any(ci => ci.ProductId != productId))
        {
            _cartRepository.Remove(cart);
        }
        else
        {
            _cartRepository.Update(cart);
        }

        try
        {
            await _cartRepository.SaveChangeAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new AppException(ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                                   "Failed to delete the cart item",
                                   StatusCodes.Status500InternalServerError);
        }
    }*/
}
