using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HotelBot.Shared.Helpers;
using Luis;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DateTime;

namespace HotelBot.Extensions
{
    public static class HotelBotLuisExtensions
    {
        public static bool HasEntityWithPropertyName(this HotelBotLuis luisResult, string propertyName)
        {
            if (luisResult == null)
            {
                throw new ArgumentNullException(nameof(luisResult));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var GetDynamicProperty = TypeUtility<HotelBotLuis._Entities>.GetMemberGetDelegate<dynamic>(propertyName);
            var dynamicResult = GetDynamicProperty(luisResult.Entities);
            if (dynamicResult != null)
            {
                return true;
            }
            return false;

        }

        public static bool ConvertDateTimeSpec(this HotelBotLuis luisResult, DateTimeSpec spec)
        {


            var test = spec;
            return false;

        }


    }
}

