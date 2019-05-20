using HotelBot.Dialogs.Shared.PromptValidators.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Shared.PromptValidators
{
    public class PromptValidatorResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.IncorrectDate, (context, data) =>
                        MessageFactory.Text(
                            ValidatorStrings.INCORRECT_DATE,
                            ValidatorStrings.INCORRECT_DATE,
                            InputHints.AcceptingInput)

                },
                {
                    ResponseIds.NotRecognizedDate, (context, data) =>
                        MessageFactory.Text(
                            ValidatorStrings.NOT_RECOGNIZED_DATE,
                            ValidatorStrings.NOT_RECOGNIZED_DATE,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.NotInThePast, (context, data) =>
                        MessageFactory.Text(
                            ValidatorStrings.NOT_IN_THE_PAST_DATE,
                            ValidatorStrings.NOT_IN_THE_PAST_DATE,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.InvalidEmail, (context, data) =>
                        MessageFactory.Text(
                            ValidatorStrings.INVALID_EMAIL,
                            ValidatorStrings.INVALID_EMAIL,
                            InputHints.AcceptingInput)
                }

            }


        };

        public PromptValidatorResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string IncorrectDate = "incorrectDate";
            public const string NotRecognizedDate = "notRecognizedDate";
            public const string NotInThePast = "notInThePast";
            public const string InvalidEmail = "invalidEmail";
            public const string MatchedUpdateArrivalNoEntity = "matchedUpdateArrivalNoEntity";
        }
    }
}
