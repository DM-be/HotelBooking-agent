using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Shared.Welcome.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace HotelBot.Shared.Helpers
{
    public  class FacebookHelper
    {
        private  FacebookHelperResponses _responder = new FacebookHelperResponses();


        public async Task SendGettingStartedQuickReplies(ITurnContext context)
        {

            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.SendGetStartedQuickReplies);
        }

        public async Task SendLocationQuickReply(ITurnContext context)
        {
            await _responder.ReplyWith(context, FacebookHelperResponses.ResponseIds.SendLocationQuickReply);
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

        


    }
}
