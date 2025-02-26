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


        public async Task<IActionResult> IndexAsync(string genre, string price, string publisher, string platform)
        {
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(genre))
            {
                products = products.Where(p => p.Genre.Name == genre);
            }

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

            if (!string.IsNullOrEmpty(publisher))
            {
                products = products.Where(p => p.Publisher == publisher);
            }

            if (!string.IsNullOrEmpty(platform) && Enum.TryParse(typeof(Platform), platform, out var platformEnum))
            {
                products = products.AsEnumerable()
                    .Where(p => JsonSerializer.Deserialize<List<Platform>>(p.PlatformsSerialized)?
                                    .Contains((Platform)platformEnum) ?? false)
                    .AsQueryable();
            }

            ViewData["Title"] = "Каталог ігор";
            ViewData["Message"] = "Найкращі відеоігри саме для вас!";

            ViewBag.Genres = _context.Genres.Select(g => g.Name).ToList();
            ViewBag.Publishers = _context.Products.Select(p => p.Publisher).Distinct().ToList();
            ViewBag.Platforms = _context.Products
                .AsEnumerable()
                .SelectMany(p => JsonSerializer.Deserialize<List<Platform>>(p.PlatformsSerialized) ?? new List<Platform>())
                .Distinct()
                .Select(p => p.ToString())
                .ToList();
            ViewBag.Collections = await _context.Collections.ToListAsync();

            return View(products.ToList());
        }


        public IActionResult Details(int id)
        {
            var product = _context.Products.Include(p => p.Genre).FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Collection(int id)
        {
            var collection = await _context.Collections
                .Include(c => c.Products)
                .ThenInclude(p => p.Genre)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection == null)
                return NotFound();

            ViewData["Title"] = $"{collection.Name}";
            ViewData["Message"] = $"{collection.Description}";

            ViewBag.Genres = _context.Genres.Select(g => g.Name).ToList();
            ViewBag.Publishers = _context.Products.Select(p => p.Publisher).Distinct().ToList();
            ViewBag.Platforms = _context.Products
                .AsEnumerable()
                .SelectMany(p => JsonSerializer.Deserialize<List<Platform>>(p.PlatformsSerialized) ?? new List<Platform>())
                .Distinct()
                .Select(p => p.ToString())
                .ToList();
            ViewBag.IsCollectionPage = true;

            return View("Index", collection.Products.ToList());
        }
    }
}
