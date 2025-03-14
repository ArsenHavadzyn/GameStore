using Microsoft.AspNetCore.Mvc;
using GameStore.Models;
using GameStore.Services;
using GameStore.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace GameStore.Controllers
{
    namespace GameStore.Controllers
    {
        [Authorize]
        public class CheckoutController : Controller
        {
            private readonly CartService _cartService;
            private readonly ApplicationDbContext _context;

            public CheckoutController(CartService cartService, ApplicationDbContext context)
            {
                _cartService = cartService;
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

                return View(orderViewModel);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Checkout(OrderViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                if (model.CartItems == null || !model.CartItems.Any())
                {
                    Console.WriteLine("CartItems is empty in Checkout!");
                    return View("Index", model);
                }


                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalPrice = model.TotalAmount,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var item in model.CartItems)
                {
                    var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);

                    try
                    {
                        product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }

                    if (product != null)
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            Order = order,
                            ProductId = product.Id,
                            Product = product,
                            ProductTitle = product.Title,
                            Quantity = item.Quantity,
                            Price = product.Price,
                            TotalPrice = item.Quantity * product.Price
                        });
                    }
                }


                order.TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);


                _context.Orders.Add(order);
                foreach (var item in order.OrderItems)
                {
                    Console.WriteLine($"OrderItem: ProductId = {item.ProductId}, Quantity = {item.Quantity}, Price = {item.Price}");
                }


                _context.OrderItems.AddRange(order.OrderItems);
                _context.SaveChanges();

                _cartService.ClearCart();


                return RedirectToAction("Confirmation", new { orderId = order.Id });
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
