using System.Collections.Generic;
using HotelBot.Dialogs.Prompts.LocationPrompt.Resources;
using HotelBot.Models.Facebook;
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
                Text = LocationStrings.QUICK_REPLY_ASK_LOCATION,
                QuickReplies = new List<FacebookQuickReply>
                {
                    new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Location
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
                    new CardImage("http://www.hoteldepauw.be/hoteldepauw/assets/pirate/images/cover/cover.jpg")
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, LocationStrings.HEROCARD_BUTTON_DIRECTION_TITLE, value: url)
                }
            };

            var reply = context.Activity.CreateReply();
            reply.Text = LocationStrings.HEROCARD_REPLY_TEXT_DIRECTION;
            reply.Attachments = new List<Attachment>
            {
                heroCard.ToAttachment()
            };
            return reply;
        }



        public class ResponseIds
        {
            public const string SendNavigation = "sendNavigation";
            public const string SendLocationQuickReply = "sendLocationQuickReply";
        }
    }
}
