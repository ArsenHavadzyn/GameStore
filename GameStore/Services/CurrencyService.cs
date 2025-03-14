namespace GameStore.Services
{
    public class CurrencyService
    {
        private static readonly Dictionary<string, decimal> ExchangeRates = new()
        {
            { "USD", 1m },
            { "EUR", 0.9m },
            { "UAH", 40m }
        };

        public decimal ConvertPrice(decimal price, string currency)
        {
            return ExchangeRates.TryGetValue(currency, out var rate) ? price * rate : price;
        }
    }

}
