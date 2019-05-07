using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBot.Models.Facebook;
using HotelBot.Models.Wrappers;
using HotelBot.Shared.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                },
                {
                    ResponseIds.SendReceipt, (context, data) =>
                        SendReceiptCard(context, data)
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

        public static IMessageActivity SendReceiptCard(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage

            {


                Attachment = new FacebookAttachment
                {
                    Type = "template",
                    FacebookPayload = new FacebookPayload
                    {
                        Template_Type = "receipt",
                        RecipientName = "testname",
                        OrderNumber = "123456",
                        Currency = "EUR",
                        PaymentMethod = "MASTERCARD",
                        OrderUrl = "http://google.com",
                        Timestamp = "1428444852",
                        FacebookSummary = new FacebookSummary
                        {
                            Subtotal = 75.00,
                            TotalTax = 21.00,
                            TotalCost = 100.00

                        },
                        FacebookElements = new[]
                       {
                            new FacebookElement
                            {
                                Title = "test title",
                                Currency = "EUR",
                                ImageUrl =
                                    "https://static01.nyt.com/images/2019/03/24/travel/24trending-shophotels1/24trending-shophotels1-superJumbo.jpg?quality=90&auto=webp",
                                Price = 25.00,
                                Quantity = 1,
                                Subtitle = "test subtitle"


                            },
                            new FacebookElement
                            {
                                Title = "test title",
                                Currency = "EUR",
                                ImageUrl =
                                    "https://static01.nyt.com/images/2019/03/24/travel/24trending-shophotels1/24trending-shophotels1-superJumbo.jpg?quality=90&auto=webp",
                                Price = 25.00,
                                Quantity = 1,
                                Subtitle = "test subtitle"


                            }
                        }
                    },


                }

            };

            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
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


        private static ReceiptCard BuildReceiptCard(ConfirmOrderState confirmOrderState)
        {
            return new ReceiptCard
            {

                Title = "test",
                Total = "100",
                Tax = "20",
                Items = new List<ReceiptItem>
                {
                    new ReceiptItem
                    {
                        Title = "test",
                        Text = "TEST",
                        Subtitle = "TEST",
                        Price = "100",
                        Quantity = "10"

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
            message += $"Name: {confirmOrderState.FullName} \n";
            message += $"Email: {confirmOrderState.Email} \n";
            message += $"Number: {confirmOrderState.Number} \n";
            message += $"Total: €{totalPrice}\n";
            return message;

        }







        public class ResponseIds
        {
            public const string SendPaymentCard = "sendPaymentCard";
            public const string SendReceipt = "sendReceipt";
        }
    }

}
