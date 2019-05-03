using System.Collections.Generic;
using HotelBot.Models.DTO;
using HotelBot.Models.Wrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace HotelBot.Dialogs.Prompts.SelectRatePrompt
{
    //TODO: remove or move into this file?
    public class SelectRateResponses: TemplateManager
    {

        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.SendRates, (context, data) =>
                        SendRates(context, data)
                }

            }

        };

        public SelectRateResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
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
                                    RoomId = roomDetailDto.Id,
                                    Action = RoomAction.Actions.SelectRoomWithRate,
                                    SelectedRate = roomDetailDto.Rates[i]

                                }),
                            Title = $"Book for €{roomDetailDto.Rates[i].Price}"

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


        public class ResponseIds
        {
            public const string SendRates = "sendRates";
        }
    }
}
