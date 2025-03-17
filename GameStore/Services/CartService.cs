using GameStore.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System;
using Microsoft.EntityFrameworkCore;
using GameStore.Data;

namespace GameStore.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        private const string CartSessionKey = "Cart";

        public CartService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Dictionary<int, int> GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                Console.WriteLine("Session is NULL!");
                return new Dictionary<int, int>();
            }

            var cartJson = session.GetString(CartSessionKey);
            Console.WriteLine("Session data: " + (cartJson ?? "NULL"));


            return cartJson != null
                ? JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson) ?? new Dictionary<int, int>()
                : new Dictionary<int, int>();
        }


        public void SaveCart(Dictionary<int, int> cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
            }
        }

        public bool IsCartEmpty()
        {
            return !GetCart().Any();
        }

        public OrderViewModel GetOrderViewModel()
        {
            var cart = GetCart();
            if (!cart.Any()) return new OrderViewModel { CartItems = new List<OrderItemViewModel>() };

            var productIds = cart.Keys.ToList();
            var products = _context.Products.Where(p => productIds.Contains(p.Id)).ToList();

            var cartItems = cart.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.Key);
                if (product == null) return null;

                decimal actualPrice = product.Discount > 0 ? product.DiscountedPrice : product.Price;

                return new OrderItemViewModel
                {
                    ProductId = product.Id,
                    ProductTitle = product.Title,
                    Quantity = item.Value,
                    TotalPrice = item.Value * actualPrice
                };
            }).Where(item => item != null).ToList();

            return new OrderViewModel { CartItems = cartItems };
        }



        public void ClearCart()
        {
            SaveCart(new Dictionary<int, int>());
        }
    }
}
