using HotelBot.Dialogs.Shared.PromptValidators.Resources;
using HotelBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using System.Threading;

namespace HotelBot.Dialogs.Shared.PromptValidators
{
    public class PromptValidatorResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.MissingDayOfMonth, (context, data) =>
                        GenerateRandomMissingDayOfMonthResponse()

                },
                {
                    ResponseIds.NotRecognizedDate, (context, data) =>
                        MessageFactory.Text(
                            ValidatorStrings.NOT_RECOGNIZED_DATE,
                            ValidatorStrings.NOT_RECOGNIZED_DATE,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.NotInThePastDate, (context, data) =>
                     GenerateRandomRandomNotInThePastDateResponse()
                },
                   {
                    ResponseIds.DepartureBeforeArrival, (context, data) =>
                     GenerateRandomRandomNotDepartureBeforeArrivalResponse()
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

     

        private static IMessageActivity GenerateRandomMissingDayOfMonthResponse()
        {

            var resourceManager = ValidatorStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.MISSING_DAY_OF_MONTH);
            return MessageFactory.Text(message);
        }

        private static IMessageActivity GenerateRandomRandomNotInThePastDateResponse()
        {

            var resourceManager = ValidatorStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.NOT_IN_THE_PAST_DATE);
            return MessageFactory.Text(message);
        }

        private static IMessageActivity GenerateRandomRandomNotDepartureBeforeArrivalResponse()
        {

            var resourceManager = ValidatorStrings.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            var message = resourceSet.GenerateRandomResponse(ResponseKeys.DEPARTURE_BEFORE_ARRIVAL);
            return MessageFactory.Text(message);
        }

        public class ResponseIds
        {
            public const string NotRecognizedDate = "notRecognizedDate";
            public const string NotInThePastDate = "notInThePastDate";
            public const string DepartureBeforeArrival = "departureBeforeArrival";
            public const string MissingDayOfMonth = "missingDayOfMonth";
            public const string InvalidEmail = "invalidEmail";
        }
        public class ResponseKeys
        {

            public const string NOT_IN_THE_PAST_DATE = "NOT_IN_THE_PAST_DATE";
            public const string MISSING_DAY_OF_MONTH = "MISSING_DAY_OF_MONTH";
            public const string DEPARTURE_BEFORE_ARRIVAL = "DEPARTURE_BEFORE_ARRIVAL";
        }
    }
}
