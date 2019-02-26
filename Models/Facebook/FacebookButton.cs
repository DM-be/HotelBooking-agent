using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HotelBot.Models.Facebook
{
    public class FacebookButton
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }


        [JsonProperty("payload")]
        public string Payload { get; set; }
    }
}
