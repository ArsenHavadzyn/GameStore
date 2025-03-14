using GameStore.Data;
using GameStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GameStore.Controllers
{
    public class CollectionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CollectionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var collections = await _context.Collections.Include(c => c.Products).ToListAsync();
            return View(collections);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.AllGames = await _context.Products.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Collection collection, int[] selectedGames)
        {
            if (ModelState.IsValid)
            {
                collection.Products = new List<Product>();

                if (selectedGames != null)
                {
                    var selectedProducts = await _context.Products
                        .Where(p => selectedGames.Contains(p.Id))
                        .ToListAsync();
                    collection.Products.AddRange(selectedProducts);
                }

                _context.Collections.Add(collection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AllGames = await _context.Products
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Title })
                .ToListAsync();

            return View(collection);
        }


        // GET: Collection/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var collection = await _context.Collections
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection == null) return NotFound();

            var allGames = await _context.Products.ToListAsync();

            ViewBag.AllGames = allGames.Select(game => new SelectListItem
            {
                Value = game.Id.ToString(),
                Text = game.Title,
                Selected = collection.Products.Any(p => p.Id == game.Id)
            }).ToList();

            return View(collection);
        }


        // POST: Collection/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Collection collection, string[] selectedGames)
        {
            if (id != collection.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var selectedGameIds = selectedGames.Select(int.Parse).ToList();
                    collection.Products = await _context.Products
                        .Where(p => selectedGameIds.Contains(p.Id))
                        .ToListAsync();

                    _context.Update(collection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Collections.Any(c => c.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AllGames = _context.Products.Select(p => new { p.Id, p.Title }).ToList();
            return View(collection);
        }


        // GET: Collection/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collection = await _context.Collections
                .FirstOrDefaultAsync(m => m.Id == id);
            if (collection == null)
            {
                return NotFound();
            }

            return View(collection);
        }

        // POST: Collection/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var collection = await _context.Collections
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection != null)
            {
                collection.Products.Clear();
                await _context.SaveChangesAsync();

                _context.Collections.Remove(collection);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var collection = await _context.Collections
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collection == null) return NotFound();

            return View(collection);
        }

    }
}
