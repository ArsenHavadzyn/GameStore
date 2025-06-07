using GameStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GameStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Назва")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Опис")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Ціна")]
        public decimal Price { get; set; }

        [Display(Name = "Знижка (%)")]
        public int Discount { get; set; } = 0;

        [NotMapped]
        public decimal DiscountedPrice => Price * (1 - (Discount / 100.0m));

        [Display(Name = "Дата виходу")]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "Розробник")]
        public string Developer { get; set; } = string.Empty;

        [Display(Name = "Видавець")]
        public string Publisher { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "Платформи")]
        public List<Platform> Platforms { get; set; } = new List<Platform>();

        [Column("Platforms")]
        public string PlatformsSerialized
        {
            get => JsonSerializer.Serialize(Platforms);
            set => Platforms = string.IsNullOrEmpty(value) ? new List<Platform>() : JsonSerializer.Deserialize<List<Platform>>(value);
        }

        [Display(Name = "Зображення (URL)")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Обкладинка (URL)")]
        public string CoverImageUrl { get; set; } = string.Empty;

        [Display(Name = "Жанр")]
        public int GenreId { get; set; }
        public Genre? Genre { get; set; }

        [Display(Name = "Це DLC?")]
        public bool IsDLC { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }
}