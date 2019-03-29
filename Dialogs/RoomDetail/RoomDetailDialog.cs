using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Shared.RecognizerDialogs;
using HotelBot.Services;
using HotelBot.StateAccessors;

namespace HotelBot.Dialogs.RoomDetail
{
    public class RoomDetailDialog: BookARoomRecognizerDialog
    {
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public RoomDetailDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomDetailDialog))
        {
            
        }
    }
}
