using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers
{
    public class CurrencyController : Controller
    {
        public IActionResult SetCurrency(string currency, string returnUrl)
        {
            HttpContext.Session.SetString("Currency", currency);
            return LocalRedirect(returnUrl);
        }
    }
}
