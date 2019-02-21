using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;


namespace HotelBot.Middleware
{
    public class SetUserProfileMiddleware: IMiddleware
    {
        private readonly StateBotAccessors _accessors;
        public SetUserProfileMiddleware(StateBotAccessors accessors)
        {
            _accessors = accessors;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                if (turnContext.Activity.ChannelId == "facebook")
                {
                    // TODO refactor in separate service and check expirations
                    var page_access_token =
                        "EAAZAwdCH6kA4BALmrXAxNUYIUfqaH01Lx3bsNDFlQZCgRolXq0yErVZABhdRHuZCkGjTuHGZCZBzhcWDCRVGOIXf6v5Yckz0MidVUJg8EXbWWXeaDhaGVR19CXtZBao64Y09N5IWWZBNMbQOa23Dt222YW8NwgdpNVWZBDjVOh5qXZCAZDZD";
                    var userId = turnContext.Activity.From.Id;
                    userProfile = await GetUserProfileBasedOnFacebookData(userId, page_access_token);
                    userProfile.Locale = userProfile.Locale.Replace("_", "-");
                    await _accessors.UserProfileAccessor.SetAsync(turnContext, userProfile);
                    await next(cancellationToken).ConfigureAwait(false);
                    return;
                }
            }
            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task<UserProfile> GetUserProfileBasedOnFacebookData(string userId, string page_access_token)
        {
            // https://developers.facebook.com/docs/messenger-platform/identity/user-profile/
            UserProfile userProfile = null;
            var client = new HttpClient();
            var path =
                $"https://graph.facebook.com/{userId}?fields=name,first_name,last_name,locale&access_token={page_access_token}";
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var userProfileAsAString = await response.Content.ReadAsStringAsync();
                userProfile = JsonConvert.DeserializeObject<UserProfile>(userProfileAsAString);
            }

            return userProfile;
        }
    }
}
