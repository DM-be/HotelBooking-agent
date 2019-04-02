using System.Collections.Generic;
using HotelBot.Models.LUIS;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.FetchAvailableRooms
{
    public class FetchAvailableRoomsState
    {

        public FetchAvailableRoomsState()
        {
            LuisResults = new Dictionary<string, HotelBotLuis>();
            TimexResults = new Dictionary<string, TimexProperty>();
        }

        public string Email { get; set; }
        public double? NumberOfPeople { get; set; }
        public TimexProperty ArrivalDate { get; set; }
        public TimexProperty LeavingDate { get; set; }


        // a dictionary holding temporary luisResults
        public Dictionary<string, HotelBotLuis> LuisResults { get; set; }

        // dictionary holding temporay timexproperties
        public Dictionary<string, TimexProperty> TimexResults { get; set; }
    }
}
