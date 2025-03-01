using Services.ApiModels.Category;

namespace Services.ApiModels.Product
{
    public class ProductResponse
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string BriefDescription { get; set; }

        public string FullDescription { get; set; }

        public string TechnicalSpecifications { get; set; }

        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public int? CategoryId { get; set; }

        public CategoryResponse? Category { get; set; }
    }
}
