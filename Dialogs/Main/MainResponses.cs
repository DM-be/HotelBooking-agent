using System;
using System.Collections.Generic;
using System.Threading;
using HotelBot.Dialogs.ConfirmOrder;
using HotelBot.Dialogs.Main.Resources;
using HotelBot.Dialogs.Prompts.LocationPrompt.Resources;
using HotelBot.Extensions;
using HotelBot.Models.Facebook;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Main
{
    public class MainResponses : TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.Cancelled, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.CANCELLED,
                            MainStrings.CANCELLED,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.Completed, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.COMPLETED,
                            MainStrings.COMPLETED,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.Confused, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.CONFUSED,
                            MainStrings.CONFUSED,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.Help, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.HELP,
                            MainStrings.HELP,
                            InputHints.AcceptingInput)
                },
                 {
                    ResponseIds.WhatCanYouDoTasks, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.WHAT_CAN_YOU_DO_TASKS,
                            MainStrings.WHAT_CAN_YOU_DO_TASKS,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.WhatCanYouDoSmart, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.WHAT_CAN_YOU_DO_SMART,
                            MainStrings.WHAT_CAN_YOU_DO_SMART,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.WhatCanYouDoNLU, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.WHAT_CAN_YOU_DO_NLU,
                            MainStrings.WHAT_CAN_YOU_DO_NLU,
                            InputHints.AcceptingInput)
                },

                {
                    ResponseIds.GreetingWithName, (context, data) =>
                        MessageFactory.Text(
                            string.Format(MainStrings.GREETING_WITH_NAME, data),
                            string.Format(MainStrings.GREETING_WITH_NAME, data),
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.GreetingPromptForAction, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.GREETING_PROMPT_FOR_ACTION,
                            MainStrings.GREETING_PROMPT_FOR_ACTION,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.ConfirmedPaymentQuickReplies, (context, data) =>
                        SendConfirmedPaymentQuickReplies(context, data)
                },
                {
                    ResponseIds.UnconfirmedPaymentQuickReplies, (context, data) =>
                        SendUnconfirmedPaymentQuickReplies(context, data)
                },
                {
                    ResponseIds.EmptyRoomOverviewStateQuickReplies, (context, data) =>
                        SendEmptyRoomOverviewStateQuickReplies(context, data)
                },
                 {
                    ResponseIds.SendGettingStartedQuickReplies, (context, data) =>
                        SendGettingStartedQuickReplies(context)
                },

                {
                    ResponseIds.SendCallCard, (context, data) =>
                        BuildCallMessage(context)

                },
                   {
                    ResponseIds.SendReceipt, (context, data) =>
                        SendReceiptCard(context, data)

                },
                       {
                    ResponseIds.AfterConfirmation, (context, data) =>
                        MessageFactory.Text(
                            MainStrings.AFTER_CONFIRMATION,
                            MainStrings.AFTER_CONFIRMATION,
                            InputHints.IgnoringInput)
                },


            }
        };

        public MainResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
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


        public static IMessageActivity BuildCallMessage(ITurnContext context)
        {
            var heroCard = new HeroCard
            {
                Title = MainStrings.CALL_CARD_TITLE,
                Subtitle = MainStrings.HOTEL_NUMBER,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.Call, MainStrings.CALL_BUTTON_TEXT, value: MainStrings.HOTEL_NUMBER)
                }
            };
            var reply = context.Activity.CreateReply();
            reply.Text = MainStrings.CALL_REPLY_TEXT;
            reply.Attachments = new List<Attachment>
            {
                heroCard.ToAttachment()
            };
            return reply;
        }

        public static IMessageActivity SendGettingStartedQuickReplies(ITurnContext context)
        {

            var facebookMessage = new FacebookMessage
            {
                Text = MainStrings.GREETING_PROMPT_FOR_ACTION,
                QuickReplies = new List<FacebookQuickReply>
                {
                    new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_WHAT_CAN_YOU_DO,
                        Payload = string.Empty
                    },
                    new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_FIND_A_ROOM,
                        Payload = FacebookQuickReply.PayLoads.Book
                    },
                    new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_CALL,
                        Payload = FacebookQuickReply.PayLoads.Call
                    },
                      new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_DIRECTION,
                        Payload = FacebookQuickReply.PayLoads.Directions
                    }

                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public static IMessageActivity SendEmptyRoomOverviewStateQuickReplies(ITurnContext context, dynamic data)
        {

            string text;
            if (data != null && data.GetType() == typeof(string))
            {
                text = data;
            }
            else {
                text = GenerateRandomCompleteMessage().Text;
            }

            var facebookMessage = new FacebookMessage
            {
                Text = text,
                QuickReplies = GenerateFacebookQuickRepliesBasedOnState(States.Empty)
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }


        public static IMessageActivity SendUnconfirmedPaymentQuickReplies(ITurnContext context, dynamic data)
        {
            string text;
            if (data != null && data.GetType() == typeof(string))
            {
                text = data;
            }
            else
            {
                text = GenerateRandomCompleteMessage().Text;
            }
            var facebookMessage = new FacebookMessage
            {
                Text = text,
                QuickReplies = GenerateFacebookQuickRepliesBasedOnState(States.Unconfirmed),
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }



        public static IMessageActivity SendConfirmedPaymentQuickReplies(ITurnContext context, dynamic data)
        {

            string text;
            if (data != null && data.GetType() == typeof(string))
            {
                text = data;
            }
            else
            {
                text = GenerateRandomCompleteMessage().Text;
            }
            var facebookMessage = new FacebookMessage
            {
                Text = text,
                QuickReplies = GenerateFacebookQuickRepliesBasedOnState(States.Confirmed),
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }


        private static IMessageActivity GenerateRandomCompleteMessage() {

            var mainStrings = MainStrings.ResourceManager;
            var resourceSet = mainStrings.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse("RANDOM_CONTINUE");
            return MessageFactory.Text(message);
        }


        private static List<FacebookQuickReply> GenerateFacebookQuickRepliesBasedOnState(States state) {
            switch (state)
            {
                case States.Empty:
                    var emptyReplies = GenerateGeneralQuickReplies();
                    emptyReplies.Insert(0, new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_WHAT_CAN_YOU_DO,
                        Payload = string.Empty
                    });
                    emptyReplies.Insert(1, new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_FIND_A_ROOM,
                        Payload = FacebookQuickReply.PayLoads.Book
                    });
                    return emptyReplies;
                case States.Unconfirmed:
                    var unconfirmedReplies = GenerateGeneralQuickReplies();
                 
                    unconfirmedReplies.Insert(0, new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_BOOKING_OVERVIEW,
                        Payload = FacebookQuickReply.PayLoads.BookingOverview
                    });
                    unconfirmedReplies.Insert(1, new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_FIND_A_ROOM,
                        Payload = FacebookQuickReply.PayLoads.Book
                    });
                    unconfirmedReplies.Add(new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_WHAT_CAN_YOU_DO,
                        Payload = string.Empty
                    });
                    return unconfirmedReplies;
                    
                case States.Confirmed:
                    var confirmedReplies = GenerateGeneralQuickReplies();
                    confirmedReplies.Add(new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_BOOKING_OVERVIEW,
                        Payload = FacebookQuickReply.PayLoads.BookingOverview
                    });

                    confirmedReplies.Add(new FacebookQuickReply
                    {
                        Content_Type = FacebookQuickReply.ContentTypes.Text,
                        Title = MainStrings.QUICK_REPLY_BUTTON_WHAT_CAN_YOU_DO,
                        Payload = string.Empty
                    });
                    return confirmedReplies;

            }
            return null;
        
        }

        private static List<FacebookQuickReply> GenerateGeneralQuickReplies() {

            return new List<FacebookQuickReply> {

                        new FacebookQuickReply
                        {
                            Content_Type = FacebookQuickReply.ContentTypes.Text,
                            Title = MainStrings.QUICK_REPLY_BUTTON_CALL,
                            Payload = FacebookQuickReply.PayLoads.Call
                        },
                        new FacebookQuickReply
                        {
                            Content_Type = FacebookQuickReply.ContentTypes.Text,
                            Title = MainStrings.QUICK_REPLY_BUTTON_DIRECTION,
                            Payload = FacebookQuickReply.PayLoads.Directions
                        },
            };
        } 

        enum States {
            Unconfirmed,
            Confirmed,
            Empty
        }



        public class ResponseIds
        {
            // Constants
            public const string Cancelled = "cancelled";
            public const string Completed = "completed";
            public const string Confused = "confused";
            public const string Help = "help";
            public const string SendCallCard = "sendCallCard";
            public const string ConfirmedPaymentQuickReplies = "confirmedPaymentQuickReplies";
            public const string UnconfirmedPaymentQuickReplies = "unconfirmedPaymentQuickReplies";
            public const string EmptyRoomOverviewStateQuickReplies = "emptyRoomOverviewStateQuickReplies";
            public const string SendGettingStartedQuickReplies = "sendGettingStartedQuickReplies";
            public const string SendReceipt = "SendReceipt";
            public const string AfterConfirmation = "AfterConfirmation";

            public const string WhatCanYouDoTasks = "whatCanYouDoTasks";
            public const string WhatCanYouDoSmart = "whatCanYouDoSmart";
            public const string WhatCanYouDoNLU = "whatCanYouDoNLU";

            // intro
            public const string GreetingWithName = "greetingWithName";
            public const string GreetingPromptForAction = "greetingPromptForAction";
        }
    }

}
