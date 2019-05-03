using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.RoomDetail;
using Microsoft.Bot.Builder.TemplateManager;

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
            
        }
    }
}
