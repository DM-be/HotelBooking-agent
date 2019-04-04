using HotelBot.Models.LUIS;

namespace HotelBot.Models.Wrappers
{
    public class DialogOptions
    {
        public bool SkipConfirmation { get; set; }
        public bool Rerouted { get; set; }

        public HotelBotLuis LuisResult { get; set; }

    }
}
