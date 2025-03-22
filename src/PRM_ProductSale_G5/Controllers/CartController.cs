using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Service.Utils;
using Services.ApiModels;
using Services.ApiModels.Cart;
using Services.Constants;
using Services.Services;

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICartService _cartService;
      
        public CartController(IServiceProvider serviceProvider)
        {
            _cartService  = serviceProvider.GetRequiredService<ICartService>();
             _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
          
        }
        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateCart([FromQuery] int productId, [FromQuery] int quantity)
        {
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);

            if (currentUserId == null)
            {
                return Unauthorized(BaseResponse.BadRequestResponseDto("User not authenticated"));
            }

            var response = await _cartService.AddProductToCartAsync(currentUserId, productId, quantity);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpGet]
        [Route("GetCarts")]
        public async Task<IActionResult> GetCarts()
        {
            try
            {
                var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
                var currentUserId = JwtClaimUltils.GetUserId(currentUser);

                if (currentUserId == null || currentUserId == 0)
                {
                    return Unauthorized(BaseResponse.InternalErrorResponseDto("Invalid or missing authentication token.")); // ✅ Unauthorized response
                }

                var cartResponse = await _cartService.GetCartByUserIdAsync(currentUserId);
                if (cartResponse == null)
                {
                    return NotFound(BaseResponse.InternalErrorResponseDto("No active cart found.")); // ✅ Not Found response
                }

                return Ok(BaseResponse.OkResponseDto(cartResponse)); // ✅ Successful response
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponse.InternalErrorResponseDto("An error occurred while retrieving the cart.")); // ✅ Internal Server Error response
            }
        }

        [HttpPut]
        [Route("Updatecart")]
        public async Task<IActionResult> UpdateCart([FromBody] UpdateCartRequest request)
        {
            // Get logged-in user from JWT token
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);

            // Ensure user is authenticated
            if (currentUserId <= 0) // Assuming 0 or negative is invalid
            {
                return Unauthorized(BaseResponse.BadRequestResponseDto("User not authenticated"));
            }

            var result = await _cartService.UpdateProductQuantityAsync(currentUserId, request.ProductId, request.Quantity);

            return Ok(BaseResponse.OkResponseDto(request));
        }


        [HttpDelete]
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
        }

    }
}
