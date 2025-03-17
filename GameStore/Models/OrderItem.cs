using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        [Display(Name = "Ціна за одиницю")]
        public decimal Price { get; set; }
        public decimal TotalPrice { get; internal set; } = 0;
        public string ProductTitle { get; internal set; }
    }
}
