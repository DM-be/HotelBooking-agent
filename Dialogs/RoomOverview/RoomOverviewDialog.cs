using System;
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

            if (dialogOptions.RoomAction != null)
            {
                //todo: add alternate flow if room is unavailable (could be an action button tapped from hours/days before)
                var roomDetailDto = await requestHandler.FetchRoomDetail(dialogOptions.RoomAction.Id);
                var selectedRate = dialogOptions.RoomAction.SelectedRate;
                var selectedRoom = new SelectedRoom
                {
                    RoomDetailDto = roomDetailDto,
                    SelectedRate = selectedRate
                };
                state.SelectedRooms.Add(selectedRoom);
                await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.RoomAdded, state);
                var succes = true;  //todo: think about alternate flow and this var to skip the prompt
                return await sc.NextAsync(succes);
            }

            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> PromptContinueOrFindMoreRooms(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            if (sc.Result != null) return await sc.BeginDialogAsync(nameof(ContinueOrAddMoreRoomsPrompt));
            return await sc.NextAsync();
        }
        public async Task<DialogTurnResult> ProcessResultContinueOrAddMoreRoomsPrompt(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
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
                        var dialogResult = new DialogResult
                        {
                            PreviousOptions = dialogOptions,
                            TargetDialog = nameof(FetchAvailableRoomsDialog)

                        };
                        return await sc.EndDialogAsync(dialogResult);

                }
            }

            return await sc.NextAsync();

        }







        public async Task<DialogTurnResult> ShowOverview(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            var state = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.ShowOverview, state);
            return EndOfTurn;


        }

        public class RoomOverviewChoices
        {
            public const string AddAnotherRoom = "Add another room";
            public const string ContinueToPayment = "Continue to payment"; // maybe confirm instead of payment 
        }
    }
}
