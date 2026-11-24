using System.ComponentModel.DataAnnotations;

namespace Zepto.Backend.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(50)]
        public string? Icon { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
