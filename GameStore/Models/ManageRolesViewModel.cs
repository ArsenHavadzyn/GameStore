using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace GameStore.Models
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }

        public List<IdentityRole> AvailableRoles { get; set; } = new();
        public IList<string> AssignedRoles { get; set; } = new List<string>();

        public List<UserPurchaseViewModel> Purchases { get; set; } = new();
    }

    public class UserPurchaseViewModel
    {
        public DateTime Date { get; set; }
        public List<string> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
