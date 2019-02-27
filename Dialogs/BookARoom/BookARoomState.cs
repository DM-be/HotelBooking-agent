using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomState
    {
        public string Email { get; set; }
        public int NumberOfPeople { get; set; }

        // todo: correct format based on timex
        public string ArrivalDate { get; set; }

        public string LeavingDate { get; set; }

    }
}
