using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace GameStore.Models
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<IdentityRole> AvailableRoles { get; set; }
        public IList<string> AssignedRoles { get; set; }
    }

}
