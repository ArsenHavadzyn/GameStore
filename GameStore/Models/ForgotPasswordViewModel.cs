using System.ComponentModel.DataAnnotations;

namespace GameStore.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
