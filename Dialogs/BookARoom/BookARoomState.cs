using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomState
    {
        public string Email { get; set; }
        public double? NumberOfPeople { get; set; }

        // todo: correct format based on needs
        public DateTimeResolution ArrivalDate { get; set; }

        public DateTimeResolution LeavingDate { get; set; }

    }
}
