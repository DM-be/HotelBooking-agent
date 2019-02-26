﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.Main.Resources;
using HotelBot.Models.Facebook;
using HotelBot.Shared.QuickReplies.Resources;
using HotelBot.Shared.Welcome.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace HotelBot.Shared.Helpers
{
    public class FacebookHelperResponses: TemplateManager
    {
        private static LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            // todo: implement basic welcoming based on postback (quick replies)
            ["default"] = new TemplateIdMap
            {
                { ResponseIds.SendLocationQuickReply,
                    (context, data) =>
                        BuildLocationQuickReply(context, data)
                },
                { ResponseIds.SendGetStartedQuickReplies,
                    (context, data) =>
                        BuildGettingStartedQuickReplies(context, data)
                },
                { ResponseIds.SendDirections,
                    (context, data) =>
                        BuildDirectionsCard(context, data)
                },
                { ResponseIds.CallUs,
                    (context, data) =>
                        BuildCallMessage(context, data)
                },
                { ResponseIds.Welcome,
                    (context, data) =>
                        MessageFactory.Text(
                            text: WelcomeStrings.WELCOME_MESSAGE,
                            ssml: WelcomeStrings.WELCOME_MESSAGE,
                            inputHint: InputHints.AcceptingInput)
                },
                { ResponseIds.Functionality,
                    (context, data) =>
                        MessageFactory.Text(
                            text: WelcomeStrings.FUNCTIONALITY,
                            ssml: WelcomeStrings.FUNCTIONALITY,
                            inputHint: InputHints.AcceptingInput)
                }

            }
        };

        public FacebookHelperResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity BuildLocationQuickReply(ITurnContext context, dynamic data)
        {
            var reply = context.Activity.CreateReply();
            reply.Text = QuickReplyStrings.ASK_LOCATION;
            var channelData = new JObject();
            var child = new JObject
            {
                { "content_type", "location" }
            };
            channelData.Add("quick_replies", new JArray(child));
            reply.ChannelData = channelData;
            return reply;
        }

        public static IMessageActivity BuildGettingStartedQuickReplies(ITurnContext context, dynamic data)
        {
            var reply = context.Activity.CreateReply();
            FacebookQuickReply[] quick_replies =
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
            JObject[] jObjects = new JObject[quick_replies.Length];
            for (int i = 0; i < quick_replies.Length; i++)
            {
                jObjects[i] = (JObject)JToken.FromObject(quick_replies[i]);
            }
            reply.Text = QuickReplyStrings.WELCOME_OPTIONS;
            var channelData = new JObject();
            channelData.Add("quick_replies", new JArray(jObjects));
            reply.ChannelData = channelData;
            return reply;
        }

        public static IMessageActivity BuildCallMessage(ITurnContext context, dynamic data)
        {
            //TODO: refactor + backend get number
           var facebookMessage = new FacebookMessage();
           var facebookAttachment = new FacebookAttachment();
           facebookAttachment.Type = "template";
           var payload =  new FacebookPayload();
           payload.Template_Type = "button";
           payload.Text = "Need more help? here is our number";
           var button = new FacebookButton();
           button.Type = "phone_number";
           button.Title = "Call us";
           button.Payload = "+15105551234";
           payload.FacebookButtons = new FacebookButton[1];
           payload.FacebookButtons[0] = button;
           facebookAttachment.FacebookPayload = payload;
           facebookMessage.Attachment = facebookAttachment;
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