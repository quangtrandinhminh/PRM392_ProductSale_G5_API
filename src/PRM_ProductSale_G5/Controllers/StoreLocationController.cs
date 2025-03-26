using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.ApiModels;
using Services.Constants;
using Services.Services;

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    public class StoreLocationController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly IStoreLocationService _storeLocationService = serviceProvider.GetRequiredService<IStoreLocationService>();

        [Authorize]
        [HttpGet(WebApiEndpoint.StoreLocation.GetStoreLocations)]
        public async Task<IActionResult> GetStoreLocations()
        {
            var storeLocations = await _storeLocationService.GetStoreLocations();
            return Ok(BaseResponse.OkResponseDto(storeLocations));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost(WebApiEndpoint.StoreLocation.CreateStoreLocation)]
        public async Task<IActionResult> CreateStoreLocation([FromBody] StoreLocation storeLocation)
        {
            var result = await _storeLocationService.CreateStoreLocation(storeLocation);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut(WebApiEndpoint.StoreLocation.UpdateStoreLocation)]
        public async Task<IActionResult> UpdateStoreLocation([FromBody] StoreLocation storeLocation)
        {
            var result = await _storeLocationService.UpdateStoreLocation(storeLocation);
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete(WebApiEndpoint.StoreLocation.DeleteStoreLocation)]
        public async Task<IActionResult> DeleteStoreLocation([FromRoute] int id)
        {
            var result = await _storeLocationService.DeleteStoreLocation(id);
            return Ok(BaseResponse.OkResponseDto(result));
        }
    }
}
