
using System.Linq;
using HotelBot.Models.Facebook;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace HotelBot.Extensions
{
    public static class ActivityExtensions
    {

        // TODO: remove isstartactivity when obselete
        public static bool IsStartActivity(this Activity activity)
        {
            switch (activity.ChannelId)
            {
                case Channels.Skype:
                {
                    if (activity.Type == ActivityTypes.ContactRelationUpdate && activity.Action == "add")
                    {
                        return true;
                    }

                    return false;
                }

                case Channels.Directline:
                case Channels.Emulator:
                case Channels.Webchat:
                case Channels.Msteams:
                {
                    if (activity.Type == ActivityTypes.ConversationUpdate)
                    {
                        // When bot is added to the conversation (triggers start only once per conversation)
                        if (activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                default:
                {
                    return false;
                }
            }
        }

        public static bool IsGetStartedPostBack(this Activity activity)
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
