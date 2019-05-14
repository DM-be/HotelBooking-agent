using System.Threading;
using System.Threading.Tasks;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;


namespace HotelBot.Middleware
{
    public class SetConversationDataMiddleware: IMiddleware
    {
        private readonly StateBotAccessors _accessors;
        public SetConversationDataMiddleware(StateBotAccessors accessors)
        {
            _accessors = accessors;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var conversationData = await _accessors.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());
            if (string.IsNullOrEmpty(conversationData.FacebookPageId))
            {
                conversationData.FacebookPageId = turnContext.Activity.Recipient.Id;
                await _accessors.ConversationDataAccessor.SetAsync(turnContext, conversationData, cancellationToken);
                await next(cancellationToken).ConfigureAwait(false);
                return;
            }
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
