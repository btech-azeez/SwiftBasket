using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zepto.Backend.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Placed"; // Placed, Processing, Out for Delivery, Delivered, Cancelled

        [Required, MaxLength(250)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash on Delivery"; // Cash on Delivery, UPI, Card

        [Required, MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
