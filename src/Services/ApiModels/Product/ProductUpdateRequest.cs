using System.ComponentModel.DataAnnotations;

namespace Services.ApiModels.Product
{
    public class ProductUpdateRequest : ProductCreateRequest
    {
        [Required]
        public int ProductId { get; set; }
    }
}
