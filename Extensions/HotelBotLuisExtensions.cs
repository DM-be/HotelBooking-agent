using System;
using HotelBot.Shared.Helpers;
using Luis;

namespace HotelBot.Extensions
{
    public static class HotelBotLuisExtensions
    {
        public static bool HasEntityWithPropertyName(this HotelBotLuis luisResult, string propertyName)
        {
            if (luisResult == null) throw new ArgumentNullException(nameof(luisResult));

            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            var GetDynamicProperty = TypeUtility<HotelBotLuis._Entities>.GetMemberGetDelegate<dynamic>(propertyName);
            var dynamicResult = GetDynamicProperty(luisResult.Entities);
            if (dynamicResult != null) return true;
            return false;

        }
    }
}
