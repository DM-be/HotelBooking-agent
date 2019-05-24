
using System.Linq;
using HotelBot.Models.Facebook;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace HotelBot.Extensions
{
    public static class ActivityExtensions
    {

    
        public static bool IsGettingStartedPostBack(this Activity activity)
        {
            var channelData = activity.ChannelData;
            var facebookPayload = (channelData as JObject)?.ToObject<FacebookPayload>();
            if (facebookPayload.PostBack != null)
            {
                if (facebookPayload.PostBack.Payload.Equals(FacebookPostback.GetStartedPostback))
                {
                    return true;
                }
            }
            return false;


        }
    }
}
