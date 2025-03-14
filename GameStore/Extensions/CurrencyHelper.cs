using GameStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameStore.Extensions
{
    public static class CurrencyHelper
    {
        public static string FormatPrice(this IHtmlHelper htmlHelper, decimal price)
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var currency = httpContext.Session.GetString("Currency") ?? "USD";

            var currencyService = httpContext.RequestServices.GetService<CurrencyService>();
            var convertedPrice = currencyService?.ConvertPrice(price, currency) ?? price;

            return $"{convertedPrice:0.00} {currency}";
        }
    }
}
