using Microsoft.AspNetCore.Mvc;
using Services.ApiModels;
using Services.ApiModels.User;
using Services.Constants;
using Services.Services;

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    public class AuthController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly AuthService service = serviceProvider.GetRequiredService<AuthService>();

        [HttpGet(WebApiEndpoint.Authentication.Hello)]
        public IActionResult Hello()
        {
            return Ok(BaseResponse.OkResponseDto("Hello"));
        }

        [HttpPost(WebApiEndpoint.Authentication.Login)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return Ok(BaseResponse.OkResponseDto(await service.Authenticate(request)));
        }

        [HttpPost(WebApiEndpoint.Authentication.Register)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await service.Register(request);
            return Ok(BaseResponse.OkResponseDto(ResponseMessageIdentitySuccess.REGIST_USER_SUCCESS));
        }
    }
}
