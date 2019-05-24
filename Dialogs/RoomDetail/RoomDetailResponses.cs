using System.Collections.Generic;
using HotelBot.Dialogs.RoomDetail.Resources;
using HotelBot.Models.DTO;
using HotelBot.Models.Wrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

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
                    ResponseIds.SendDescription, (context, data) =>
                        SendDescription(data)
                },
                {
                    ResponseIds.SendRates, (context, data) =>
                        SendRates(context, data)
                },
                {
                    ResponseIds.SendLowestRate, (context, data) =>
                        SendLowestRate(data)
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
            var imageCards = new HeroCard[roomDetailDto.RoomImages.Count];
            for (var i = 0; i < roomDetailDto.RoomImages.Count; i++)
                imageCards[i] = new HeroCard
                {
                    Title = roomDetailDto.Title,
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
            reply.Text = RoomDetailStrings.IMAGES_TAP_TEXT;
            var attachments = new List<Attachment>();
            foreach (var heroCard in imageCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = attachments;
            return reply;
        }

        public static IMessageActivity SendRates(ITurnContext context, dynamic data)
        {
            var roomDetailDto = data as RoomDetailDto;
            var rateCards = new HeroCard[roomDetailDto.Rates.Count];
            for (var i = 0; i < roomDetailDto.Rates.Count; i++)
                rateCards[i] = new HeroCard
                {
                    Title = roomDetailDto.Rates[i].RateName,
                    Subtitle = roomDetailDto.Rates[i].RateDescription,
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            Value = JsonConvert.SerializeObject(
                                new RoomAction
                                {
                                    RoomId = roomDetailDto.Id,
                                    Action = RoomAction.Actions.SelectRoomWithRate,
                                    SelectedRate = roomDetailDto.Rates[i]
                                }),
                            Title = string.Format(RoomDetailStrings.HEROCARD_RATES_BUTTON_TEXT, roomDetailDto.Rates[i].Price),
                        }
                    }
                };
            var reply = context.Activity.CreateReply();
            reply.Text = RoomDetailStrings.HEROCARD_RATES_REPLY_TEXT;
            var attachments = new List<Attachment>();
            foreach (var heroCard in rateCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = attachments;
            return reply;
        }


        public static IMessageActivity SendDescription(dynamic data)
        {
            var selectedRoomDetailDto = data as RoomDetailDto;
            return MessageFactory.Text(selectedRoomDetailDto.Description);
        }

        public static IMessageActivity SendLowestRate(dynamic data)
        {
            var selectedRoomDetailDto = data as RoomDetailDto;
            var message = string.Format(RoomDetailStrings.LOWEST_RATE_TEXT, selectedRoomDetailDto.LowestRate);
            return MessageFactory.Text(message);
        }

        public class ResponseIds
        {
            public const string SendImages = "sendImages";
            public const string SendDescription = "sendDescription";
            public const string SendRates = "sendRates";
            public const string SendLowestRate = "sendLowestRate";
        }
    }
}
