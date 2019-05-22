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


        public class ResponseIds
        {
            public const string NumberOfPeoplePrompt = "numberOfPeoplePrompt";
            public const string HaveNumberOfPeople = "haveNumberOfPeople";
            public const string HaveUpdatedNumberOfPeople = "haveUpdatedNumberOfPeople";
        }
    }
}
