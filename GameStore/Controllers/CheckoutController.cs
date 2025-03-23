using Microsoft.AspNetCore.Mvc;
using GameStore.Models;
using GameStore.Services;
using GameStore.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Controllers
{
    namespace GameStore.Controllers
    {
        [Authorize]
        public class CheckoutController : Controller
        {
            private readonly CartService _cartService;
            private readonly ILogger<CheckoutController> _logger;
            private readonly OrderFacade _orderFacade; // added OrderFacade
            private readonly ApplicationDbContext _context;

            public CheckoutController(CartService cartService, ILogger<CheckoutController> logger, OrderFacade orderFacade, ApplicationDbContext context) // added OrderFacade
            {
                _cartService = cartService;
                _logger = logger;
                _orderFacade = orderFacade;
                _context = context;
            }

            [HttpGet]
            public IActionResult Index()
            {
                var orderViewModel = _cartService.GetOrderViewModel();

                Console.WriteLine("OrderViewModel in CheckoutController: " + JsonSerializer.Serialize(orderViewModel.CartItems));

                if (_cartService.IsCartEmpty())
                {
                    Console.WriteLine("Cart is empty! Redirecting to Cart...");
                    return RedirectToAction("Index", "Cart");
                }

                if (orderViewModel.CartItems == null || !orderViewModel.CartItems.Any())
                {
                    orderViewModel.CartItems = new List<OrderItemViewModel>();
                }

                _logger.LogInformation("Checkout page visited");
                return View(orderViewModel);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Checkout(OrderViewModel model)
            {
                _logger.LogInformation("Checkout initiated by user: {UserEmail}", model.Email);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Checkout failed due to invalid model state");
                    return View("Index", model);
                }

                try
                {
                    var order = _orderFacade.CreateOrder(model, User); // replaced _context with _orderFacade
                    return RedirectToAction("Confirmation", new { orderId = order.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Checkout process failed for user: {UserEmail}", model.Email);
                    ModelState.AddModelError("", "Сталася помилка під час оформлення замовлення.");
                    return View("Index", model);
                }
            }


            [HttpGet]
            public IActionResult Confirmation(int orderId)
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (order == null)
                {
                    return NotFound();
                }
                return View(order);
            }
        }
    }

}
