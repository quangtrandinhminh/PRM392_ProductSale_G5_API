using System.ComponentModel.DataAnnotations;

namespace Services.ApiModels.Category
{
    public class CategoryUpdateRequest : CategoryCreateRequest
    {
        [Required]
        public int CategoryId { get; set; }
    }
}