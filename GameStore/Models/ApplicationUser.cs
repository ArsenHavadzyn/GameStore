using Microsoft.AspNetCore.Identity;

namespace GameStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public string ProfilePictureUrl { get; set; }
    }

}
