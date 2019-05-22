
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
            return this.ArrivalDate != null && this.LeavingDate != null && this.NumberOfPeople != null;
        }

        public bool DepartureBeforeArrival() {

            if (ArrivalDate != null && LeavingDate != null)
            {
                var arrivalDateAsDateTime = new DateTime(2019, ArrivalDate.Month.Value, ArrivalDate.DayOfMonth.Value);
                var departureDateAsDateTime = new DateTime(2019, LeavingDate.Month.Value, LeavingDate.DayOfMonth.Value);
                return DateTime.Compare(departureDateAsDateTime, arrivalDateAsDateTime) > 0;
            }
            return false;

        }
    }
}
