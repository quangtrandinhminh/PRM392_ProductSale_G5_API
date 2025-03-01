using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Services.Constants;

namespace Services.ApiModels.Product
{
    public class ProductCreateRequest
    {
        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; }
        
        [Required]
        public string FullDescription { get; set; }
        [Required]
        public string BriefDescription { get; set; }

        [Required]
        public string TechnicalSpecifications { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue, ErrorMessage = ResponseMessageConstantsCommon.DECIMAL_INVALID)]
        public decimal Price { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        public IFormFile Image { get; set; }
    }
}
