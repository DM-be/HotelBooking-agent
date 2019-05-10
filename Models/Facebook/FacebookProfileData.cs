namespace HotelBot.Models.Facebook
{
    public class FacebookProfileData
    {

        public string Id { get; set; } // user id from the bot state  todo: still needed? (maybe for azure cosmos)
        public string Name { get; set; }
        public string Locale { get; set; } // use to determine luis language - translations etc
    }
}
