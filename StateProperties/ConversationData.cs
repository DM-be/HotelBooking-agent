using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBot.StateProperties
{
    public class ConversationData
    {

        // used to determine qna services per facebook page
        public string FacebookPageId { get; set; }

        // persistent dictionary in every state;
        public Dictionary<string, object> StateObjects { get; set; }

        public ConversationData()
        {
            StateObjects = new Dictionary<string, object>();
        }
    }
}
