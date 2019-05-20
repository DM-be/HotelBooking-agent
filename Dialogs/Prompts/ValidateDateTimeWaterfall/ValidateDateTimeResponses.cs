using HotelBot.Dialogs.Prompts.ValidateDateTimeWaterfall.Resources;
using HotelBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using System.Threading;

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
                    GenerateRandomArrivalResponse()
                },
                   {
                    ResponseIds.MatchedIntentNoEntityDeparture, (context, data) =>
                        GenerateRandomDepartureResponse()
                }

            }
        };

        public ValidateDateTimeResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }


        private static IMessageActivity GenerateRandomArrivalResponse()
        {

            var resourceManager = ValidateDateTimeStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.MATCHED_INTENT_NO_ENTITY_ARRIVAL);
            return MessageFactory.Text(message);
        }


        private static IMessageActivity GenerateRandomDepartureResponse()
        {

            var resourceManager = ValidateDateTimeStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.MATCHED_INTENT_NO_ENTITY_DEPARTURE);
            return MessageFactory.Text(message);
        }



        public class ResponseIds
        {
            public const string MatchedIntentNoEntityArrival = "matchedIntentNoEntityArrival";
            public const string MatchedIntentNoEntityDeparture = "matchedIntentNoEntityDeparture";
        }

        public class ResponseKeys
        {

            public const string MATCHED_INTENT_NO_ENTITY_DEPARTURE = "MATCHED_INTENT_NO_ENTITY_DEPARTURE";
            public const string MATCHED_INTENT_NO_ENTITY_ARRIVAL = "MATCHED_INTENT_NO_ENTITY_ARRIVAL";

        }
    }
}
