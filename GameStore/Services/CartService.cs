using GameStore.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System;
using Microsoft.EntityFrameworkCore;
using GameStore.Data;
using GameStore.Services.Interfaces;

namespace GameStore.Services
{
    public class CartService
    {
        private static CartService? _instance;
        private static readonly object _lock = new();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Func<ApplicationDbContext> _dbContextFactory; // Factory Scoped `ApplicationDbContext`
        private readonly Func<IPriceStrategy> _priceStrategyFactory; // Factory Scoped `IPriceStrategy`

        private const string CartSessionKey = "Cart";

        public CartService(IHttpContextAccessor httpContextAccessor, Func<IPriceStrategy> priceStrategyFactory, Func<ApplicationDbContext> dbContextFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _priceStrategyFactory = priceStrategyFactory;
            _dbContextFactory = dbContextFactory;
        }

        public static CartService GetInstance(IHttpContextAccessor httpContextAccessor, Func<IPriceStrategy> priceStrategyFactory, Func<ApplicationDbContext> dbContextFactory)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CartService(httpContextAccessor, priceStrategyFactory, dbContextFactory);
                    }
                }
            }
            return _instance;
        }

        public OrderViewModel GetOrderViewModel()
        {
            var cart = GetCart();
            if (!cart.Any()) return new OrderViewModel { CartItems = new List<OrderItemViewModel>() };

            using var dbContext = _dbContextFactory(); // Used new Scoped `ApplicationDbContext`
            var productIds = cart.Keys.ToList();
            var products = dbContext.Products.Where(p => productIds.Contains(p.Id)).ToList();

            var cartItems = cart.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.Key);
                if (product == null) return null;

                var priceStrategy = _priceStrategyFactory(); // Gotten new Scoped `IPriceStrategy`
                decimal actualPrice = priceStrategy.CalculatePrice(product);

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

        public void ClearCart()
        {
            SaveCart(new Dictionary<int, int>());
        }
    }
}
