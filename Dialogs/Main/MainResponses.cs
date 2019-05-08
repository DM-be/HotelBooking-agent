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
                { ResponseIds.BasicQuickReplies,
                    (context, data) =>
                        SendBasicQuickReplies(context, data)
                },
                { ResponseIds.AfterPaymentQuickReplies,
                    (context, data) =>
                        SendAfterPaymentQuickReplies(context, data)
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

        public static IMessageActivity SendBasicQuickReplies(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = "What would you like to do next?",
                QuickReplies = new[]
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "text",
                        Title = "Book me a room",
                        Payload = "test"
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }


        public static IMessageActivity SendAfterPaymentQuickReplies(ITurnContext context, dynamic data)
        {
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
                    Title = "Room overview",
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
        }


    }

}
