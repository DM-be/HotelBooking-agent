using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.ConfirmOrder;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Main;
using HotelBot.Dialogs.Prompts.ContinueOrAddMoreRooms;
using HotelBot.Dialogs.Shared.CustomDialog;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.RoomOverview
{

    public class RoomOverviewDialog: RoomOverviewRecognizerDialog
    {

        private static readonly RoomOverviewResponses _responder = new RoomOverviewResponses();
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services; //todo: services still needed? 


        public RoomOverviewDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomOverviewDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(RoomOverviewDialog);
            var roomOverviewWaterfallSteps = new WaterfallStep []
            {
                FetchSelectedRoomDetailAndAddToStateAsync, PromptContinueOrFindMoreRoomsAsync, ProcessResultContinueOrAddMoreRoomsPromptAsync,
            };
            AddDialog(new WaterfallDialog(InitialDialogId, roomOverviewWaterfallSteps));
            AddDialog(new ContinueOrAddMoreRoomsPrompt(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }


        // check roomaction and add room to state if it is not null
        public async Task<DialogTurnResult> FetchSelectedRoomDetailAndAddToStateAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var dialogOptions = sc.Options as DialogOptions;
            var state = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            

            if (dialogOptions != null && dialogOptions.RoomAction != null)
            {
                if (dialogOptions.RoomAction.Action != null && dialogOptions.RoomAction.Action == RoomAction.Actions.SelectRoomWithRate)
                    await AddRoomAsync(state, dialogOptions, sc);

                if (dialogOptions.RoomAction.Action != null && dialogOptions.RoomAction.Action == RoomAction.Actions.Remove)
                    await RemoveRoomAsync(state, dialogOptions, sc);
            }

            return await sc.NextAsync();

        }

        public async Task<DialogTurnResult> PromptContinueOrFindMoreRoomsAsync(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            return await sc.BeginDialogAsync(nameof(ContinueOrAddMoreRoomsPrompt));
        }

        public async Task<DialogTurnResult> ProcessResultContinueOrAddMoreRoomsPromptAsync(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            


            if (sc.Result != null && sc.Result.GetType() == typeof(bool)) return await sc.EndDialogAsync();
            var dialogOptions = new DialogOptions();
            if (sc.Options != null) dialogOptions = (DialogOptions) sc.Options;
            if (sc.Result != null)
            {
                var choice = sc.Result as FoundChoice;
                switch (choice.Value)

                {
                    case RoomOverviewChoices.CancelBooking:
                        await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.NotSupported);
                        var emptyConfirmOrderState = new ConfirmOrderState();
                        var emptyRoomOverViewState = new RoomOverviewState();
                        await _accessors.RoomOverviewStateAccessor.SetAsync(sc.Context, emptyRoomOverViewState);
                        await _accessors.ConfirmOrderStateAccessor.SetAsync(sc.Context, emptyConfirmOrderState);
                        return await sc.ReplaceDialogAsync(InitialDialogId);
                    case RoomOverviewChoices.AddARoom:
                    case RoomOverviewChoices.FindRoom:
                        var dialogResult = new DialogResult
                        {
                            PreviousOptions = dialogOptions,
                            TargetDialog = nameof(FetchAvailableRoomsDialog)

                        };
                        return await sc.EndDialogAsync(dialogResult);
                    case RoomOverviewChoices.Receipt:
                        var confirmOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
                        var userProfile = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());
                        var data = new dynamic[2];
                        data[0] = confirmOrderState;
                        data[1] = userProfile;
                        var mainResponses = new MainResponses();
                        await mainResponses.ReplyWith(sc.Context, MainResponses.ResponseIds.SendReceipt, data);
                        return await sc.EndDialogAsync();
                            
                }
            }

            return await sc.NextAsync();

        }



        private async Task AddRoomAsync(RoomOverviewState state, DialogOptions dialogOptions, WaterfallStepContext sc)
        {
            var requestHandler = new RequestHandler();
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

        public async Task RemoveRoomAsync(RoomOverviewState state, DialogOptions dialogOptions, WaterfallStepContext sc)
        {
            var roomId = dialogOptions.RoomAction.RoomId;
            var selectedRate = dialogOptions.RoomAction.SelectedRate.Price;
            // todo: implement better removal... 

            if (state.SelectedRooms != null)
            {
                var toRemove = state.SelectedRooms.Where(x => x.RoomDetailDto.Id == roomId && x.SelectedRate.Price == selectedRate);
                if (toRemove.Count() > 0)
                {
                    state.SelectedRooms.Remove(toRemove.First());
                    await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.RoomRemoved);
                }
            }
            else {
                await _responder.ReplyWith(sc.Context, RoomOverviewResponses.ResponseIds.RoomRemoved);
            }

        }






        public class RoomOverviewChoices
        {
            public const string AddARoom = "Add a room";
            public const string FindRoom = "Find a room";
            public const string NoThankyou = "No thank you";
            public const string Cancel = "Cancel";
            public const string Confirm = "Confirm";
            public const string CancelBooking = "Cancel booking";
            public const string Receipt = "Receipt";

            public static readonly ReadOnlyCollection<string> Choices =
                new ReadOnlyCollection<string>(
                    new []
                    {
                        AddARoom, FindRoom, NoThankyou, Cancel, Confirm
                    });
        }
    }
}
