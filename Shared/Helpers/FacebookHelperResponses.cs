using HotelBot.Models.Facebook;
using HotelBot.Shared.Helpers.Resources;
using HotelBot.Shared.QuickReplies.Resources;
using HotelBot.Shared.Welcome.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace HotelBot.Shared.Helpers
{
    public class FacebookHelperResponses : TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.SendLocationQuickReply,
                    (context, data) =>
                        BuildLocationQuickReply(context, data)
                },
                {
                    ResponseIds.SendGetStartedQuickReplies,
                    (context, data) =>
                        BuildGettingStartedQuickReplies(context, data)
                },
                {
                    ResponseIds.SendDirections,
                    (context, data) =>
                        BuildDirectionsCard(context, data)
                },
                {
                    ResponseIds.CallUs,
                    (context, data) =>
                        BuildCallMessage(context, data)
                },
                {
                    ResponseIds.Welcome,
                    (context, data) =>
                        MessageFactory.Text(
                            WelcomeStrings.WELCOME_MESSAGE,
                            WelcomeStrings.WELCOME_MESSAGE,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.Functionality,
                    (context, data) =>
                        MessageFactory.Text(
                            WelcomeStrings.FUNCTIONALITY,
                            WelcomeStrings.FUNCTIONALITY,
                            InputHints.AcceptingInput)
                }
            }
        };

        public FacebookHelperResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity BuildLocationQuickReply(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = FacebookStrings.QUICK_REPLY_ASK_LOCATION,
                QuickReplies = new[]
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "location"
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public static IMessageActivity BuildGettingStartedQuickReplies(ITurnContext context, dynamic data)
        {
            var reply = context.Activity.CreateReply();
            FacebookQuickReply[] quick_replies =
            {
                new FacebookQuickReply
                {
                    Title = FacebookStrings.QUICK_REPLY_BUTTON_BOOK_A_ROOM,
                    Content_Type = "text",
                    Payload = "book"
                },
                new FacebookQuickReply
                {
                    Title = FacebookStrings.QUICK_REPLY_BUTTON_DIRECTION,
                    Content_Type = "text",
                    Payload = "location"
                },
                new FacebookQuickReply
                {
                    Title = FacebookStrings.QUICK_REPLY_BUTTON_CALL,
                    Content_Type = "text",
                    Payload = "call"
                }
            };
            var facebookMessage = new FacebookMessage
            {
                Text = QuickReplyStrings.WELCOME_OPTIONS
            };

            facebookMessage.QuickReplies = quick_replies;
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public static IMessageActivity BuildCallMessage(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Attachment = new FacebookAttachment
                {
                    Type = "template",
                    FacebookPayload = new FacebookPayload
                    {
                        Template_Type = "button",
                        Text = FacebookStrings.CALL_MESSAGE_PAYLOAD_TEXT,
                        FacebookButtons = new[]
                        {
                            new FacebookButton
                            {
                                Type = "phone_number",
                                Title = FacebookStrings.BUTTON_TITLE_CALL,
                                Payload = "+15105551234"
                            },
                        }
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public static IMessageActivity BuildDirectionsCard(ITurnContext context, FacebookAttachment attachment)
        {
            var latCoordinatesLat = attachment.FacebookPayload.Coordinates.Lat;
            var longCoordinatesLong = attachment.FacebookPayload.Coordinates.Long;
            var url = $"https://www.google.com/maps/dir/?api=1&origin={latCoordinatesLat},{longCoordinatesLong}&destination=51.228557,3.231737";
            var heroCard = new HeroCard
            {
                Title = "Starhotel Bruges", // TODO: get from conversation state
                Images = new List<CardImage> { new CardImage("https://img.hotelspecials.be/fc2fadf52703ae0181b289f84011bf6a.jpeg?w=250&h=200&c=1&quality=70") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, FacebookStrings.HEROCARD_BUTTON_DIRECTION_TITLE, value: url) }
            };

            var reply = context.Activity.CreateReply();
            reply.Text = FacebookStrings.HEROCARD_REPLY_TEXT_DIRECTION;
            reply.Attachments = new List<Attachment>
            {
                heroCard.ToAttachment()
            };
            return reply;
        }

        public class ResponseIds
        {
            // Constants
            public const string SendGetStartedQuickReplies = "sendGetStartedQuickReplies";

            public const string SendLocationQuickReply = "sendLocationQuickReply";
            public const string Functionality = "functionality";
            public const string Welcome = "welcome";
            public const string SendDirections = "sendDirections";
            public const string CallUs = "callUs";
        }
    }
}