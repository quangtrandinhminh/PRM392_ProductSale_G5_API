using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Services.Constants;

namespace Services.ApiModels.Product
{
    public class ProductUpdateRequest : ProductCreateRequest
    {
        [Required]
        public int ProductId { get; set; }
    }
}
