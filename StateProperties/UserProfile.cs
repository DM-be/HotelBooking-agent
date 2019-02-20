using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.StateProperties
{
    public class UserProfile
    {
        // user id from the bot state 
        public string Id { get; set; }

        public string Name { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }

        // use to determine luis language - translations etc
        public string Locale { get; set; }
    }
}
