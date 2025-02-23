using System.ComponentModel.DataAnnotations;

namespace GameStore.Models
{
    public class Collection
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
