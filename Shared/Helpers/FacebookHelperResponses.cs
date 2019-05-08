using System.Collections.Generic;
using HotelBot.Dialogs.ConfirmOrder.Resources;
using HotelBot.Models.Facebook;
using HotelBot.Shared.Helpers.Resources;
using HotelBot.Shared.QuickReplies.Resources;
using HotelBot.Shared.Welcome.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Shared.Helpers
{
    public class FacebookHelperResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {

            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.SendLocationQuickReply, (context, data) =>
                        BuildLocationQuickReply(context, data)
                },
                {
                    ResponseIds.SendPhoneNumberQuickReply, (context, data) =>
                        BuildPhoneNumberQuickReply(context, data)
                },
                {
                    ResponseIds.SendFullNameQuickReply, (context, data) =>
                        BuildFullNameQuickReply(context, data)
                },
                {
                    ResponseIds.SendEmailQuickReply, (context, data) =>
                        BuildEmailQuickReply(context, data)
                },
                {
                    ResponseIds.SendGetStartedQuickReplies, (context, data) =>
                        BuildGettingStartedQuickReplies(context, data)
                },
                {
                    ResponseIds.CallUs, (context, data) =>
                        BuildCallMessage(context, data)
                },
                {
                    ResponseIds.Welcome, (context, data) =>
                        MessageFactory.Text(
                            WelcomeStrings.WELCOME_MESSAGE,
                            WelcomeStrings.WELCOME_MESSAGE,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.Functionality, (context, data) =>
                        MessageFactory.Text(
                            WelcomeStrings.FUNCTIONALITY,
                            WelcomeStrings.FUNCTIONALITY,
                            InputHints.AcceptingInput)
                }
            }
        };

        public FacebookHelperResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity BuildLocationQuickReply(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = FacebookStrings.QUICK_REPLY_ASK_LOCATION,
                QuickReplies = new []
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "location"
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public static IMessageActivity BuildEmailQuickReply(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = string.Format(ConfirmOrderStrings.RESPOND_NAME, data),
                QuickReplies = new []
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "user_email"
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public static IMessageActivity BuildPhoneNumberQuickReply(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = FacebookStrings.QUICK_REPLY_ASK_NUMBER,
                QuickReplies = new []
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
                Text = "What name should we use to confirm this booking?",
                QuickReplies = new []
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

        //TODO: move into main dialog 
        public static IMessageActivity BuildGettingStartedQuickReplies(ITurnContext context, dynamic data)
        {
            var reply = context.Activity.CreateReply();
            FacebookQuickReply [] quick_replies =
            {
                new FacebookQuickReply
                {
                    Title = FacebookStrings.QUICK_REPLY_BUTTON_BOOK_A_ROOM,
                    Content_Type = "text",
                    Payload = "book"
                },
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
                }
            };
            var facebookMessage = new FacebookMessage
            {
                Text = QuickReplyStrings.WELCOME_OPTIONS
            };

            facebookMessage.QuickReplies = quick_replies;
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public static IMessageActivity BuildCallMessage(ITurnContext context, dynamic data)
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

        public class ResponseIds
        {
            // Constants
            public const string SendGetStartedQuickReplies = "sendGetStartedQuickReplies";

            public const string SendLocationQuickReply = "sendLocationQuickReply";
            public const string Functionality = "functionality";
            public const string Welcome = "welcome";
            public const string CallUs = "callUs";
            public const string SendEmailQuickReply = "sendEmailQuickReply";
            public const string SendPhoneNumberQuickReply = "SendPhoneNumberQuickReply";
            public const string SendFullNameQuickReply = "SendFullNameQuickReply";
        }
    }
}
