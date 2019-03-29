using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Shared.RecognizerDialogs;
using HotelBot.Dialogs.Shared.RecognizerDialogs.RoomDetail;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.RoomDetail
{
    public class RoomDetailDialog: RoomDetailRecognizerDialog
    {
        private readonly RoomDetailResponses _responder = new RoomDetailResponses();
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;


        public RoomDetailDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomDetailDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(RoomDetailDialog);

            var roomDetailWaterfallSteps = new WaterfallStep []
            {
                ReplyDetailsAndWait
            };

            AddDialog(new WaterfallDialog(InitialDialogId, roomDetailWaterfallSteps));


        }

        public async Task<DialogTurnResult> ReplyDetailsAndWait(WaterfallStepContext sc, CancellationToken cancellationToken)


        {
            var roomId = (string) sc.Options;
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, roomId);

            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
    }
}
