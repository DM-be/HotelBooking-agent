using System;
using HotelBot.Dialogs.Prompts.DepartureDate.Resources;
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
                        MessageFactory.Text(
                            DepartureDateStrings.DEPARTURE_DATE_PROMPT,
                            DepartureDateStrings.DEPARTURE_DATE_PROMPT,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.RetryDepartureDatePrompt, (context, data) =>
                        GenerateRandomRetryResponse()
                },
                {
                    ResponseIds.HaveDepartureDate, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(DepartureDateStrings.HAVE_DEPARTURE_DATE, data),
                            ssml: string.Format(DepartureDateStrings.HAVE_DEPARTURE_DATE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                {
                    ResponseIds.HaveUpdatedDepartureDate, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(DepartureDateStrings.HAVE_UPDATED_DEPARTURE_DATE, data),
                            ssml: string.Format(DepartureDateStrings.HAVE_UPDATED_DEPARTURE_DATE, data),
                            inputHint: InputHints.IgnoringInput)
                }
            }
        };


        public DepartureDateResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }


        // todo: only retry in validator responses?
        private static IMessageActivity GenerateRandomRetryResponse()
        {
            var rnd = new Random();
            var message = "";
            var randomNumber = rnd.Next(1, 3);
            switch (randomNumber)
            {
                case 1:
                    message = DepartureDateStrings.RETRY_DEPARTURE_PROMPT_1;
                    break;
                case 2:
                    message = DepartureDateStrings.RETRY_DEPARTURE_PROMPT_2;
                    break;
                case 3:
                    message = DepartureDateStrings.RETRY_DEPARTURE_PROMPT_3;
                    break;
            }

            return MessageFactory.Text(
                message,
                message,
                InputHints.AcceptingInput);
        }


        public class ResponseIds
        {
            public const string DepartureDatePrompt = "departureDatePrompt";
            public const string HaveDepartureDate = "haveDepartureDate";
            public const string HaveUpdatedDepartureDate = "haveUpdatedDepartureDate";

            public const string RetryDepartureDatePrompt = "retryDepartureDatePrompt";
        }
    }
}
