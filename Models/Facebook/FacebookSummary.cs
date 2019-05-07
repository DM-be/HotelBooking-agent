using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HotelBot.Models.Facebook
{
    public class FacebookSummary
    {
        [JsonProperty("subtotal")] public double Subtotal { get; set; }

        [JsonProperty("total_cost")]
        public double TotalCost { get; set; }

        [JsonProperty("total_tax")]
        public double TotalTax { get; set; }

    }
}
