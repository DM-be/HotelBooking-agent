using HotelBot.Dialogs.Prompts.ValidateDateTimeWaterfall.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Prompts.ValidateDateTimeWaterfall
{
    public class ValidateDateTimeResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.MatchedIntentNoEntityArrival, (context, data) =>
                        MessageFactory.Text(
                            ValidateDateTimeStrings.MATCHED_INTENT_NO_ENTITY_ARRIVAL,
                            ValidateDateTimeStrings.MATCHED_INTENT_NO_ENTITY_ARRIVAL,
                            InputHints.AcceptingInput)
                },
                   {
                    ResponseIds.MatchedIntentNoEntityDeparture, (context, data) =>
                        MessageFactory.Text(
                            ValidateDateTimeStrings.MATCHED_INTENT_NO_ENTITY_DEPARTURE,
                            ValidateDateTimeStrings.MATCHED_INTENT_NO_ENTITY_DEPARTURE,
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
            public const string MatchedIntentNoEntityArrival = "matchedIntentNoEntityArrival";
            public const string MatchedIntentNoEntityDeparture = "matchedIntentNoEntityDeparture";
        }
    }
}
