using System.ComponentModel.DataAnnotations;

namespace GameStore.Models
{
    public class OrderViewModel
    {
        public int ProductId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public List<OrderItemViewModel> CartItems { get; set; } = new();
        public decimal TotalAmount => CartItems?.Sum(item => item.TotalPrice) ?? 0;
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
