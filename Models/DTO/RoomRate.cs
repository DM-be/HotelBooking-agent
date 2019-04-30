using System;

namespace HotelBot.Models.DTO
{
    [Serializable]
    public class RoomRate
    {

        public string Id { get; set; }
        public string RateName { get; set; }
        public int Price { get; set; }
        public string RateDescription { get; set; }
    }
}
