using HotelBot.Dialogs.Shared.Prompts.ValidateDateTimeWaterfall.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Shared.Prompts
{
    public class ValidateDateTimeResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.IncorrectFormatPrompt, (context, data) =>
                        MessageFactory.Text(
                            ValidateDateTimeStrings.TIME_INCORRECT_FORMAT,
                            ValidateDateTimeStrings.TIME_INCORRECT_FORMAT,
                            InputHints.AcceptingInput)
                }

            }
        };

        public ValidateDateTimeResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string IncorrectFormatPrompt = "incorrectFormatPrompt";
        }
    }
}
