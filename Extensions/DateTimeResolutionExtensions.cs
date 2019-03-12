using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Extensions
{
    public static class DateTimeResolutionExtensions
    {
        public static TimexProperty ConvertToTimex(this DateTimeResolution dateTimeResolution)
        {
            return new TimexProperty(dateTimeResolution.Timex);
        }
    }
}
