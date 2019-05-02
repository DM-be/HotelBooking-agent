using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.ContinueOrAddMoreRooms;
using HotelBot.Dialogs.Shared.CustomDialog;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.RoomOverview
{

    public class RoomOverviewDialog: RoomOverviewRecognizerDialog
    {

        private static readonly RoomOverviewResponses _responder = new RoomOverviewResponses();
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
            var RoomOverviewWaterfallsteps = new WaterfallStep []
            {
                FetchSelectedRoomDetailAndAddToState, PromptContinueOrFindMoreRooms, ProcessResultContinueOrAddMoreRoomsPrompt, ShowOverview
            };
            AddDialog(new WaterfallDialog(InitialDialogId, RoomOverviewWaterfallsteps));
            AddDialog(new ContinueOrAddMoreRoomsPrompt(accessors));


        }


        // check roomaction and add room to state if it is not null
        public async Task<DialogTurnResult> FetchSelectedRoomDetailAndAddToState(WaterfallStepContext sc, CancellationToken cancellationToken)
        {


            var dialogOptions = sc.Options as DialogOptions;
            var requestHandler = new RequestHandler();
            var state = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());


            if (dialogOptions.RoomAction.Action != null && dialogOptions.RoomAction.Action == "selectRoomWithRate")
            {
                //todo: add alternate flow if room is unavailable (could be an action button tapped from hours/days before)
                var roomDetailDto = await requestHandler.FetchRoomDetail(dialogOptions.RoomAction.RoomId); // fetch to check availability
                var selectedRate = dialogOptions.RoomAction.SelectedRate;
                var selectedRoom = new SelectedRoom
                {
                    RoomDetailDto = roomDetailDto,
                    SelectedRate = selectedRate
                };
                state.SelectedRooms.Add(selectedRoom);
                await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.RoomAdded, state);
            }

            if (dialogOptions.RoomAction.Action != null && dialogOptions.RoomAction.Action == "remove")
            {
                var roomId = dialogOptions.RoomAction.RoomId;
                var selectedRate = dialogOptions.RoomAction.SelectedRate.Price;
                // todo: implement better removal... 
                var toRemove = state.SelectedRooms.Where(x => x.RoomDetailDto.Id == roomId && x.SelectedRate.Price == selectedRate).First();
                state.SelectedRooms.Remove(toRemove);
                await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.RoomRemoved);
            }


            if (dialogOptions.RoomAction.Action != null && dialogOptions.RoomAction.Action == "viewDetails")
            {
                var skipToOverview = true;

            }

            return await sc.NextAsync();

        }

        public async Task<DialogTurnResult> PromptContinueOrFindMoreRooms(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            var state = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            if (state.SelectedRooms.Count != 0)
            {
                await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.CompleteOverview, state);
            }



            return await sc.BeginDialogAsync(nameof(ContinueOrAddMoreRoomsPrompt));

        }



        public async Task<DialogTurnResult> ProcessResultContinueOrAddMoreRoomsPrompt(WaterfallStepContext sc, CancellationToken cancellationToken)

        {


            if (sc.Result != null && sc.Result.GetType() == typeof(bool)) return await sc.NextAsync();

            var dialogOptions = new DialogOptions();

            if (sc.Options != null) dialogOptions = (DialogOptions) sc.Options;

            if (sc.Result != null)
            {
                var choice = sc.Result as FoundChoice;
                switch (choice.Value)

                {
                    case RoomOverviewChoices.ContinueToPayment:
                        return await sc.NextAsync();
                    case RoomOverviewChoices.AddAnotherRoom:
                    case RoomOverviewChoices.FindRoom:
                        var dialogResult = new DialogResult
                        {
                            PreviousOptions = dialogOptions,
                            TargetDialog = nameof(FetchAvailableRoomsDialog)

                        };
                        return await sc.EndDialogAsync(dialogResult);
                    case RoomOverviewChoices.ShowOverview:
                        return await sc.ReplaceDialogAsync(InitialDialogId, null);
                    //todo: fix with replacedialogloop 


                }
            }

            return await sc.NextAsync();

        }







        public async Task<DialogTurnResult> ShowOverview(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            var state = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.DetailedRoomsOverview, state);
            return EndOfTurn;
            // switch on payment still needed and what else?


        }

        public class RoomOverviewChoices
        {
            public const string AddAnotherRoom = "Add another room";
            public const string FindRoom = "Find a room";
            public const string ContinueToPayment = "Continue to payment"; // maybe confirm instead of payment
            public const string ShowOverview = "Overview";
        }
    }
}
