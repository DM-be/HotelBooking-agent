using HotelBot.Models.DTO;

namespace HotelBot.Models.Wrappers
{
    public class RoomAction
    {
        public string RoomId { get; set; }
        public string Action { get; set; }
        public RoomRate SelectedRate { get; set; }

        public class Actions
        {
            public const string Info = "info";
            public const string Book = "book";
            public const string Remove = "remove";
            public const string SelectRoomWithRate = "selectRoomWithRate";
            public const string Confirm = "confirm";
            public const string Paid = "paid"; // a fake paid callback 
            public const string ViewDetails = "viewDetails";
        }
    }



}
