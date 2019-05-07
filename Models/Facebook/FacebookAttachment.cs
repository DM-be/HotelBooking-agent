using Newtonsoft.Json;

namespace HotelBot.Models.Facebook
{
    public class FacebookAttachment
    {
        public const string Location = "location";

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("payload")] public FacebookPayload FacebookPayload { get; set; }

   
    }
}
