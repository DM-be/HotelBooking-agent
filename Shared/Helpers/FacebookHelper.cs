using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace HotelBot.Shared.Helpers
{
    public class FacebookHelper
    {
        private readonly FacebookHelperResponses _responder = new FacebookHelperResponses();


        public async Task SendGettingStartedQuickReplies(ITurnContext context)
        {

            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.SendGetStartedQuickReplies);
        }

        public async Task SendLocationQuickReply(ITurnContext context)
        {
            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.SendLocationQuickReply);
        }

        public async Task SendEmailQuickReply(ITurnContext context)
        {
            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.SendEmailQuickReply);
        }

        public async Task SendPhoneNumberQuickReply(ITurnContext context)
        {
            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.SendPhoneNumberQuickReply);
        }


        public async Task SendWelcomeMessage(ITurnContext context)
        {
            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.Welcome);
        }

        public async Task SendFunctionalityMessage(ITurnContext context)
        {
            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.Functionality);
        }

        public async Task SendDirections(ITurnContext context, dynamic data)
        {

            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.SendDirections, data);

        }

        public async Task SendDirectionsWithoutOrigin(ITurnContext context, dynamic data)
        {

            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.SendDirectionsWithoutOrigin, data);

        }

        public async Task SendCallMessage(ITurnContext context)
        {

            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.CallUs);

        }
    }
}
