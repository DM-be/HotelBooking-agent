using System.Collections.Generic;

namespace HotelBot.StateProperties
{
    public class ConversationData
    {

        public ConversationData()
        {
            StateObjects = new Dictionary<string, object>();
        }

        // used to determine qna services per facebook page
        public string FacebookPageId { get; set; }

        // persistent dictionary in every state;
        public Dictionary<string, object> StateObjects { get; set; }
    }
}
