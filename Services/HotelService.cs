using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.Services
{

    
    // temporary backend service that provides some fake data for now.
    public class HotelService
    {

        // You don't need a Google API key to use Maps URLs.
        // lat first
        public static string GetMapsUrl(string latitude, string longitude)
        {
            return
                $"https://www.google.com/maps/dir/?api=1&origin={latitude},{longitude}&destination=51.228557,3.231737";
        }


    }








}
