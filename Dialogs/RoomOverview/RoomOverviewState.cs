using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Models.DTO;

namespace HotelBot.Dialogs.RoomOverview
{
    public class RoomOverviewState
    {
        public List<SelectedRoom> SelectedRooms { get; set; } = new List<SelectedRoom>();

    }
}
