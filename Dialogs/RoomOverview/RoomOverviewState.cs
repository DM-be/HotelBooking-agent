using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Models.DTO;

namespace HotelBot.Dialogs.RoomOverview
{
    public class RoomOverviewState
    {
        public int Id { get; set; }
        public bool Confirmed { get; set; } = false; // paid for? use in cosmos db --> allow or dissallow updating booking objects 
        public List<RoomDetailDto> Rooms { get; set; }

        // add extra properties like the name of booker - selected checkin time. 
        // userprofile property
    }
}
