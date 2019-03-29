using System;
using System.Collections.Generic;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.BookARoom.Resources;
using HotelBot.Models.DTO;
using HotelBot.Models.Facebook;
using HotelBot.Shared.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace HotelBot.Dialogs.RoomDetail
{
    public class RoomDetailResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.SendImages, (context, data) =>
                      SendImages(context, data)
                },
                {
                    ResponseIds.SendGeneralDescription, (context, data) =>
                        SendGeneralDescription(context, data)
                }

            }

        };

        public static IMessageActivity SendImages(ITurnContext context, dynamic data)
        {
            var requestHandler = new RequestHandler();
            RoomDetailDto roomDetailDto = requestHandler.FetchRoomDetail(data).Result;
            var imageCards = new HeroCard[4];
            for (var i = 0; i < roomDetailDto.RoomImages.Count; i++)
                imageCards[i] = new HeroCard
                {
                    Title = "Room name",
                    Subtitle = string.Empty,
                    Text = string.Empty,
                    Images = new List<CardImage>
                    {
                        new CardImage(roomDetailDto.RoomImages[i].ImageUrl)
                    },
                    Tap = new CardAction {
                       Type = ActionTypes.ShowImage,
                       Value = roomDetailDto.RoomImages[i].ImageUrl
                    }
                };
            var reply = context.Activity.CreateReply();
            reply.Text = "Here are some more pictures of the room";
            var attachments = new List<Attachment>();
            foreach (var heroCard in imageCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = "carousel";
            reply.Attachments = attachments;
            return reply;
        }

        public static IMessageActivity SendGeneralDescription(ITurnContext context, dynamic data)
        {
            var requestHandler = new RequestHandler();
            RoomDetailDto roomDetailDto = requestHandler.FetchRoomDetail(data).Result;
            var message = "The hotel would describe this room as: " + roomDetailDto.Description;
            return MessageFactory.Text(message);
        }


        public RoomDetailResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            // all images? maybe room + bathroom separate?
            public const string SendImages = "SendImages";
            public const string SendGeneralDescription = "SendGeneralDescription";
        }
    }
}
