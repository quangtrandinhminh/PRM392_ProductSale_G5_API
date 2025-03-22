using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Service.Utils;
using Services.ApiModels;
using Services.ApiModels.Cart;
using Services.ApiModels.CartItem;
using Services.Constants;
using Services.Enum;
using Services.Services;

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICartService _cartService;
      
        public CartController(IServiceProvider serviceProvider)
        {
            _cartService  = serviceProvider.GetRequiredService<ICartService>();
             _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
          
        }

        [HttpPost]
        [Route(WebApiEndpoint.Cart.AddToCart)]
        public async Task<IActionResult> CreateCart([FromBody] CartItemRequest request)
        {
            var response = await _cartService.AddProductToCartAsync(request);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpGet]
        [Route(WebApiEndpoint.Cart.GetCarts)]
        public async Task<IActionResult> GetCarts()
        {
            return Ok(BaseResponse.OkResponseDto(await _cartService.GetCartByUserIdAsync()));
        }

        [HttpPut]
        [Route(WebApiEndpoint.Cart.UpdateCart)]
        public async Task<IActionResult> UpdateCart([FromBody] UpdateCartRequest request)
        {
            var result = await _cartService.UpdateProductQuantityAsync(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }


        /*[HttpDelete]
        [Route("DeleteCartItem")]
        public async Task<IActionResult> DeleteCartItem([FromQuery] int productId)
        {
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);

            // Ensure user is authenticated
            if (currentUserId <= 0)
            {
                return Unauthorized(BaseResponse.BadRequestResponseDto("User not authenticated"));
            }

            // Validate productId
            if (productId <= 0)
            {
                return BadRequest(BaseResponse.BadRequestResponseDto("Invalid product ID"));
            }

            // Call service to delete the cart item
            var result = await _cartService.DeleteCartItemAsync(currentUserId, productId);
            return Ok(BaseResponse.OkResponseDto(result));
        }*/

    }
}
