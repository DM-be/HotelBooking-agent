using HotelBot.Models.Facebook;

namespace HotelBot.StateProperties
{
    public class UserProfile
    {


        public FacebookProfileData FacebookProfileData { get; set; } // contains name etc
        public bool SendFetchAvailableRoomsIntroduction { get; set; } = true; // send introduction only on first time seeing the convo
 


    }
}
