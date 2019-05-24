
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using HotelBot.Dialogs.Prompts.RoomDetailChoices.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Prompts.RoomDetailChoices
{
    public class RoomDetailChoicesResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                 {
                    ResponseIds.GetRandomContinuePrompt, (context, data) =>
                        GenerateRandomContinuePromptMessage()
                },


            }
        };


        private static IMessageActivity GenerateRandomContinuePromptMessage()
        {

            var mainStrings = RoomDetailChoicesStrings.ResourceManager;
            var resourceSet = mainStrings.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
            IDictionaryEnumerator id = resourceSet.GetEnumerator();
            List<dynamic> randomContinueResponses = new List<dynamic>();
            while (id.MoveNext())
            {
                if (id.Key.ToString().StartsWith(ResponseKeys.RANDOM_PROMPT))
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


        public RoomDetailChoicesResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string GetRandomContinuePrompt = "getRandomContinuePrompt";
        }

        public class ResponseKeys {
            public const string RANDOM_PROMPT = "RANDOM_PROMPT";
        } 

    }
}
