using HotelBot.Models.LUIS;

namespace HotelBot.Models.Wrappers
{
    public class DialogOptions
    {
        public bool SkipConfirmation { get; set; }
        public RoomAction RoomAction { get; set; }

        // todo: refactor into dictionary when to many properties?
        public bool UpdatedNumberOfPeople { get; set; }

        public bool UpdatedArrivalDate { get; set; }

        public bool UpdatedLeavingDate { get; set; }

        public HotelBotLuis LuisResult { get; set; }
    }
}
