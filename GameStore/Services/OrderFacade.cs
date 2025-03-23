using GameStore.Data;
using GameStore.Models;
using GameStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace GameStore.Services
{
    public class OrderFacade
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly IPriceStrategy _priceStrategy; // added Price strategy
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderFacade> _logger;


        public OrderFacade(ApplicationDbContext context, CartService cartService, IPriceStrategy priceStrategy, IEmailService emailService, ILogger<OrderFacade> logger) // added Price strategy
        {
            _context = context;
            _cartService = cartService;
            _priceStrategy = priceStrategy;
            _emailService = emailService;
            _logger = logger;
        }

        public Order CreateOrder(OrderViewModel model, ClaimsPrincipal userClaims)
        {
            var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (userId == null)
            {
                _logger.LogWarning("Checkout failed due to invalid user");
                throw new InvalidOperationException("User not found");
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
                        decimal actualPrice = _priceStrategy.CalculatePrice(product); // replaced product.Price with _priceStrategy.CalculatePrice(product)

                        order.OrderItems.Add(new OrderItem
                        {
                            Order = order,
                            ProductId = product.Id,
                            Product = product,
                            ProductTitle = product.Title,
                            Quantity = item.Quantity,
                            Price = actualPrice,
                            TotalPrice = item.Quantity * actualPrice
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
                    _logger.LogWarning("Email not sent - no recipient email found");
                }

                _logger.LogInformation("Order successfully created for user: {UserEmail}", model.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout process failed for user: {UserEmail}", model.Email);
                throw;
            }

            return order;
        }

        private void SendOrderConfirmationEmail(Order order, string recipientEmail, OrderViewModel model)
        {
            var orderItems = order.OrderItems.Select(oi => $"{oi.Quantity} x {oi.Product.Title} - ${oi.TotalPrice}");
            var body = $@"
            <h1>Підтвердження замовлення</h1>
            <p>Дякуємо за ваше замовлення!</p>
            <p><strong>Номер замовлення:</strong> {order.Id}</p>
            <p><strong>Дата:</strong> {order.OrderDate}</p>
            <p><strong>Товари:</strong> {string.Join("<br>", orderItems)}</p>
            <p><strong>Сума:</strong> ${order.TotalPrice}</p>
            <p><strong>Адреса доставки:</strong> {order.Address}</p>
            <p><strong>Телефон:</strong> {order.PhoneNumber}</p>
            <p><strong>Статус:</strong> {order.Status}</p>";

            _emailService.SendEmailAsync(recipientEmail, "Підтвердження замовлення", body);
        }
    }

}
