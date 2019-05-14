using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.ConfirmOrder;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.ContinueOrAddMoreRooms;
using HotelBot.Dialogs.Shared.CustomDialog;
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
        private readonly BotServices _services; //todo: services still needed? 


        public RoomOverviewDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomOverviewDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(RoomOverviewDialog);
            var roomOverviewWaterfallSteps = new WaterfallStep []
            {
                FetchSelectedRoomDetailAndAddToState, PromptContinueOrFindMoreRooms, ProcessResultContinueOrAddMoreRoomsPrompt,
            };
            AddDialog(new WaterfallDialog(InitialDialogId, roomOverviewWaterfallSteps));
            AddDialog(new ContinueOrAddMoreRoomsPrompt(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }


        // check roomaction and add room to state if it is not null
        public async Task<DialogTurnResult> FetchSelectedRoomDetailAndAddToState(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var dialogOptions = sc.Options as DialogOptions;
            var state = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            

            if (dialogOptions != null && dialogOptions.RoomAction != null)
            {
                if (dialogOptions.RoomAction.Action != null && dialogOptions.RoomAction.Action == RoomAction.Actions.SelectRoomWithRate)
                    await AddRoom(state, dialogOptions, sc);

                if (dialogOptions.RoomAction.Action != null && dialogOptions.RoomAction.Action == RoomAction.Actions.Remove)
                    await RemoveRoom(state, dialogOptions, sc);
            }

            return await sc.NextAsync();

        }

        public async Task<DialogTurnResult> PromptContinueOrFindMoreRooms(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            var roomOverviewState = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            var confirmOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            return await sc.BeginDialogAsync(nameof(ContinueOrAddMoreRoomsPrompt));

        }

        public async Task<DialogTurnResult> ProcessResultContinueOrAddMoreRoomsPrompt(WaterfallStepContext sc, CancellationToken cancellationToken)

        {

            if (sc.Result != null && sc.Result.GetType() == typeof(bool)) return await sc.EndDialogAsync();
            var dialogOptions = new DialogOptions();
            if (sc.Options != null) dialogOptions = (DialogOptions) sc.Options;
            if (sc.Result != null)
            {
                var choice = sc.Result as FoundChoice;
                switch (choice.Value)

                {
                    case RoomOverviewChoices.NoThankyou:

                        return await sc.PromptAsync(
                            nameof(ChoicePrompt),
                            new PromptOptions
                            {
                                Prompt = await _responder.RenderTemplate(
                                    sc.Context,
                                    sc.Context.Activity.Locale,
                                    RoomOverviewResponses.ResponseIds.UnconfirmedPayment),
                                Choices = ChoiceFactory.ToChoices(
                                    new List<string>
                                    {
                                        RoomOverviewChoices.Confirm,
                                        RoomOverviewChoices.Cancel

                                    })
                            });
                    case "Cancel booking":
                        return await sc.EndDialogAsync();
                    case RoomOverviewChoices.AddARoom:
                    case RoomOverviewChoices.FindRoom:
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



        private async Task AddRoom(RoomOverviewState state, DialogOptions dialogOptions, WaterfallStepContext sc)
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

        private async Task RemoveRoom(RoomOverviewState state, DialogOptions dialogOptions, WaterfallStepContext sc)
        {
            var roomId = dialogOptions.RoomAction.RoomId;
            var selectedRate = dialogOptions.RoomAction.SelectedRate.Price;
            // todo: implement better removal... 
            var toRemove = state.SelectedRooms.Where(x => x.RoomDetailDto.Id == roomId && x.SelectedRate.Price == selectedRate);
            if (toRemove.Count() > 0)
            {
                state.SelectedRooms.Remove(toRemove.First());
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
            public const string CancelRoom = "Cancel room";

            public static readonly ReadOnlyCollection<string> Choices =
                new ReadOnlyCollection<string>(
                    new []
                    {
                        AddARoom, FindRoom, NoThankyou, Cancel, Confirm, CancelRoom
                    });
        }
    }
}
