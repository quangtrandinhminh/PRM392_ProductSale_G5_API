using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using Repositories.Repositories;
using Services.ApiModels.PaginatedList;
using Services.ApiModels.Product;
using Services.Constants;
using Services.Exceptions;
using Services.Mapper;
using Serilog;
using Microsoft.AspNetCore.Http;

namespace Services.Services
{
    public interface IProductService
    {
        Task<PaginatedListResponse<ProductResponse>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<ProductResponse> GetProductByIdAsync(int id);
        Task<ProductResponse> CreateProductAsync(ProductCreateRequest request);
        Task<int> UpdateProductAsync(ProductUpdateRequest request);
        Task<int> DeleteProductAsync(int id);
        Task<PaginatedListResponse<ProductResponse>> SearchProductByNameAsync(string productName, int pageNumber, int pageSize);
    }

    public class ProductService(IServiceProvider serviceProvider) : IProductService
    {
        private readonly IProductRepository _productRepository = serviceProvider.GetRequiredService<IProductRepository>();
        private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
        private readonly ICloudinaryService _cloudinaryService = serviceProvider.GetRequiredService<ICloudinaryService>();
        private readonly MapperlyMapper _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();

        public async Task<PaginatedListResponse<ProductResponse>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            _logger.Information($"Getting {pageSize} products at page {pageNumber}");
            var products = await _productRepository.GetAllPaginatedQueryable(pageNumber, pageSize);
            if (products is null)
            {
                throw new AppException(ResponseCodeConstants.BAD_REQUEST,
                    ResponseMessageConstrantsProduct.NOTFOUND, StatusCodes.Status400BadRequest);
            }

            return _mapper.Map(products);
        }

        public async Task<ProductResponse> GetProductByIdAsync(int id)
        {
            _logger.Information($"Getting product with id {id}");
            var product = await _productRepository.GetSingleAsync(x => x.ProductId == id);
            return _mapper.Map(product);
        }

        public async Task<ProductResponse> CreateProductAsync(ProductCreateRequest request)
        {
            _logger.Information("Creating new product {request}", request);
            var product = _mapper.Map(request);
            product.ImageUrl = await _cloudinaryService.UploadImageAsync(request.Image);
            _productRepository.Create(product);
            await _productRepository.SaveChangeAsync();
            return _mapper.Map(product);
        }

        public async Task<int> UpdateProductAsync(ProductUpdateRequest request)
        {
            _logger.Information("Updating product {request}", request.ProductId);
            var product = await GetProductById(request.ProductId);
            product.ImageUrl = await _cloudinaryService.UploadImageAsync(request.Image);
            _mapper.Map(request, product);
            _productRepository.Update(product);
            return await _productRepository.SaveChangeAsync();
        }

        public async Task<int> DeleteProductAsync(int id)
        {
            _logger.Information("Deleting product with id {id}", id);
            var product = await GetProductById(id);
            _productRepository.Remove(product);
            return await _productRepository.SaveChangeAsync();
        }

        private async Task<Product> GetProductById(int id)
        {
            var product = await _productRepository.GetSingleAsync(x => x.ProductId == id);
            if (product is null)
            {
                throw new AppException(ResponseCodeConstants.BAD_REQUEST,
                    ResponseMessageConstrantsProduct.NOTFOUND, StatusCodes.Status400BadRequest);
            }

            return product;
        }

        public async Task<PaginatedListResponse<ProductResponse>> SearchProductByNameAsync(string productName, int pageNumber, int pageSize)
        {
            _logger.Information($"Searching product with name {productName}");
            var products = await _productRepository.GetAllPaginatedQueryable(pageNumber, pageSize, x => x.ProductName.Contains(productName));
            if (products is null)
            {
                throw new AppException(ResponseCodeConstants.BAD_REQUEST,
                    ResponseMessageConstrantsProduct.NOTFOUND, StatusCodes.Status400BadRequest);
            }

            return _mapper.Map(products);
        }
    }
}
