using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.ArrivalDate.Resources;
using HotelBot.Models.DTO;
using HotelBot.Models.Wrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace HotelBot.Dialogs.Prompts.RoomDetailChoices
{
    public class RoomDetailChoicesResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                


            }
        };


        public RoomDetailChoicesResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {

        }

    }
}
