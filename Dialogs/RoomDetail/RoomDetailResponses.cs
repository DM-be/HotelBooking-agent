using System.Collections.Generic;
using HotelBot.Models.DTO;
using HotelBot.Shared.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

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


        public RoomDetailResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity SendImages(ITurnContext context, dynamic data)
        {
            var roomDetailDto = data as RoomDetailDto;
            var imageCards = new HeroCard[4];
            for (var i = 0; i < roomDetailDto.RoomImages.Count; i++)
                imageCards[i] = new HeroCard
                {
                    Title = roomDetailDto.Title,
                    Subtitle = string.Empty,
                    Text = string.Empty,
                    Images = new List<CardImage>
                    {
                        new CardImage(roomDetailDto.RoomImages[i].ImageUrl)
                    },
                    Tap = new CardAction
                    {
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
            var selectedRoomDetailDto = data as RoomDetailDto;
            var message = "The hotel would describe this room as: " + selectedRoomDetailDto.Description;
            return MessageFactory.Text(message);
        }

        public class ResponseIds
        {
            // all images? maybe room + bathroom separate?
            public const string SendImages = "SendImages";
            public const string SendGeneralDescription = "SendGeneralDescription";
        }
    }
}
