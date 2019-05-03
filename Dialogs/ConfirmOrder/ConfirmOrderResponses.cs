using HotelBot.Dialogs.ConfirmOrder.Resources;
using HotelBot.Dialogs.Prompts.NumberOfPeople.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.ConfirmOrder
{
    public class ConfirmOrderResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                
            }
        };

        public ConfirmOrderResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string UseFacebookName = "useFacebookName";
        }


    }

}


