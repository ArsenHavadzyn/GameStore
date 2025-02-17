using GameStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GameStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Developer { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;

        [NotMapped] // Не зберігається безпосередньо в БД
        public List<Platform> Platforms { get; set; } = new List<Platform>();

        // Серіалізація у JSON для збереження в БД
        [Column("Platforms")]
        public string PlatformsSerialized
        {
            get => JsonSerializer.Serialize(Platforms);
            set => Platforms = string.IsNullOrEmpty(value) ? new List<Platform>() : JsonSerializer.Deserialize<List<Platform>>(value);
        }
        public string ImageUrl { get; set; } = string.Empty;

        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;
    }
}