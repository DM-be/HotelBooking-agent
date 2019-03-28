using System;
using HotelBot.Dialogs.Prompts.NumberOfPeople.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Prompts.NumberOfPeople
{
    public class NumberOfPeopleResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.NumberOfPeoplePrompt, (context, data) =>
                        MessageFactory.Text(
                            NumberOfPeopleStrings.NUMBER_OF_PEOPLE_PROMPT,
                            NumberOfPeopleStrings.NUMBER_OF_PEOPLE_PROMPT,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.RetryNumberOfPeoplePrompt, (context, data) =>
                        GenerateRandomRetryResponse()
                },
                {
                    ResponseIds.HaveNumberOfPeople, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(NumberOfPeopleStrings.HAVE_NUMBER_OF_PEOPLE, data),
                            ssml: string.Format(NumberOfPeopleStrings.HAVE_NUMBER_OF_PEOPLE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                {
                    ResponseIds.HaveUpdatedNumberOfPeople, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(NumberOfPeopleStrings.HAVE_UPDATED_NUMBER_OF_PEOPLE, data),
                            ssml: string.Format(NumberOfPeopleStrings.HAVE_UPDATED_NUMBER_OF_PEOPLE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                



            }
        };

        public NumberOfPeopleResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        private static IMessageActivity GenerateRandomRetryResponse()
        {
            var rnd = new Random();
            string message = "";
            int randomNumber = rnd.Next(1, 3);
            switch (randomNumber)
            {
                case 1:
                    message = NumberOfPeopleStrings.RETRY_NUMBER_OF_PEOPLE_PROMPT_1;
                    break;
                case 2:
                    message = NumberOfPeopleStrings.RETRY_NUMBER_OF_PEOPLE_PROMPT_2;
                    break;
                case 3:
                    message = NumberOfPeopleStrings.RETRY_NUMBER_OF_PEOPLE_PROMPT_3;
                    break;
            }

            return MessageFactory.Text(
                message,
                message,
                InputHints.AcceptingInput);
        }

        public class ResponseIds
        {
            public const string NumberOfPeoplePrompt = "NumberOfPeoplePrompt";
            public const string HaveNumberOfPeople = "HaveNumberOfPeople";
            public const string HaveUpdatedNumberOfPeople = "HaveUpdatedNumberOfPeople";

            public const string RetryNumberOfPeoplePrompt = "retryNumberOfPeoplePrompt";
        }
    }
}
