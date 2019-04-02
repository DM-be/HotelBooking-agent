using System;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.StateAccessors
{
    public class StateBotAccessors
    {
        // accessors for converstation and userstate
        public StateBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public static string UserProfileName { get; } = "UserProfile";
        public static string ConversationDataName { get; } = "ConversationData";
        public static string DialogStateName { get; } = "DialogState";
        public static string FetchAvailableRoomsName { get; } = "FetchAvailableRoomsState";

        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }

        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public IStatePropertyAccessor<FetchAvailableRoomsState> FetchAvailableRoomsStateAccessor { get; set; }
    
        public ConversationState ConversationState { get; }
        public UserState UserState { get; }

    }
}
