using System.Collections.Generic;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Prompts.Email.Resources;
using HotelBot.Models.Facebook;
using HotelBot.Shared.Helpers.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Prompts.LocationPrompt
{
    public class LocationResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    EmailResponses.ResponseIds.EmailPrompt, (context, data) =>
                        MessageFactory.Text(
                            EmailStrings.EMAIL_PROMPT,
                            EmailStrings.EMAIL_PROMPT,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.SendLocation, (context, data) =>
                        BuildLocationCard(context)

                },
                {
                    ResponseIds.SendNavigation, (context, data) =>
                        BuildNavigationCard(context, data)
                },
                {
                    ResponseIds.SendLocationQuickReply, (context, data) =>
                        BuildLocationQuickReply(context, data)
                }


            }
        };

        public LocationResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity BuildLocationQuickReply(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = FacebookStrings.QUICK_REPLY_ASK_LOCATION,
                QuickReplies = new []
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

        public static IMessageActivity BuildNavigationCard(ITurnContext context, FacebookPayloadCoordinates coordinates)
        {
            var latCoordinatesLat = coordinates.Lat;
            var longCoordinatesLong = coordinates.Long;
            var url = $"https://www.google.com/maps/dir/?api=1&origin={latCoordinatesLat},{longCoordinatesLong}&destination=51.228557,3.231737";
            var heroCard = new HeroCard
            {
                Title = "Starhotel Bruges",
                Subtitle = "Tijl Uilenspiegelstraat 9, 8000 Brugge",
                Images = new List<CardImage>
                {
                    new CardImage("https://img.hotelspecials.be/fc2fadf52703ae0181b289f84011bf6a.jpeg?w=250&h=200&c=1&quality=70")
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, FacebookStrings.HEROCARD_BUTTON_DIRECTION_TITLE, value: url)
                }
            };

            var reply = context.Activity.CreateReply();
            reply.Text = FacebookStrings.HEROCARD_REPLY_TEXT_DIRECTION;
            reply.Attachments = new List<Attachment>
            {
                heroCard.ToAttachment()
            };
            return reply;
        }


        public static IMessageActivity BuildLocationCard(ITurnContext context)
        {
            var url = "https://www.google.com/maps/dir/?api=1&destination=51.228557,3.231737";
            var heroCard = new HeroCard
            {
                Title = "Starhotel Bruges",
                Subtitle = "Tijl Uilenspiegelstraat 9, 8000 Brugge",
                Images = new List<CardImage>
                {
                    new CardImage("https://img.hotelspecials.be/fc2fadf52703ae0181b289f84011bf6a.jpeg?w=250&h=200&c=1&quality=70")
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, FacebookStrings.HEROCARD_BUTTON_DIRECTION_TITLE, value: url)
                }
            };

            var reply = context.Activity.CreateReply();
            reply.Text = "Our adres is Tijl Uilenspiegelstraat 9, 8000 Brugge";
            reply.Attachments = new List<Attachment>
            {
                heroCard.ToAttachment()
            };
            return reply;
        }

        public class ResponseIds
        {
            public const string SendNavigation = "sendNavigation";
            public const string SendLocation = "sendLocation";
            public const string SendLocationQuickReply = "sendLocationQuickReply";
        }
    }
}
