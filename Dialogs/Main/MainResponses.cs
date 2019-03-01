using System.Collections.Generic;
using HotelBot.Dialogs.Main.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Main
{
    public class MainResponses: TemplateManager
    {
        private static LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            // todo: implement basic welcoming based on postback (quick replies)
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
                }
            }
        };

        public MainResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
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
        }
    }

}
