using GameStore.Models;

namespace GameStore.Services.Interfaces
{
    public interface IPriceStrategy
    {
        decimal CalculatePrice(Product product);
    }
}
