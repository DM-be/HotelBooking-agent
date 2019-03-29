using Newtonsoft.Json;

namespace HotelBot.Models.Facebook
{
    public class FacebookImageTemplate
    {
        [JsonProperty("image_url")] public string ImageUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
