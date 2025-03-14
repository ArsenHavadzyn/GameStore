using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameStore.Data;
using GameStore.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace GameStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Genre);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
            ViewData["Platforms"] = Enum.GetValues(typeof(Platform))
                            .Cast<Platform>()
                            .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() })
                            .ToList();

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,ReleaseDate,Developer,Publisher,ImageUrl, CoverImageUrl, GenreId, IsDLC")] Product product, string[] Platforms)
        {
            if (ModelState.IsValid)
            {
                product.Platforms = Platforms?
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => Enum.Parse<Platform>(p))
                    .ToList() ?? new List<Platform>();

                product.PlatformsSerialized = JsonSerializer.Serialize(product.Platforms);

                _context.Add(product);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Platforms received in request: {Platforms?.Length}");

                if (Platforms != null)
                {
                    foreach (var platform in Platforms)
                    {
                        Console.WriteLine($"Platform: {platform}");
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name", product.GenreId);
            ViewBag.Platforms = Enum.GetValues(typeof(Platform))
                .Cast<Platform>()
                .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() })
                .ToList();

            return View(product);
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
            ViewBag.Platforms = Enum.GetValues(typeof(Platform))
                .Cast<Platform>()
                .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() })
                .ToList();

            if (!string.IsNullOrEmpty(product.PlatformsSerialized))
            {
                product.Platforms = JsonSerializer.Deserialize<List<Platform>>(product.PlatformsSerialized) ?? new List<Platform>();
            }
            else
            {
                product.Platforms = new List<Platform>();
            }


            return View(product);
        }


        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,ReleaseDate,Developer,Publisher,ImageUrl, CoverImageUrl, GenreId, IsDLC")] Product product, string[] Platforms)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Platforms != null && Platforms.Any())
                    {
                        product.Platforms = Platforms
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .Select(p =>
                            {
                                if (Enum.TryParse<Platform>(p.Trim(), out var platform))
                                {
                                    return platform;
                                }
                                return (Platform?)null;
                            })
                            .Where(p => p.HasValue)
                            .Select(p => p!.Value)
                            .ToList();
                    }
                    else
                    {
                        product.Platforms = new List<Platform>();
                    }

                    product.PlatformsSerialized = JsonSerializer.Serialize(product.Platforms);

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name", product.GenreId);
            ViewBag.Platforms = Enum.GetValues(typeof(Platform))
                .Cast<Platform>()
                .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() })
                .ToList();

            return View(product);
        }



        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
