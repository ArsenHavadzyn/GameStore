using Microsoft.AspNetCore.Mvc;
using GameStore.Services;
using Microsoft.AspNetCore.Authorization;

namespace GameStore.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [Authorize]
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
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
