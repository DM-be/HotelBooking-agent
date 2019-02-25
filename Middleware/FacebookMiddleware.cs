using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Models.Facebook;
using HotelBot.Shared.QuickReplies.Resources;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
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
            ProcessFacebookPayload(channelData, turnContext);
            await next(cancellationToken).ConfigureAwait(false);
        }

        private async void ProcessFacebookPayload(object channelData, ITurnContext context)
        {
            // At this point we know we are on Facebook channel, and can consume the Facebook custom payload
            // present in channelData.
            var facebookPayload = (channelData as JObject)?.ToObject<FacebookPayload>();

            if (facebookPayload != null)
            {
                // PostBack
                if (facebookPayload.PostBack != null)
                {
                    OnFacebookPostBack(facebookPayload.PostBack, context);
                }

                // Optin
                else if (facebookPayload.Optin != null)
                {
                    OnFacebookOptin(facebookPayload.Optin);
                }

                // Quick reply
                else if (facebookPayload.Message?.QuickReply != null)
                {
                    OnFacebookQuickReply(facebookPayload.Message.QuickReply);
                }

                else if (facebookPayload.Message.Attachments != null)
                {
                    var attachments = facebookPayload.Message.Attachments;
                    var latCoordinatesLat = attachments[0].FacebookPayload.Coordinates.Lat;
                    var longCoordinatesLong = attachments[0].FacebookPayload.Coordinates.Long;
                    var url = $"https://www.google.com/maps/dir/?api=1&origin={latCoordinatesLat},{longCoordinatesLong}&destination=51.228557,3.231737";
                    var heroCard = new HeroCard
                    {
                        Title = "Starhotel Bruges",
                        Text = "",
                        Images = new List<CardImage> { new CardImage("https://img.hotelspecials.be/fc2fadf52703ae0181b289f84011bf6a.jpeg?w=250&h=200&c=1&quality=70") },
                        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Show direction", value: url) },
                    };

                    var reply = context.Activity.CreateReply();
                    reply.Text = "Here it is";
                    reply.Attachments = new List<Attachment>
                    {
                        heroCard.ToAttachment(),
                    };
                    await context.SendActivityAsync(reply);
                }

                // TODO: Handle other events that you're interested in...
            }
        }

        private void OnFacebookOptin(FacebookOptin optin)
        {
            // TODO: Your optin event handling logic here...
        }

        private async void OnFacebookPostBack(FacebookPostback postBack, ITurnContext context)
        {


            Activity reply = context.Activity.CreateReply();
            var heroCard = new HeroCard
            {
                Title = "BotFramework Hero Card",
                Subtitle = "Microsoft Bot Framework",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
                      " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            //var heroCard2 = new HeroCard
            //{
            //    Title = "BotFramework Hero Card",
            //    Subtitle = "Microsoft Bot Framework",
            //    Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
            //           " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
            //    Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
            //    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            //};






            SendLocationQuickReply(context);

        }

        private void OnFacebookQuickReply(FacebookQuickReply quickReply)
        {
            // TODO: Your quick reply event handling logic here...
        }

        private async void SendLocationQuickReply(ITurnContext context)
        {
            Activity reply = context.Activity.CreateReply();
            reply.Text = QuickReplyStrings.ASK_LOCATION;
            var channelData = new JObject();
            var child = new JObject();
            child.Add("content_type", "location");
            channelData.Add("quick_replies", new JArray(child));
            reply.ChannelData = channelData;
            await context.SendActivityAsync(reply);
        }

    }
}
