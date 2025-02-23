using GameStore.Data;
using GameStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace GameStore.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index(string genre, string price, string publisher, string platform)
        {
            // Отримання списку ігор
            var products = _context.Products.AsQueryable();

            // Фільтрація за жанром
            if (!string.IsNullOrEmpty(genre))
            {
                products = products.Where(p => p.Genre.Name == genre);
            }

            // Фільтрація за ціною
            if (!string.IsNullOrEmpty(price))
            {
                var priceRange = price.Split('-');
                if (priceRange.Length == 2)
                {
                    var minPrice = int.Parse(priceRange[0]);
                    var maxPrice = int.Parse(priceRange[1]);
                    products = products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
                else if (price == "100+")
                {
                    products = products.Where(p => p.Price >= 100);
                }
            }

            // Фільтрація за видавцем
            if (!string.IsNullOrEmpty(publisher))
            {
                products = products.Where(p => p.Publisher == publisher);
            }

            // Фільтрація за платформою
            if (!string.IsNullOrEmpty(platform) && Enum.TryParse(typeof(Platform), platform, out var platformEnum))
            {
                products = products.AsEnumerable()
                    .Where(p => JsonSerializer.Deserialize<List<Platform>>(p.PlatformsSerialized)?
                                    .Contains((Platform)platformEnum) ?? false)
                    .AsQueryable();
            }




            // Отримання списку жанрів, видавців і платформ для фільтрів
            ViewBag.Genres = _context.Genres.Select(g => g.Name).ToList();
            ViewBag.Publishers = _context.Products.Select(p => p.Publisher).Distinct().ToList();
            ViewBag.Platforms = _context.Products
                .AsEnumerable()
                .SelectMany(p => JsonSerializer.Deserialize<List<Platform>>(p.PlatformsSerialized) ?? new List<Platform>())
                .Distinct()
                .Select(p => p.ToString())
                .ToList();

            return View(products.ToList());
        }


        //[HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            var product = _context.Products.Include(p => p.Genre).FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }

    }
}
