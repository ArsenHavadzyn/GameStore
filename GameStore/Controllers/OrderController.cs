using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameStore.Data;
using GameStore.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace GameStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string userEmail, string sortOrder)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userEmail) && userEmail != "All")
            {
                ordersQuery = ordersQuery.Where(o => o.User.Email == userEmail);
            }

            ordersQuery = sortOrder switch
            {
                "date_asc" => ordersQuery.OrderBy(o => o.OrderDate),
                "date_desc" => ordersQuery.OrderByDescending(o => o.OrderDate),
                "price_asc" => ordersQuery.OrderBy(o => o.TotalPrice),
                "price_desc" => ordersQuery.OrderByDescending(o => o.TotalPrice),
                "status" => ordersQuery.OrderBy(o => o.Status),
                _ => ordersQuery.OrderByDescending(o => o.OrderDate)
            };

            var orders = await ordersQuery.ToListAsync();
            ViewBag.Users = await _context.Users.Select(u => u.Email).ToListAsync();
            ViewBag.SelectedUser = userEmail;
            ViewBag.SelectedSort = sortOrder;

            return View(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            return View(orders);
        }
    }
}
