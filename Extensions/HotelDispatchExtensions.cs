using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Models.LUIS;

namespace HotelBot.Extensions
{
    public static class HotelDispatchExtensions
    {
        public static string ConvertToQnAServiceName(this HotelDispatch.Intent qnaIntent)
        {
            var intentString = qnaIntent.ToString();
            return intentString.Substring(2);
        }

        public static bool IsQnAIntent(this HotelDispatch.Intent qnaIntent)
        {
            return qnaIntent.ToString().StartsWith("q_");
        }
    }
}
