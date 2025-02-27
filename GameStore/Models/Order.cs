﻿using Microsoft.AspNetCore.Identity;

namespace GameStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";

        public List<OrderItem> OrderItems { get; set; } = new();
    }

}
