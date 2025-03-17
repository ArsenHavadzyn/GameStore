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
            private readonly ApplicationDbContext _context;
            private readonly IEmailService _emailService;
            private readonly ILogger<CheckoutController> _logger;

            public CheckoutController(CartService cartService, ApplicationDbContext context, IEmailService emailService, ILogger<CheckoutController> logger)
            {
                _cartService = cartService;
                _context = context;
                _emailService = emailService;
                _logger = logger;
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

                if (model.CartItems == null || !model.CartItems.Any())
                {
                    model.CartItems = _cartService.GetOrderViewModel().CartItems;

                    if (model.CartItems == null || !model.CartItems.Any())
                    {
                        _logger.LogWarning("Checkout failed due to invalid cart items");
                        return RedirectToAction("Index", "Cart");
                    }
                }



                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (userId == null)
                {
                    _logger.LogWarning("Checkout failed due to invalid user");
                    return RedirectToAction("Login", "Account");
                }

                string recipientEmail = model.UseCustomEmail ? model.Email : user?.Email;

                var order = new Order
                {
                    UserId = userId,
                    User = user,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalPrice = model.TotalAmount,
                    OrderItems = new List<OrderItem>(),
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    CustomEmail = model.UseCustomEmail ? model.Email : null
                };

                try
                {
                    foreach (var item in model.CartItems)
                    {
                        var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);

                        if (product != null)
                        {
                            decimal actualPrice = product.Discount > 0 ? product.DiscountedPrice : product.Price;

                            order.OrderItems.Add(new OrderItem
                            {
                                Order = order,
                                ProductId = product.Id,
                                Product = product,
                                ProductTitle = product.Title,
                                Quantity = item.Quantity,
                                Price = actualPrice,
                                TotalPrice = item.Quantity * product.Price
                            });
                        }
                    }

                    order.TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    _cartService.ClearCart();

                    order = _context.Orders.Include(o => o.User).FirstOrDefault(o => o.Id == order.Id);

                    if (!string.IsNullOrEmpty(recipientEmail))
                    {
                        SendOrderConfirmationEmail(order, recipientEmail, model);
                    }
                    else
                    {
                        Console.WriteLine("Email not sent - no recipient email found");
                    }
                    _logger.LogInformation("Order successfully created for user: {UserEmail}", model.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Checkout process failed for user: {UserEmail}", model.Email);
                    ModelState.AddModelError("", "Сталася помилка під час оформлення замовлення.");
                    return View("Index", model);
                }


                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }

            private void SendOrderConfirmationEmail(Order order, string recipientEmail, OrderViewModel model)
            {
                var orderItems = order.OrderItems.Select(oi => $"{oi.Quantity} x {oi.ProductTitle} - ${oi.Price:C}");
                var body = $@"<h1>Підтвердження замовлення</h1>
                    <p>Дякуємо за ваше замовлення!</p>
                    <p><strong>Номер замовлення:</strong> {order.Id}</p>
                    <p><strong>Дата:</strong> {order.OrderDate}</p>
                    <p><strong>Товари:</strong> {string.Join("<br>", orderItems)}</p>
                    <p><strong>Сума:</strong> ${order.TotalPrice}</p>
                    <p><strong>Статус:</strong> {order.Status}</p>
                    <p><strong>Ім'я:</strong> {model.FullName}</p>
                    <p><strong>Email:</strong> {model.Email}</p>
                    <p><strong>Телефон:</strong> {model.PhoneNumber}</p>
                    <p><strong>Адреса доставки:</strong> {model.Address}</p>
                    <p>Очікуйте листа з цифровим кодом продукту</p>";

                _emailService.SendEmailAsync(recipientEmail, "Підтвердження замовлення", body);
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
