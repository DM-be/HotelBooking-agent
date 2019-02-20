using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.StateProperties
{
    public class ConversationData
    {
        // The conversation channel ---> on facebook "facebook" etc
        public string ChannelId { get; set; }
        // could be null when channelId not facebook
        public string FacebookPageName { get; set; }

        public string ProfilePageId { get; set; } 
    }
}
