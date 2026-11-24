using System.ComponentModel.DataAnnotations;

namespace Zepto.Backend.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool InStock { get; set; }
        public bool IsFeatured { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CreateProductDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        [Range(0, 10000)]
        public int StockQuantity { get; set; } = 50;

        [MaxLength(50)]
        public string Unit { get; set; } = "1 unit";

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool InStock { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto : CreateProductDto
    {
    }
}
