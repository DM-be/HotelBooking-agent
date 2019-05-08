using System.Collections.Generic;
using HotelBot.Dialogs.Main.Resources;
using HotelBot.Models.Facebook;
using HotelBot.Shared.Helpers.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Main
{
    public class MainResponses: TemplateManager
    {
        private static LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                { ResponseIds.Cancelled,
                    (context, data) =>
                        MessageFactory.Text(
                            text: MainStrings.CANCELLED,
                            ssml: MainStrings.CANCELLED,
                            inputHint: InputHints.AcceptingInput)
                },
                { ResponseIds.Completed,
                    (context, data) =>
                        MessageFactory.Text(
                            text: MainStrings.COMPLETED,
                            ssml: MainStrings.COMPLETED,
                            inputHint: InputHints.AcceptingInput)
                },
                { ResponseIds.Confused,
                    (context, data) =>
                        MessageFactory.Text(
                            text: MainStrings.CONFUSED,
                            ssml: MainStrings.CONFUSED,
                            inputHint: InputHints.AcceptingInput)
                },
                { ResponseIds.Greeting,
                    (context, data) =>
                        MessageFactory.Text(
                            text: MainStrings.GREETING,
                            ssml: MainStrings.GREETING,
                            inputHint: InputHints.AcceptingInput)
                },
                { ResponseIds.Help,
                    (context, data) =>
                        MessageFactory.Text(
                            text: MainStrings.HELP,
                            ssml: MainStrings.HELP,
                            inputHint: InputHints.AcceptingInput)
                },
                { ResponseIds.ConfirmedPaymentQuickReplies,
                    (context, data) =>
                        SendConfirmedPaymentQuickReplies(context, data)
                },
                { ResponseIds.UnconfirmedPaymentQuickReplies,
                    (context, data) =>
                        SendUnconfirmedPaymentQuickReplies(context, data)
                },
                { ResponseIds.EmptyRoomOverviewStateQuickReplies,
                    (context, data) =>
                        SendEmptyRoomOverviewStateQuickReplies(context, data)
                },

                {
                    ResponseIds.SendCallCard, (context, data) =>
                        BuildCallMessage(context)

                }

            }
        };

        public MainResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity BuildCallMessage(ITurnContext context)
        {
            var number = "tel: +15 105 551 234";
            var heroCard = new HeroCard
            {
                Title = FacebookStrings.BUTTON_TITLE_CALL,
                Subtitle = number,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.Call, FacebookStrings.BUTTON_TITLE_CALL, value: number)
                }
            };
            var reply = context.Activity.CreateReply();
            reply.Attachments = new List<Attachment>
            {
                heroCard.ToAttachment()
            };
            return reply;
        }


        // state is empty, there is nothing in the room overview, only prompt add a room
        public static IMessageActivity SendEmptyRoomOverviewStateQuickReplies(ITurnContext context, dynamic data)
        {

            // nothing in room overview 
            //      * Find a room
            //      * (call hotel)

            var facebookMessage = new FacebookMessage
            {
                Text = "What would you like to do next?",
                QuickReplies = new[]
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "text",
                        Title = "Find a room",
                        Payload = "test"
                    },
                    new FacebookQuickReply
                    {
                        Content_Type = "text",
                        Title = "Call us",
                        Payload = "test"
                    }

                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }




        // has rooms in room overview state, but is not confirmed with payment. 
        public static IMessageActivity SendUnconfirmedPaymentQuickReplies(ITurnContext context, dynamic data)
        {

            // End dialog quick replies
            // rooms in order not confirmed:
            //      * Add a room
            //      * Booking overview
            //      * (call)
            //      * (Confirm booking)

            var facebookMessage = new FacebookMessage
            {
                Text = "What would you like to do next?",
                QuickReplies = new[]
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "text",
                        Title = "Add a room",
                        Payload = "test"
                    },
                    new FacebookQuickReply
                    {
                    Content_Type = "text",
                    Title = "Booking overview",
                    Payload = "test"
                    },
                    new FacebookQuickReply
                    {
                        Content_Type = "text",
                        Title = "Confirm booking",
                        Payload = "test"
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }



        //payment is confirmed 
        public static IMessageActivity SendConfirmedPaymentQuickReplies(ITurnContext context, dynamic data)
        {

            // (Book more rooms)
            // get directions
            // call hotel
            // booking overview
            // cancel bookings
            var facebookMessage = new FacebookMessage
            {
                Text = "Do you have any more questions about our hotel or services?",
                QuickReplies = new[]
                {
                    new FacebookQuickReply
                    {
                        Title = FacebookStrings.QUICK_REPLY_BUTTON_DIRECTION,
                        Content_Type = "text",
                        Payload = "location"
                    },
                    new FacebookQuickReply
                    {
                        Title = FacebookStrings.QUICK_REPLY_BUTTON_CALL,
                        Content_Type = "text",
                        Payload = "call"
                    },
                    new FacebookQuickReply
                    {
                    Title = "Booking overview",
                    Content_Type = "text",
                    Payload = "none"
                    },
                    new FacebookQuickReply
                    {
                        Title = "Cancel booking", //todo: implement second dialog cancelling bookings
                        Content_Type = "text",
                        Payload = "none"
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }





        public class ResponseIds
        {
            // Constants
            public const string Cancelled = "cancelled";
            public const string Completed = "completed";
            public const string Confused = "confused";
            public const string Greeting = "greeting";
            public const string Help = "help";
            public const string Intro = "intro";
            public const string BasicQuickReplies = "basicQuickReplies";
            public const string AfterPaymentQuickReplies = "afterPaymentQuickReplies";
            public const string SendCallCard = "sendCallCard";

            public const string ConfirmedPaymentQuickReplies = "confirmedPaymentQuickReplies";
            public const string UnconfirmedPaymentQuickReplies = "unconfirmedPaymentQuickReplies";
            public const string EmptyRoomOverviewStateQuickReplies = "emptyRoomOverviewStateQuickReplies";

        }


    }

}
