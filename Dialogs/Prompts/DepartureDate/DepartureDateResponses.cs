using System;
using System.Threading;
using HotelBot.Dialogs.Prompts.DepartureDate.Resources;
using HotelBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Prompts.DepartureDate
{
    public class DepartureDateResponses: TemplateManager
    {

        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.DepartureDatePrompt, (context, data) =>
                        GenerateRandomDeparturePrompt()
                },
             
                {
                    ResponseIds.HaveDepartureDate, (context, data) =>
                        GenerateRandomHaveDepartureResponse(data)
                },
                {
                    ResponseIds.HaveUpdatedDepartureDate, (context, data) =>
                        GenerateRandomUpdatedDepartureResponse(data)
                }
            }
        };


        public DepartureDateResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }


        private static IMessageActivity GenerateRandomDeparturePrompt()
        {

            var resourceManager = DepartureDateStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.DEPARTURE_DATE_PROMPT);
            return MessageFactory.Text(message);
        }

        private static IMessageActivity GenerateRandomUpdatedDepartureResponse(dynamic data)
        {

            var resourceManager = DepartureDateStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.HAVE_UPDATED_DEPARTURE_DATE);
            return MessageFactory.Text(string.Format(message, data));
        }

        private static IMessageActivity GenerateRandomHaveDepartureResponse(dynamic data)
        {

            var resourceManager = DepartureDateStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.HAVE_DEPARTURE_DATE);
            return MessageFactory.Text(string.Format(message, data));
        }


        public class ResponseIds
        {
            public const string DepartureDatePrompt = "departureDatePrompt";
            public const string HaveDepartureDate = "haveDepartureDate";
            public const string HaveUpdatedDepartureDate = "haveUpdatedDepartureDate";

        }

        public class ResponseKeys
        {

            public const string HAVE_DEPARTURE_DATE = "HAVE_DEPARTURE_DATE";
            public const string DEPARTURE_DATE_PROMPT = "DEPARTURE_DATE_PROMPT";
            public const string HAVE_UPDATED_DEPARTURE_DATE = "HAVE_UPDATED_DEPARTURE_DATE";
        }
    }
}
