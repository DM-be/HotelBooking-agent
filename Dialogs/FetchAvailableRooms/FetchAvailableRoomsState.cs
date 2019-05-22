
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;

namespace HotelBot.Dialogs.FetchAvailableRooms
{
    public class FetchAvailableRoomsState
    {

        public string Email { get; set; }
        public double? NumberOfPeople { get; set; }
        public TimexProperty ArrivalDate { get; set; }
        public TimexProperty LeavingDate { get; set; }

        public TimexProperty TempTimexProperty { get; set; } // used in delegates --> stores this in arrival/leaving

        public bool IsComplete()
        {
            return ArrivalDate != null && LeavingDate != null && NumberOfPeople != null;
        }

        public bool TempTimexPropertyBeforeArrival() {

            if (ArrivalDate != null && TempTimexProperty != null)
            {
                var arrivalDateAsDateTime = new DateTime(2019, ArrivalDate.Month.Value, ArrivalDate.DayOfMonth.Value);
                var tempTimexPropertyAsDateTime = new DateTime(2019, TempTimexProperty.Month.Value, TempTimexProperty.DayOfMonth.Value);
                return DateTime.Compare(tempTimexPropertyAsDateTime, arrivalDateAsDateTime) < 0;
            }
            return false;

        }
    }
}
