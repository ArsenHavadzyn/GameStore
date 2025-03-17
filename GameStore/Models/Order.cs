using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalPrice { get; set; } = 0;
        public string Status { get; set; } = "Pending";

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public string DigitalCode { get; set; } = string.Empty;

        public string? CustomEmail { get; set; }

    }

}
