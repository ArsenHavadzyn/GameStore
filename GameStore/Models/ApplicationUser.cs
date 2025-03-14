using Microsoft.AspNetCore.Identity;

namespace GameStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string Token { get; set; }
    }

}
