using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HotelBot.Models.Facebook
{
    public class FacebookReceipt
    {

        [JsonProperty("message")]
        public FacebookMessage FacebookMessage { get; set; }


        [JsonProperty("recipient")]

        public  FacebookRecipient FacebookRecipient { get; set; }

    }
}
