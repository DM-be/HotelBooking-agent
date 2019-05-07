using System.Collections.Generic;
using HotelBot.Dialogs.Main.Resources;
using HotelBot.Models.Facebook;
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
                { ResponseIds.QuickReplies,
                    (context, data) =>
                       SendQuickReplies(context, data)
                },

            }
        };

        public MainResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }
        public static IMessageActivity SendQuickReplies(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = "What would you like to next?",
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




        public class ResponseIds
        {
            // Constants
            public const string Cancelled = "cancelled";
            public const string Completed = "completed";
            public const string Confused = "confused";
            public const string Greeting = "greeting";
            public const string Help = "help";
            public const string Intro = "intro";
            public const string QuickReplies = "quickreplies";
        }


    }

}
