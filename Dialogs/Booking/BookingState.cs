using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Models.DTO;

namespace HotelBot.Dialogs.Booking
{
    public class BookingState
    {
        public List<RoomDetailDto> RoomDetailDto { get; set; }
        public RoomRate SelectedRoomRate { get; set; }
        

    }
}
