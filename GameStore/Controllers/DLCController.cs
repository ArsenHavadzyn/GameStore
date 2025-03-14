using Microsoft.AspNetCore.Mvc;
using GameStore.Data;
using GameStore.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GameStore.Controllers
{
    public class DLCController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DLCController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchQuery, string genre, string price, string publisher, string platform, string sortOrder)
        {
            var dlcProducts = _context.Products.Include(p => p.Genre).Where(p => p.IsDLC == true).AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                dlcProducts = dlcProducts.Where(p => p.Title.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(genre))
            {
                dlcProducts = dlcProducts.Where(p => p.Genre.Name == genre);
            }

            if (!string.IsNullOrEmpty(price))
            {
                var priceRange = price.Split('-');
                if (priceRange.Length == 2)
                {
                    var minPrice = int.Parse(priceRange[0]);
                    var maxPrice = int.Parse(priceRange[1]);
                    dlcProducts = dlcProducts.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
                else if (price == "100+")
                {
                    dlcProducts = dlcProducts.Where(p => p.Price >= 100);
                }
            }

            if (!string.IsNullOrEmpty(publisher))
            {
                dlcProducts = dlcProducts.Where(p => p.Publisher == publisher);
            }

            if (!string.IsNullOrEmpty(platform))
            {
                dlcProducts = dlcProducts.Where(p => p.PlatformsSerialized.Contains(platform));
            }

            dlcProducts = sortOrder switch
            {
                "title_asc" => dlcProducts.OrderBy(p => p.Title),
                "title_desc" => dlcProducts.OrderByDescending(p => p.Title),
                "price_asc" => dlcProducts.OrderBy(p => p.Price),
                "price_desc" => dlcProducts.OrderByDescending(p => p.Price),
                "date_asc" => dlcProducts.OrderBy(p => p.ReleaseDate),
                "date_desc" => dlcProducts.OrderByDescending(p => p.ReleaseDate),
                _ => dlcProducts
            };

            ViewBag.Genres = _context.Genres.Select(g => g.Name).ToList();
            ViewBag.Publishers = _context.Products.Select(p => p.Publisher).Distinct().ToList();
            ViewBag.Platforms = _context.Products
                .AsEnumerable()
                .SelectMany(p => JsonSerializer.Deserialize<List<Platform>>(p.PlatformsSerialized) ?? new List<Platform>())
                .Distinct()
                .Select(p => p.ToString())
                .ToList();

            return View(await dlcProducts.ToListAsync());
        }
    }
}
