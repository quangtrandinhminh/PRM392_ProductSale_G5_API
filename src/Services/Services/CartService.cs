using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using Repositories.Repositories;
using Services.Mapper;
using System;
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
using Services.Enum;
using Services.ApiModels.NewFolder;


namespace Services.Services;

public interface ICartService
{
    // get cart by user id, including cart items

    // add product to cart
    Task<CartResponse> GetCartByUserIdAsync(int userId);
    Task<bool> AddProductToCartAsync(int userId, int productId, int quantity);
    Task<bool> UpdateProductQuantityAsync(int userId, int productId, int quantity);
    Task<bool> RemoveProductFromCartAsync(int userId, int productId);

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


    // Get cart by user ID, including cart items


    public async Task<CartResponse> GetCartByUserIdAsync(int userId)
    {

        var cart = await _cartRepository.GetSingleAsync(
            x => x.UserId == userId && x.Status == CartEnum.Active.ToString(),
            x => x.CartItems,
            x => x.CartItems.Select(ci => ci.Product)
        );

        if (cart == null)
        {
            return null;
        }


        var response = _mapper.Map(cart);

        // ✅ Ensure CartItems include product details
        response.CartItems = cart.CartItems?.Select(ci => new CartItemResponse
        {
            ProductId = ci.ProductId ?? 0,
            Quantity = ci.Quantity,
            Price = ci.Price,
            ProductName = ci.Product?.ProductName ?? "Unknown"
        }).ToList() ?? new List<CartItemResponse>();


        return response;
    }





    // Add product to cart
    public async Task<bool> AddProductToCartAsync(int userId, int productId, int quantity)
    {
        try
        {
            // ✅ Ensure user exists
            var user = await _userRepository.GetSingleAsync(u => u.UserId == userId);
            if (user == null) throw new Exception("User does not exist");

            // ✅ Ensure product exists
            var product = await _productRepository.GetSingleAsync(p => p.ProductId == productId);
            if (product == null) throw new Exception("Product does not exist");

            // ✅ Fetch cart, include cart items
            var cart = await _cartRepository.GetSingleAsync(c => c.UserId == userId, c => c.CartItems);

            if (cart == null)
            {
                // ✅ Create new cart and ensure it's saved
                cart = new Cart
                {
                    UserId = userId,
                    Status = CartEnum.Active.ToString(),
                    TotalPrice = 0,
                    CartItems = new List<CartItem>()
                };

                _cartRepository.Create(cart);
                await _cartRepository.SaveChangeAsync(); // Ensures cart has an ID
            }

            // ✅ Find existing cart item
            var existingCartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                existingCartItem.Price = existingCartItem.Quantity * product.Price;
                _cartItemRepository.Update(existingCartItem);
            }
            else
            {
                // ✅ Ensure new cart item is added correctly
                var newCartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price * quantity
                };

                _cartItemRepository.Create(newCartItem);
                _cartItemRepository.SaveChangeAsync();
            }

            // ✅ Recalculate total price correctly
            cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price);
            _cartRepository.Update(cart);

            // ✅ Save changes in a transaction
            await _cartRepository.SaveChangeAsync();
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            throw new Exception("Database update failed. Possible constraint violation.", dbEx);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding product to cart.", ex);
        }
    }



    //Update product quantity in cart


    public async Task<bool> UpdateProductQuantityAsync(int userId, int productId, int quantity)
    {
        

        // Retrieve cart with its items
        var cart = await _cartRepository.GetSingleAsync(c => c.UserId == userId, c => c.CartItems);
        if (cart is null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND,
                                   "Cart not found",
                                   StatusCodes.Status404NotFound);
        }

        // Find the cart item
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (cartItem is null)
        {
            throw new AppException(ResponseCodeConstants.NOT_FOUND,
                                   "Product not found in cart",
                                   StatusCodes.Status404NotFound);
        }

        // Preserve original unit price before modifying quantity
        var unitPrice = cartItem.Price / (cartItem.Quantity > 0 ? cartItem.Quantity : 1);
        cartItem.Quantity = quantity;
        cartItem.Price = quantity * unitPrice;

        _cartItemRepository.Update(cartItem);

        // Recalculate total price
        cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price);
        _cartRepository.Update(cart);

        try
        {
            await _cartRepository.SaveChangeAsync();
            return true;
        }
        catch (Exception ex)
        {
           
            throw new AppException(ResponseCodeConstants.NOT_FOUND,
                                   "An error occurred while updating the cart",
                                   StatusCodes.Status500InternalServerError);
        }
    }



    // Remove product from cart
    public async Task<bool> RemoveProductFromCartAsync(int userId, int productId)
    {
        var cart = await _cartRepository.GetSingleAsync(c => c.UserId == userId, c => c.CartItems);
        if (cart == null) return false;

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (cartItem == null) return false;

        _cartItemRepository.Remove(cartItem);

        cart.TotalPrice = cart.CartItems.Where(ci => ci.ProductId != productId).Sum(ci => ci.Price);
        _cartRepository.Update(cart);
        await _cartRepository.SaveChangeAsync();

        return true;
    }
}
