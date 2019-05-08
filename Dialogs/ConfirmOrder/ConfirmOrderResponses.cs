using System;
using System.Collections.Generic;
using HotelBot.Dialogs.ConfirmOrder.Resources;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Models.Facebook;
using HotelBot.Models.Wrappers;
using HotelBot.StateProperties;
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
                },
                {
                    ResponseIds.SendReceipt, (context, data) =>
                        SendReceiptCard(context, data)
                },
                {
                    ResponseIds.AfterConfirmation, (context, data) =>
                        MessageFactory.Text(
                            ConfirmOrderStrings.AFTER_CONFIRMATION,
                            ConfirmOrderStrings.AFTER_CONFIRMATION,
                            InputHints.IgnoringInput)
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
            foreach (var selectedRoom in confirmOrderState.RoomOverviewState.SelectedRooms)
                heroCards.Add(RoomOverviewResponses.BuildDetailedRoomHeroCard(selectedRoom));

            var reply = context.Activity.CreateReply();
            var attachments = new List<Attachment>();
            foreach (var heroCard in heroCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = "carousel";
            reply.Attachments = attachments;
            return reply;
        }

        public static IMessageActivity SendReceiptCard(ITurnContext context, dynamic data)
        {
            var confirmOrderState = data[0] as ConfirmOrderState;
            var userProfileState = data[1] as UserProfile;
            var facebookElements = new List<FacebookElement>();
            var totalCost = 0;
            foreach (var selectedRoom in confirmOrderState.RoomOverviewState.SelectedRooms)
            {
                facebookElements.Add(
                    new FacebookElement
                    {
                        Title = selectedRoom.RoomDetailDto.Title,
                        Subtitle = selectedRoom.RoomDetailDto.ShortDescription,
                        Price = selectedRoom.SelectedRate.Price,
                        Quantity = 1,
                        ImageUrl = selectedRoom.RoomDetailDto.RoomImages[0].ImageUrl,
                        Currency = "EUR"
                    });
                totalCost += selectedRoom.SelectedRate.Price;
            }

            var facebookMessage = new FacebookMessage

            {
                Attachment = new FacebookAttachment
                {
                    Type = "template",
                    FacebookPayload = new FacebookPayload
                    {
                        Template_Type = "receipt",
                        RecipientName = userProfileState.FacebookProfileData.Name,
                        OrderNumber = "order-565678",
                        Currency = "EUR",
                        PaymentMethod = "Mastercard",
                        OrderUrl = "http://google.com",
                        Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                        FacebookSummary = new FacebookSummary
                        {
                            TotalCost = totalCost
                        },
                        FacebookElements = facebookElements
                    }
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
                Title = "Booking confirmation overview", //todo: better title
                Subtitle = BuildPaymentHeroCardText(confirmOrderState),

                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        Value = JsonConvert.SerializeObject(
                            new RoomAction
                            {
                                Action = RoomAction.Actions.Paid
                            }),
                        Title = "\t Pay \t"
                    }
                },
                Images = new List<CardImage>
                {
                    new CardImage
                    {
                        Url = "https://freerangestock.com/sample/118792/tablet-and-hand-digital-payment-icon-vector.jpg"
                    }
                }


            };
        }


        public static string BuildPaymentHeroCardText(ConfirmOrderState confirmOrderState)
        {
            var selectedRooms = confirmOrderState.RoomOverviewState.SelectedRooms;
            var numberOfPeople = 0;
            var totalPrice = 0;
            for (var i = 0; i < selectedRooms.Count; i++)
            {
                numberOfPeople += selectedRooms[i].RoomDetailDto.Capacity;
                totalPrice += selectedRooms[i].SelectedRate.Price;
            }

            var message = "";
            message += $"{confirmOrderState.FullName} \n";
            message += $"{confirmOrderState.Email} \n";
            message += $"{confirmOrderState.Number} \n";
            message += $"Total: €{totalPrice}\n";
            return message;

        }


        public class ResponseIds
        {
            public const string SendPaymentCard = "sendPaymentCard";
            public const string SendReceipt = "sendReceipt";
            public const string AfterConfirmation = "afterConfirmation";
        }
    }

}
