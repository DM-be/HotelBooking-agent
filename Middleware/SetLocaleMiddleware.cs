using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;

namespace HotelBot.Middleware
{
    public class SetLocaleMiddleware : IMiddleware
    {
        private readonly StateBotAccessors _accessors;

        public SetLocaleMiddleware(StateBotAccessors accessors)
        {
            _accessors = accessors;
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(context, () => new UserProfile());
            if (string.IsNullOrWhiteSpace(userProfile.Locale))
            {
                // still needs to be set by the SetUserProfileMiddleware
                await next(cancellationToken).ConfigureAwait(false);
                return;
            }

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(userProfile.Locale);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(userProfile.Locale);
            // calls next --> continues to bot
            // https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-middleware?view=azure-bot-service-4.0
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}