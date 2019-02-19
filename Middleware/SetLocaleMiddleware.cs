using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace HotelBot.Middleware
{
    public class SetLocaleMiddleware : IMiddleware
    {
        private readonly string defaultLocale;

        public SetLocaleMiddleware(string defaultDefaultLocale)
        {
            defaultLocale = defaultDefaultLocale;
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var cultureInfo = context.Activity.Locale != null
                ? new CultureInfo(context.Activity.Locale)
                : new CultureInfo(defaultLocale);
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = cultureInfo;

            // calls next --> continues to bot
            // https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-middleware?view=azure-bot-service-4.0
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}