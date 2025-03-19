using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ApiModels;
using Services.ApiModels.PaginatedList;
using Services.ApiModels.Product;
using Services.Constants;
using Services.Services;
using System;

namespace PRM_ProductSale_G5.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ProductService _productService = serviceProvider.GetRequiredService<ProductService>();

        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        [Route(WebApiEndpoint.Product.GetProducts)]
        public async Task<IActionResult> GetProducts([FromQuery] PaginatedListRequest request)
        {
            return Ok(BaseResponse.OkResponseDto(
                await _productService.GetAllProductsAsync(request.PageNumber, request.PageSize)));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        [Route(WebApiEndpoint.Product.GetProduct)]
        public async Task<IActionResult> GetProduct([FromRoute] int id)
        {
            return Ok(BaseResponse.OkResponseDto(await _productService.GetProductByIdAsync(id)));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route(WebApiEndpoint.Product.UpdateProduct)]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateRequest request)
        {
            return Ok(BaseResponse.OkResponseDto(await _productService.UpdateProductAsync(request)));
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route(WebApiEndpoint.Product.DeleteProduct)]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            return Ok(BaseResponse.OkResponseDto(await _productService.DeleteProductAsync(id)));
        }
    }
}