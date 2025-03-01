using Services.Constants;
using System.ComponentModel.DataAnnotations;

namespace Services.ApiModels.Cart
{
    public class CartRequest
    {
        [Required]
        public int? UserId { get; set; }

        [Required]
        [Range(0.00001, double.MaxValue, ErrorMessage = ResponseMessageConstantsCommon.DECIMAL_INVALID)]
        public decimal TotalPrice { get; set; }

        public string Status { get; set; }
    }
}