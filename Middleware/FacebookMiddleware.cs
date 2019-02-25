using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using HotelBot.Models.Facebook;
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
                    OnFacebookQuickReply(facebookPayload.Message.QuickReply, context);

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
            if (postBack.Payload.Equals(FacebookPostback.GetStartedPostback)) SendGettingStartedWelcomeMessage(context);

            // implement other postback logic before calling next
            // also possible to update text of the activity.message.text property

        }

        private void OnFacebookQuickReply(FacebookQuickReply quickReply, ITurnContext context)
        {
            if (quickReply.Payload.Equals(FacebookQuickReply.LocationQuickReplyPayload))
            {
                SendLocationQuickReply(context);
            }

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
            var welcomeReply = context.Activity.CreateReply();
            welcomeReply.Text = WelcomeStrings.WELCOME_MESSAGE;
            var functionalityReply = context.Activity.CreateReply();
            functionalityReply.Text = WelcomeStrings.FUNCTIONALITY;
            IActivity[] activities =
            {
                welcomeReply,
                functionalityReply
            };
            await context.SendActivitiesAsync(activities);
            SendWelcomeQuickReplies(context);
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
        // TODO: add icons folder and add material icons
        private async void SendWelcomeQuickReplies(ITurnContext context)
        {
            var reply = context.Activity.CreateReply();
            FacebookQuickReply [] quick_replies =
            {
                new FacebookQuickReply
                {
                    Title = "Book a room",
                    Content_Type = "text",
                    Payload = "Book_a_room",
                    Image_Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3f/Button_Icon_Blue.svg/768px-Button_Icon_Blue.svg.png"
                },
                new FacebookQuickReply {
                    Title = "Get directions",
                    Content_Type = "text",
                    Payload = "location",
                    Image_Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3f/Button_Icon_Blue.svg/768px-Button_Icon_Blue.svg.png"
                },
                new FacebookQuickReply {
                    Title = "Call us",
                    Content_Type = "text",
                    Payload = "call",
                    Image_Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3f/Button_Icon_Blue.svg/768px-Button_Icon_Blue.svg.png"
                }
            };
            JObject[] jObjects =  new JObject[quick_replies.Length];
            for (int i = 0; i < quick_replies.Length; i++)
            {
                jObjects[i] = (JObject) JToken.FromObject(quick_replies[i]);
            }
            reply.Text = QuickReplyStrings.WELCOME_OPTIONS;
            var channelData = new JObject();
            channelData.Add("quick_replies", new JArray(jObjects));
            reply.ChannelData = channelData;
            await context.SendActivityAsync(reply);

        }
    }
}
