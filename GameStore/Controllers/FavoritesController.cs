using GameStore.Data;
using GameStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var favorites = _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .ToList();

            return View(favorites);
        }

        public async Task<IActionResult> AddToFavorites(int productId)
        {
            var userId = _userManager.GetUserId(User);
            if (!_context.Favorites.Any(f => f.UserId == userId && f.ProductId == productId))
            {
                _context.Favorites.Add(new Favorite { UserId = userId, ProductId = productId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveFromFavorites(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var favorite = _context.Favorites.FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }

}
