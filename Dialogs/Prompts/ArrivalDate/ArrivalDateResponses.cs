
using System.Resources;
using System.Threading;
using HotelBot.Dialogs.Prompts.ArrivalDate.Resources;
using HotelBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Prompts.ArrivalDate
{
    public class ArrivalDateResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.ArrivalDatePrompt, (context, data) =>
                      GenerateRandomArrivalPrompt()
                },
                {
                    ResponseIds.HaveArrivalDate, (context, data) =>
                       GenerateRandomHaveArrivalResponse(data)
                },
                {
                    ResponseIds.HaveUpdatedArrivalDate, (context, data) =>
                        GenerateRandomUpdatedArrivalResponse(data)
                }
            }
        };


        public ArrivalDateResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        private static IMessageActivity GenerateRandomArrivalPrompt()
        {

            var resourceManager = ArrivalDateStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.ARRIVAL_DATE_PROMPT);
            return MessageFactory.Text(message);
        }

        private static IMessageActivity GenerateRandomUpdatedArrivalResponse(dynamic data)
        {

            var resourceManager = ArrivalDateStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.HAVE_UPDATED_ARRIVAL_DATE);
            return MessageFactory.Text(string.Format(message, data));
        }

        private static IMessageActivity GenerateRandomHaveArrivalResponse(dynamic data)
        {

            var resourceManager = ArrivalDateStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.HAVE_ARRIVAL_DATE);
            return MessageFactory.Text(string.Format(message, data));
        }



        public class ResponseIds
        {
            public const string ArrivalDatePrompt = "arrivalDatePrompt";
            public const string HaveArrivalDate = "haveArrivalDate";
            public const string HaveUpdatedArrivalDate = "haveUpdatedArrivalDate";
        }

        public class ResponseKeys {

            public const string HAVE_ARRIVAL_DATE = "HAVE_ARRIVAL_DATE";
            public const string ARRIVAL_DATE_PROMPT = "ARRIVAL_DATE_PROMPT";
            public const string HAVE_UPDATED_ARRIVAL_DATE = "HAVE_UPDATED_ARRIVAL_DATE";
        }
    }
}

