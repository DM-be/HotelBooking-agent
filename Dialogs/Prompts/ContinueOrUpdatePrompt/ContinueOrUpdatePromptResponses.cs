using HotelBot.Dialogs.Prompts.ContinueOrUpdatePrompt.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using System.Collections;
using System.Collections.Generic;


namespace HotelBot.Dialogs.Prompts.ContinueOrUpdatePrompt
{
    public class ContinueOrUpdatePromptResponses: TemplateManager
    {

        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.SendRandomContinueOrUpdatePrompt, (context, data) =>
                        GenerateRandomContinueOrUpdateText()

                },
            }
        };


        public ContinueOrUpdatePromptResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        private static IMessageActivity GenerateRandomContinueOrUpdateText()
        {

            var mainStrings = ContinueOrUpdatePromptStrings.ResourceManager;
            var resourceSet = mainStrings.GetResourceSet(System.Threading.Thread.CurrentThread.CurrentCulture, true, true);
            IDictionaryEnumerator id = resourceSet.GetEnumerator();
            List<dynamic> randomContinueResponses = new List<dynamic>();
            while (id.MoveNext())
            {
                if (id.Key.ToString().StartsWith("RANDOM_CONTINUE"))
                {
                    var dyn = new
                    {
                        Key = id.Key.ToString(),
                        Value = id.Value.ToString()
                    };
                    randomContinueResponses.Add(dyn);
                }
            }
            System.Random random = new System.Random();
            var message = randomContinueResponses[random.Next(0, randomContinueResponses.Count)].Value;
            return MessageFactory.Text(message);


        }


        public class ResponseIds
        {
            public const string SendRandomContinueOrUpdatePrompt = "sendRandomContinueOrUpdatePrompt";
        }
    }

}
