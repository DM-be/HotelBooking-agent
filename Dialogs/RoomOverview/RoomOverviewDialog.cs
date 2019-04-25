using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.RoomDetail;
using HotelBot.Dialogs.Shared.CustomDialog;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Models.DTO;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.RoomOverview
{

    public class RoomOverviewDialog: RoomOverviewRecognizerDialog
    {

        private static RoomOverviewResponses _responder =  new RoomOverviewResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();
        private readonly BotServices _services;


        public RoomOverviewDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomOverviewDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(RoomOverviewDialog);


            // send overview
            // send prompt asking to modify, update or confirm  (also implement in luis)
            // when confirmed --> send link to do "payment" --> no sql set backend validated boolean to true after payment via api? 
            var sendOverview = new WaterfallStep []
            {
                FetchSelectedRoomDetail, ShowOverview, PromptModify
            };
            AddDialog(new WaterfallDialog(InitialDialogId, sendOverview));


        }


        public async Task<DialogTurnResult> FetchSelectedRoomDetail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var dialogOptions = sc.Options as DialogOptions;
            var requestHandler = new RequestHandler();
            var state = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());

            //check on rerouted --> avoid replace dialog sending another get request 
            if (dialogOptions.Rerouted)
            {
      

                var roomDetailDto = await requestHandler.FetchRoomDetail(dialogOptions.RoomAction.Id);
                var selectedRate = dialogOptions.RoomAction.SelectedRate;

                var selectedRoom = new SelectedRoom
                {
                    RoomDetailDto = roomDetailDto,
                    SelectedRate = selectedRate
                };
                state.SelectedRooms.Add(selectedRoom);

                await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.ShowOverview, state);
      
            }

            return await sc.NextAsync();
        }




        public async Task<DialogTurnResult> ShowOverview(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            // check if room still available --> a get request for the room (backend returns null if not available)

            return null;
        }

        public async Task<DialogTurnResult> PromptModify(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            return null;

        }
    }
}
