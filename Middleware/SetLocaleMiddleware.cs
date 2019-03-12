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
                // TODO: check if not already solved by correct order
                // still needs to be set by the SetUserProfileMiddleware
                await next(cancellationToken).ConfigureAwait(false);
                return;
            }
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(userProfile.Locale);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(userProfile.Locale);
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}