using GameStore.Models;
using GameStore.Services.Interfaces;

namespace GameStore.Services
{
    public class DiscountPriceStrategy : IPriceStrategy
    {
        public decimal CalculatePrice(Product product)
        {
            return product.Discount > 0 ? product.DiscountedPrice : product.Price;
        }
    }

}
