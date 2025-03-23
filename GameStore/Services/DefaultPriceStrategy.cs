using GameStore.Models;
using GameStore.Services.Interfaces;

namespace GameStore.Services
{
    public class DefaultPriceStrategy : IPriceStrategy
    {
        public decimal CalculatePrice(Product product)
        {
            return product.Price;
        }
    }

}
