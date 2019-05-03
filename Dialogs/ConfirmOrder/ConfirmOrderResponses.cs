using System.Collections.Generic;
using HotelBot.Models.Wrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace HotelBot.Dialogs.ConfirmOrder
{
    public class ConfirmOrderResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.SendPaymentCard, (context, data) =>
                        SendPaymentCard(context, data)
                }
            }
        };

        public ConfirmOrderResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity SendPaymentCard(ITurnContext context, dynamic data)
        {

            var confirmOrderState = data as ConfirmOrderState;
            var heroCards = new List<HeroCard>();
            heroCards.Add(BuildPaymentHeroCard(confirmOrderState));
            var reply = context.Activity.CreateReply();
            reply.Text =
                "Your confirmation overview:";
            var attachments = new List<Attachment>();
            foreach (var heroCard in heroCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = "carousel";
            reply.Attachments = attachments;
            return reply;
        }


        private static HeroCard BuildPaymentHeroCard(ConfirmOrderState confirmOrderState)
        {
            return new HeroCard
            {
                Title = "Payment confirmation overview", //todo: better title
                Subtitle = BuildPaymentHeroCardText(confirmOrderState),

                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        Value = JsonConvert.SerializeObject(
                            new RoomAction
                            {
                                Action = RoomAction.Actions.Confirm
                            }),
                        Title = "\t Pay \t"
                    }
                }

            };
        }

        public static string BuildPaymentHeroCardText(ConfirmOrderState confirmOrderState)
        {
            var selectedRooms = confirmOrderState.RoomOverviewState.SelectedRooms;


            var numberOfRooms = selectedRooms.Count;
            var numberOfPeople = 0;
            var totalPrice = 0;
            var cardImages = new List<CardImage>();

            for (var i = 0; i < selectedRooms.Count; i++)
            {
                numberOfPeople += selectedRooms[i].RoomDetailDto.Capacity;
                totalPrice += selectedRooms[i].SelectedRate.Price;
                cardImages.Add(new CardImage(selectedRooms[i].RoomDetailDto.RoomImages[i].ImageUrl));
            }

            var message = "";
            message += $"Number of rooms: {numberOfRooms} \n";
            message += $"Number of people: {numberOfPeople} \n";
            message += $"Confirmation Name: {confirmOrderState.FullName} \n";
            message += $"Confirmation Email: {confirmOrderState.Email} \n";
            message += $"Emergency number: {confirmOrderState.Number} \n";
            message += $"Total: €{totalPrice}\n";
            return message;

        }




        public class ResponseIds
        {
            public const string SendPaymentCard = "sendPaymentCard";
        }
    }

}
