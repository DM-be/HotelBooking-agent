using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Models.DTO;

namespace HotelBot.Dialogs.RoomOverview
{
    public class SelectedRoom
    {
        public RoomDetailDto RoomDetailDto { get; set; }
        public RoomRate SelectedRate { get; set; }

        public int Total { get; set; }
    }
}
