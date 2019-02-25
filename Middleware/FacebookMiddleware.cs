using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using HotelBot.Models.Facebook;
using HotelBot.Shared.QuickReplies.Resources;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace HotelBot.Middleware
{
    public class FacebookMiddleware : IMiddleware
    {

        
        private readonly StateBotAccessors _accessors;

        public FacebookMiddleware(StateBotAccessors accessors)
        {
            _accessors = accessors;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var channelData = turnContext.Activity.ChannelData;
            await ProcessFacebookPayload(channelData, turnContext);
            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessFacebookPayload(object channelData, ITurnContext context)
        {
            // At this point we know we are on Facebook channel, and can consume the Facebook custom payload
            // present in channelData.
            var facebookPayload = (channelData as JObject)?.ToObject<FacebookPayload>();

            if (facebookPayload != null)
            {
                // PostBack
                if (facebookPayload.PostBack != null)
                    OnFacebookPostBack(facebookPayload.PostBack, context);

                // Optin
                else if (facebookPayload.Optin != null)
                    OnFacebookOptin(facebookPayload.Optin);

                // Quick reply
                else if (facebookPayload.Message?.QuickReply != null)
                    OnFacebookQuickReply(facebookPayload.Message.QuickReply);

                else if (facebookPayload.Message.Attachments != null) OnFacebookAttachments(facebookPayload.Message.Attachments, context);
            }
        }

        private void OnFacebookOptin(FacebookOptin optin)
        {
        }

        private void OnFacebookAttachments( FacebookAttachment [] attachments, ITurnContext context)
        {
            foreach (var attachment in attachments)
                if (attachment.Type.Equals(FacebookAttachment.Location)) SendDirections(context, attachment);
        }

        private void OnFacebookPostBack(FacebookPostback postBack, ITurnContext context)
        {
            if (postBack.Payload.Equals(FacebookPostback.GetStartedPostback)) SendLocationQuickReply(context);

            // implement other postback logic before calling next
            // also possible to update text of the activity.message.text property

        }

        private void OnFacebookQuickReply(FacebookQuickReply quickReply)
        {

            // TODO: Your quick reply event handling logic here...
        }

        private async void SendLocationQuickReply(ITurnContext context)
        {
            var reply = context.Activity.CreateReply();
            reply.Text = QuickReplyStrings.ASK_LOCATION;
            var channelData = new JObject();
            var child = new JObject();
            child.Add("content_type", "location");
            channelData.Add("quick_replies", new JArray(child));
            reply.ChannelData = channelData;
            await context.SendActivityAsync(reply);
        }

        private async void SendGettingStartedWelcomeMessage(ITurnContext context)
        {
            
        }

        private async void SendDirections(ITurnContext context, FacebookAttachment attachment)
        {

            var latCoordinatesLat = attachment.FacebookPayload.Coordinates.Lat;
            var longCoordinatesLong = attachment.FacebookPayload.Coordinates.Long;
            var url = $"https://www.google.com/maps/dir/?api=1&origin={latCoordinatesLat},{longCoordinatesLong}&destination=51.228557,3.231737";
            var heroCard = new HeroCard
            {
                Title = "Starhotel Bruges", // TODO: get from resources (hotel page property)
                Images = new List<CardImage> { new CardImage("https://img.hotelspecials.be/fc2fadf52703ae0181b289f84011bf6a.jpeg?w=250&h=200&c=1&quality=70") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Show direction", value: url) },
            };

            var reply = context.Activity.CreateReply();
            reply.Text = QuickReplyStrings.DIRECTION_REPLY;
            reply.Attachments = new List<Attachment>
            {
                heroCard.ToAttachment(),
            };
            await context.SendActivityAsync(reply);
        }
    }
}
