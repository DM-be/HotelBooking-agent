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
                    ResponseIds.TapPayToComplete, (context, data) =>
                        MessageFactory.Text(
                            ConfirmOrderStrings.TAP_PAY_TO_COMPLETE,
                            ConfirmOrderStrings.TAP_PAY_TO_COMPLETE,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.ThanksInformation, (context, data) =>
                        MessageFactory.Text(
                            ConfirmOrderStrings.THANKS_INFORMATION,
                            ConfirmOrderStrings.THANKS_INFORMATION,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.SendPhoneNumberQuickReply, (context, data) =>
                        BuildPhoneNumberQuickReply(context, data)
                },
                {
                    ResponseIds.SendFullNameQuickReply, (context, data) =>
                        BuildFullNameQuickReply(context, data)
                },
              

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
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = attachments;
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


        

        public static IMessageActivity BuildPhoneNumberQuickReply(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = ConfirmOrderStrings.ASK_NUMBER,
                QuickReplies = new[]
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "user_phone_number"
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public static IMessageActivity BuildFullNameQuickReply(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = ConfirmOrderStrings.FULL_NAME_QUICK_REPLY_TEXT,
                QuickReplies = new[]
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "text",
                        Title = data,
                        Payload = data
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }


        public class ResponseIds
        {
            public const string SendPaymentCard = "sendPaymentCard";
            public const string ThanksInformation = "thanksInformation";
            public const string TapPayToComplete = "tapPayToComplete";
            public const string SendEmailQuickReply = "sendEmailQuickReply";
            public const string SendPhoneNumberQuickReply = "SendPhoneNumberQuickReply";
            public const string SendFullNameQuickReply = "SendFullNameQuickReply";
        }
    }

}
