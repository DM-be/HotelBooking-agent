using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.Models.DTO
{
    public class RoomRate
    {
        public string RateName { get; set; }
        public int Price { get; set; }
        public string RateDescription { get; set; }
    }
}
