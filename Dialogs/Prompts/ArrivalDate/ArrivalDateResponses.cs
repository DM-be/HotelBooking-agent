using System;
using HotelBot.Dialogs.Prompts.ArrivalDate.Resources;
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
                        MessageFactory.Text(
                            ArrivalDateStrings.ARRIVAL_DATE_PROMPT,
                            ArrivalDateStrings.ARRIVAL_DATE_PROMPT,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.RetryArrivalDatePrompt, (context, data) =>
                        GenerateRandomRetryResponse()
                },
                {
                    ResponseIds.HaveArrivalDate, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(ArrivalDateStrings.HAVE_ARRIVAL_DATE, data),
                            ssml: string.Format(ArrivalDateStrings.HAVE_ARRIVAL_DATE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                {
                    ResponseIds.HaveUpdatedArrivalDate, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(ArrivalDateStrings.HAVE_UPDATED_ARRIVAL_DATE, data),
                            ssml: string.Format(ArrivalDateStrings.HAVE_UPDATED_ARRIVAL_DATE, data),
                            inputHint: InputHints.IgnoringInput)
                }
            }
        };


        public ArrivalDateResponses()
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
                    message = ArrivalDateStrings.RETRY_ARRIVAL_DATE_PROMPT_1;
                    break;
                case 2:
                    message = ArrivalDateStrings.RETRY_ARRIVAL_DATE_PROMPT_2;
                    break;
                case 3:
                    message = ArrivalDateStrings.RETRY_ARRIVAL_DATE_PROMPT_3;
                    break;
            }

            return MessageFactory.Text(
                message,
                message,
                InputHints.AcceptingInput);
        }


        public class ResponseIds
        {
            public const string ArrivalDatePrompt = "arrivalDatePrompt";
            public const string HaveArrivalDate = "haveArrivalDate";
            public const string HaveUpdatedArrivalDate = "haveUpdatedArrivalDate";

            public const string RetryArrivalDatePrompt = "retryArrivalDatePrompt";
        }
    }
}
