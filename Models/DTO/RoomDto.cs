using Newtonsoft.Json;

namespace HotelBot.Models.DTO
{

    /// <summary>
    ///     dummy class containing all data of a room from a dummy backend made in firebase
    /// </summary>
    public class RoomDto
    {

        [JsonProperty("id")]
        public string id { get; set; }
        public string Title { get; set; } // 4-bed dorm...
        public string Description { get; set; } // Standaard 2-persoonskamer. Kingsize bed, wifi, tv, ..
        public int StartingPrice { get; set; }

        public RoomImage Thumbnail { get; set; }
    }

}
