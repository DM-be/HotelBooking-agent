using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;

namespace HotelBot.Middleware
{


    public class AutoSaveMiddleware : IMiddleware
    {
        private readonly StateBotAccessors _accessors;

        public AutoSaveMiddleware(StateBotAccessors accessors)
        {
            _accessors = accessors;
        }
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _accessors.UserState.SaveChangesAsync(turnContext);
            await _accessors.ConversationState.SaveChangesAsync(turnContext);
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
