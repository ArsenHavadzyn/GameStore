namespace GameStore.Middleware
{
    public class CurrencyMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrencyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Session.GetString("Currency") == null)
            {
                context.Session.SetString("Currency", "USD");
            }
            await _next(context);
        }
    }

}
