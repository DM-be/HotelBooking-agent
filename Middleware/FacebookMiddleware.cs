using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using HotelBot.Models.Facebook;
using HotelBot.Shared.Helpers;
using HotelBot.Shared.QuickReplies.Resources;
using HotelBot.Shared.Welcome.Resources;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace HotelBot.Middleware
{
    public class FacebookMiddleware : IMiddleware
    {

        // TODO: remove accessors when not needed
        private readonly StateBotAccessors _accessors;
        private readonly FacebookHelper facebookHelper;

        public FacebookMiddleware(StateBotAccessors accessors)
        {
            _accessors = accessors;
            facebookHelper = new FacebookHelper();
        }
  

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var channelData = turnContext.Activity.ChannelData;
            await ProcessFacebookPayload(channelData, turnContext);
            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessFacebookPayload(object channelData, ITurnContext context)
        {
            var facebookPayload = (channelData as JObject)?.ToObject<FacebookPayload>();
            if (facebookPayload != null)
            {
                if (facebookPayload.PostBack != null)
                    OnFacebookPostBack(facebookPayload.PostBack, context);

                else if (facebookPayload.Optin != null)
                    OnFacebookOptin(facebookPayload.Optin);

                else if (facebookPayload.Message?.QuickReply != null)
                    OnFacebookQuickReply(facebookPayload.Message.QuickReply, context);

                else if (facebookPayload.Message.Attachments != null) OnFacebookAttachments(facebookPayload.Message.Attachments, context);
            }
        }

        private void OnFacebookOptin(FacebookOptin optin)
        {
        }

        private async void OnFacebookAttachments( FacebookAttachment [] attachments, ITurnContext context)
        {
            foreach (var attachment in attachments)
            {
                if (attachment.Type.Equals(FacebookAttachment.Location)) await facebookHelper.SendDirections(context, attachment);
            }
        }

        private async void OnFacebookPostBack(FacebookPostback postBack, ITurnContext context)
        {
            if (postBack.Payload.Equals(FacebookPostback.GetStartedPostback))
            {
                await facebookHelper.SendWelcomeMessage(context);
                await facebookHelper.SendFunctionalityMessage(context);
                await facebookHelper.SendGettingStartedQuickReplies(context);
            }

            // implement other postback logic before calling next
        }


        // TODO: think about translating middleware --> keep from translating twice in these postbacks
        private async void OnFacebookQuickReply(FacebookQuickReply quickReply, ITurnContext context)
        {
            if (quickReply.Payload.Equals(FacebookQuickReply.LocationQuickReplyPayload))
            {
                // update text --> let the maindialog handle sending quick replies
                context.Activity.Text = "Where are you located?";
            }
            else if (quickReply.Payload.Equals(FacebookQuickReply.CallUsReplyPayload))
            {
                // send english text so intent can be recognized
                
                context.Activity.Text = "Can I call you?";

            }

        }
    }
}
