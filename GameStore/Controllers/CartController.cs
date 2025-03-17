using Microsoft.AspNetCore.Mvc;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;
using GameStore.Models;
using Microsoft.EntityFrameworkCore;
using GameStore.Data;

namespace GameStore.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;

        public CartController(CartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();

            var cartItems = cart.Select(item =>
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == item.Key);
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

            return View("Index", cartItems);
        }


        [Authorize]
        public IActionResult AddToCart(int productId)
        {
            var cart = _cartService.GetCart();
            if (cart.ContainsKey(productId))
                cart[productId]++;
            else
                cart[productId] = 1;

            _cartService.SaveCart(cart);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult RemoveOne(int productId)
        {
            var cart = _cartService.GetCart();
            if (cart.ContainsKey(productId))
            {
                cart[productId]--;
                if (cart[productId] <= 0) cart.Remove(productId);
            }
            _cartService.SaveCart(cart);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Remove(int productId)
        {
            var cart = _cartService.GetCart();
            cart.Remove(productId);
            _cartService.SaveCart(cart);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Clear()
        {
            _cartService.SaveCart(new Dictionary<int, int>());
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Checkout()
        {
            if (_cartService.IsCartEmpty())
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index", "Checkout");
        }
    }
}
