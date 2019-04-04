using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.Models.DTO
{
    public class RoomBookingObject
    {
        public RoomDetailDto SelectedRoom { get; set; }
        public RoomRate SelectedRate { get; set; }
    }
}
