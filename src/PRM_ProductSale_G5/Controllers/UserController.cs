using Microsoft.AspNetCore.Mvc;
using Services.ApiModels;
using Services.ApiModels.PaginatedList;
using Services.ApiModels.User;
using Services.Constants;
using Services.Services;

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    public class UserController(IServiceProvider serviceProvider) : Controller
    {
        private readonly UserService _userService = serviceProvider.GetRequiredService<UserService>();

        [HttpGet]
        [Route(WebApiEndpoint.User.GetUsers)]
        public async Task<IActionResult> GetUser([FromQuery] PaginatedListRequest request)
        {
            return Ok(BaseResponse.OkResponseDto(
                await _userService.GetAllUsersAsync(request.PageNumber, request.PageSize)));
        }

        [HttpGet]
        [Route(WebApiEndpoint.User.GetUser)]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            return Ok(BaseResponse.OkResponseDto(await _userService.GetUserByIdAsync(id)));
        }

        [HttpPut]
        [Route(WebApiEndpoint.User.UpdateUser)]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request)
        {
            return Ok(BaseResponse.OkResponseDto(await _userService.UpdateUserAsync(request)));
        }

        [HttpDelete]
        [Route(WebApiEndpoint.User.DeleteUser)]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            return Ok(BaseResponse.OkResponseDto(await _userService.DeleteUserAsync(id)));
        }
    }
}
