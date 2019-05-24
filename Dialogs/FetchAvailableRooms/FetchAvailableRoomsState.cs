
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

        public TimexProperty TempTimexProperty { get; set; }
        // a placeholder for the LUIS recognized timexproperty or result from a datetimeprompt
        // arrival/leaving will only be updated after confirmation

        public bool IsComplete()
        {
            return ArrivalDate != null && LeavingDate != null && NumberOfPeople != null;
        }

        public bool TempTimexPropertyBeforeArrival() {

            if (ArrivalDate != null && TempTimexProperty != null)
            {
                var arrivalDateAsDateTime = new DateTime(DateTime.Now.Year, ArrivalDate.Month.Value, ArrivalDate.DayOfMonth.Value);
                var tempTimexPropertyAsDateTime = new DateTime(DateTime.Now.Year, TempTimexProperty.Month.Value, TempTimexProperty.DayOfMonth.Value);
                return DateTime.Compare(tempTimexPropertyAsDateTime, arrivalDateAsDateTime) < 0;
            }
            return false;

        }
    }
}
