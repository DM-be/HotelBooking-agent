namespace HotelBot.Models.DTO
{

    /// <summary>
    ///     dummy class containing all data of a room from a fake backend
    /// </summary>
    public class RoomDto
    {
        public string Title { get; set; } // 4-bed dorm...
        public string Description { get; set; } // Standaard 2-persoonskamer. Kingsize bed, wifi, tv, ..
        public int StartingPrice { get; set; }

        public RoomImage Thumbnail { get; set; }
    }

}
