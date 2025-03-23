using GameStore.Models;
using GameStore.Services.Interfaces;

namespace GameStore.Services
{
    public class VipPriceStrategy : IPriceStrategy
    {
        public decimal CalculatePrice(Product product)
        {
            decimal basePrice = product.Discount > 0 ? product.DiscountedPrice : product.Price;
            return basePrice * 0.9m;
        }
    }

}
