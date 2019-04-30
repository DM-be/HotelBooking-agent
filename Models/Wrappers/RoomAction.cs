using HotelBot.Models.DTO;

namespace HotelBot.Models.Wrappers
{
    public class RoomAction
    {
        public string RoomId { get; set; }
        public string Action { get; set; }

        public RoomRate SelectedRate { get; set; }

    }
}
