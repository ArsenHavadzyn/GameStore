using System.ComponentModel.DataAnnotations;

namespace GameStore.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Поле 'Повне ім'я' є обов'язковим")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле 'Email' є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        public string Email { get; set; } = string.Empty;

        public string ProfilePictureUrl { get; set; } = string.Empty;

        [Display(Name = "Фото профілю")]
        public IFormFile? ProfilePicture { get; set; }
    }
}
