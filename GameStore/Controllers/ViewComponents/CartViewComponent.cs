using GameStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var cart = _cartService.GetCart();
            int itemCount = cart.Values.Sum();
            return View(itemCount);
        }
    }

}
