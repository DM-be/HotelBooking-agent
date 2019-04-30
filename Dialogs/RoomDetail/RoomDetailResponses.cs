using System;
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
                        SendDescription(context, data)
                },
                {
                    ResponseIds.SendRates, (context, data) =>
                        SendRates(context, data)
                },
                {
                    ResponseIds.SendLowestRate, (context, data) =>
                        SendLowestRate(context, data)
                },
                {
                    ResponseIds.RoomChoicesPrompt, (context, data) =>
                        MessageFactory.Text(RoomDetailStrings.ROOM_CHOICES_PROMPT)
                },
                {
                    ResponseIds.RoomChoicesPromptLooped, (context, data) =>
                        MessageFactory.Text(RoomDetailStrings.ROOM_CHOICES_PROMPT_LOOPED)
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
                    Subtitle = String.Empty,
                    Text = String.Empty,
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
            reply.Text = "Tap these pictures to enlarge.";
            var attachments = new List<Attachment>();
            foreach (var heroCard in imageCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = "carousel";
            reply.Attachments = attachments;
            return reply;
        }

        public static IMessageActivity SendRates(ITurnContext context, dynamic data)
        {
            var roomDetailDto = data as RoomDetailDto;
            var rateCards = new HeroCard[2];
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
                                    Id = roomDetailDto.Id,
                                    Action = "selectRoomWithRate",
                                    SelectedRate = roomDetailDto.Rates[i]
                                }),
                            Title = $"Book for €{roomDetailDto.Rates[i].Price}",
                            Text = "show me more info for x room"

                        }
                    }
                };
            var reply = context.Activity.CreateReply();
            reply.Text = "This room is available with following rates:";
            var attachments = new List<Attachment>();
            foreach (var heroCard in rateCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = "carousel";
            reply.Attachments = attachments;
            return reply;
        }


        public static IMessageActivity SendDescription(ITurnContext context, dynamic data)
        {
            var selectedRoomDetailDto = data as RoomDetailDto;
            return MessageFactory.Text(selectedRoomDetailDto.Description);
        }

        public static IMessageActivity SendLowestRate(ITurnContext context, dynamic data)
        {
            var selectedRoomDetailDto = data as RoomDetailDto;
            var message = $"This rooms is available starting from {selectedRoomDetailDto.LowestRate} EUR.";
            return MessageFactory.Text(message);
        }

        public class ResponseIds
        {
            public const string SendImages = "sendImages";
            public const string SendDescription = "sendDescription";
            public const string SendRates = "sendRates";
            public const string SendLowestRate = "sendLowestRate";

            public const string RoomChoicesPrompt = "roomChoicesPrompt";
            public const string RoomChoicesPromptLooped = "roomChoisesPromptLooped";
        }

    }
}
