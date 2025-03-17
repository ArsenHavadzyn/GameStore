using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameStore.Data;
using GameStore.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using GameStore.Services;

namespace GameStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
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

            foreach (var order in orders)
            {
                order.TotalPrice = order.OrderItems.Sum(oi =>
                    oi.Quantity * (oi.Product.Discount > 0 ? oi.Product.DiscountedPrice : oi.Product.Price));
            }

            ViewBag.Users = await _context.Users.Select(u => u.Email).ToListAsync();
            ViewBag.SelectedUser = userEmail;
            ViewBag.SelectedSort = sortOrder;

            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            order.TotalPrice = order.OrderItems.Sum(oi =>
                oi.Quantity * (oi.Product.Discount > 0 ? oi.Product.DiscountedPrice : oi.Product.Price));

            return View(order);
        }



        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            if (status == "Shipped")
            {
                order.DigitalCode = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(order.UserId);
                string recipientEmail = !string.IsNullOrEmpty(order.CustomEmail) ? order.CustomEmail : user?.Email;

                if (string.IsNullOrEmpty(recipientEmail))
                {
                    Console.WriteLine("Помилка: Немає email для надсилання.");
                    return NotFound();
                }

                var orderItems = order.OrderItems.Select(oi =>
                    $"{oi.Quantity} x {oi.Product.Title} - ${oi.Quantity * (oi.Product.Discount > 0 ? oi.Product.DiscountedPrice : oi.Product.Price)}");

                var body = $@"<h1>Деталі замовлення</h1>
        <p>Дякуємо за ваше замовлення!</p>
        <p><strong>Номер замовлення:</strong> {order.Id}</p>
        <p><strong>Дата:</strong> {order.OrderDate}</p>
        <p><strong>Товари:</strong> {string.Join("<br>", orderItems)}</p>
        <p><strong>Сума:</strong> ${order.OrderItems.Sum(oi => oi.Quantity * (oi.Product.Discount > 0 ? oi.Product.DiscountedPrice : oi.Product.Price))}</p>
        <p><strong>Статус:</strong> {order.Status}</p>
        <p><strong>Ваш цифровий ключ:</strong> {order.DigitalCode}</p>";

                await _emailService.SendEmailAsync(recipientEmail, "Деталі замовлення", body);
            }

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

            foreach (var order in orders)
            {
                order.TotalPrice = order.OrderItems.Sum(oi =>
                    oi.Quantity * (oi.Product.Discount > 0 ? oi.Product.DiscountedPrice : oi.Product.Price));
            }

            return View(orders);
        }

    }
}
