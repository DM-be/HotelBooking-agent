using HotelBot.Dialogs.ConfirmOrder.Resources;
using HotelBot.Dialogs.Prompts.NumberOfPeople.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.ConfirmOrder
{
    public class ConfirmOrderResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.UseFacebookName, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(ConfirmOrderStrings.USE_FACEBOOK_NAME_QUESTION, data),
                            ssml: string.Format(ConfirmOrderStrings.USE_FACEBOOK_NAME_QUESTION, data),
                            inputHint: InputHints.IgnoringInput)
                }
            }
        };

        public ConfirmOrderResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string UseFacebookName = "useFacebookName";
        }
    }

}


