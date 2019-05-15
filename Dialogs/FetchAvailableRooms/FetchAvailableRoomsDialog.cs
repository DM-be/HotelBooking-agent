using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.ConfirmFetchRooms;
using HotelBot.Dialogs.Prompts.ContinueOrUpdatePrompt;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.FetchAvailableRoomsIntroduction;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
using HotelBot.Dialogs.Prompts.UpdateStateChoice;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Dialogs.Shared.RecognizerDialogs.FetchAvailableRooms;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.FetchAvailableRooms
{
    public class FetchAvailableRoomsDialog: FetchAvailableRoomsRecognizerDialog
    {
        private static FetchAvailableRoomsResponses _responder;
        private readonly StateBotAccessors _accessors;
        private const string CachedStateKey = "cachedState";

        public FetchAvailableRoomsDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(FetchAvailableRoomsDialog))
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _responder = new FetchAvailableRoomsResponses();
            InitialDialogId = nameof(FetchAvailableRoomsDialog);
            var fetchAvailableRoomsWaterfallSteps = new WaterfallStep []
            {
                SendOptionalIntroduction, AskForNumberOfPeople, AskForArrivalDate, AskForLeavingDate, PromptFetchRoomsConfirmationPrompt,
                ProcessFetchRoomsConfirmationPrompt, RespondToContinueOrUpdate, RespondToNewRequest
            };

            AddDialog(new WaterfallDialog(InitialDialogId, fetchAvailableRoomsWaterfallSteps));
            AddDialog(new ArrivalDatePromptDialog(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmFetchRoomsPrompt(accessors));
            AddDialog(new DepartureDatePromptDialog(accessors));
            AddDialog(new NumberOfPeoplePromptDialog(accessors));
            AddDialog(new UpdateStateChoicePrompt(accessors));
            AddDialog(new ContinueOrUpdatePrompt(accessors));
            AddDialog(new FetchAvailableRoomsIntroductionPrompt());
            AddDialog(new RoomOverviewDialog(services, accessors));
        }


        public async Task<DialogTurnResult> SendOptionalIntroduction(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());
            if (userProfile != null && userProfile.SendFetchAvailableRoomsIntroduction)
            {
                userProfile.SendFetchAvailableRoomsIntroduction = false;
                return await sc.BeginDialogAsync(nameof(FetchAvailableRoomsIntroductionPrompt));
            }

            return await sc.NextAsync();

        }

        public async Task<DialogTurnResult> AskForNumberOfPeople(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.NumberOfPeople != null)
            {
                sc.Context.TurnState[CachedStateKey] = state;
                return await sc.NextAsync();
            }


            return await sc.BeginDialogAsync(nameof(NumberOfPeoplePromptDialog));
        }

        public async Task<DialogTurnResult> AskForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.ArrivalDate != null)
            {
                sc.Context.TurnState[CachedStateKey] = state;
                return await sc.NextAsync();
            }

            return await sc.BeginDialogAsync(nameof(ArrivalDatePromptDialog));
        }

        public async Task<DialogTurnResult> AskForLeavingDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.LeavingDate != null)
            {
                sc.Context.TurnState[CachedStateKey] = state;
                return await sc.NextAsync();
            }

            return await sc.BeginDialogAsync(nameof(DepartureDatePromptDialog));
        }

        public async Task<DialogTurnResult> PromptFetchRoomsConfirmationPrompt(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            DialogOptions dialogOptions = null;
            if (sc.Options != null)
            {
                dialogOptions = (DialogOptions) sc.Options;
                if (dialogOptions.SkipConfirmation) return await sc.NextAsync(dialogOptions.SkipConfirmation);
            }

            return await sc.BeginDialogAsync(nameof(ConfirmFetchRoomsPrompt), dialogOptions);
        }

        public async Task<DialogTurnResult> ProcessFetchRoomsConfirmationPrompt(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var confirmed = (bool) sc.Result;
            if (confirmed)
            {
                // send book a room cards and prompt to continue or update
                var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
                await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendRoomsCarousel, state);
                return await sc.BeginDialogAsync(nameof(ContinueOrUpdatePrompt));
            }

            // when a user taps no, we can assume he wants to change a value
            var foundChoice = new FoundChoice
            {
                Value = FetchAvailableRoomsChoices.UpdateSearch
            };
            return await sc.NextAsync(foundChoice);
        }

        public async Task<DialogTurnResult> RespondToContinueOrUpdate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            if (sc.Result != null)
            {
                var choice = sc.Result as FoundChoice;
                switch (choice.Value)
                {
                    case FetchAvailableRoomsChoices.UpdateSearch:
                        return await sc.BeginDialogAsync(nameof(UpdateStateChoicePrompt), sc.Options);
                    case FetchAvailableRoomsChoices.StartOver:
                    {
                        var emptyState = new FetchAvailableRoomsState();
                        _accessors.FetchAvailableRoomsStateAccessor.SetAsync(sc.Context, emptyState);
                        await sc.Context.SendActivityAsync("Ok, let's start over"); // in responder
                        var dialogOptions = new DialogOptions
                        {
                            SkipConfirmation = false // start over and prompt for confirmation again 
                        };
                        return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                    }
                    case FetchAvailableRoomsChoices.NoThanks:
                        await sc.Context.SendActivityAsync("You're welcome."); // in responder
                        return await sc.EndDialogAsync();
                    case FetchAvailableRoomsChoices.RoomOverview:
                        var dialogResult = new DialogResult
                        {
                            TargetDialog = nameof(RoomOverviewDialog)
                        };
                        return await sc.EndDialogAsync(dialogResult);
                }
            }

            // this is in case state is manually updated in the previous dialog (ContinueOrUpdatePrompt) 
            var dialogOpts = new DialogOptions
            {
                SkipConfirmation = true
            };
            return await sc.ReplaceDialogAsync(InitialDialogId, dialogOpts);
        }



        public async Task<DialogTurnResult> RespondToNewRequest(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var dialogOptions = new DialogOptions
            {
                SkipConfirmation =
                    true // skip the confirmation in the middle of the dialog (at the end we assume that the user only makes a single adjustment to one value or selects the startover option instead
            };
            if (sc.Result != null)
            {
                var choice = sc.Result as FoundChoice;
                var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
                switch (choice.Value)
                {
                    case FetchAvailableRoomsChoices.Checkin:
                        state.ArrivalDate = null;
                        return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                    case FetchAvailableRoomsChoices.Checkout:
                        state.LeavingDate = null;
                        return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                    case FetchAvailableRoomsChoices.NumberOfPeople:
                        state.NumberOfPeople = null;
                        return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                   
                }
            }

            // this occurs after luis detects and updates a property via updatestatechoiceprompt
            // uses its resumedialog to end with sc.result = null --> ending here
            return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
        }

        public class FetchAvailableRoomsChoices
        {

            public const string Checkin = "Checkin";
            public const string Checkout = "Checkout";
            public const string NumberOfPeople = "Number of people";
            public const string StartOver = "Start over";
            public const string UpdateSearch = "Update search";
            public const string NoThanks = "No thanks";

            public const string RoomOverview = "Order overview";

            public static readonly ReadOnlyCollection<string> Choices =
                new ReadOnlyCollection<string>(
                    new []
                    {
                        Checkin, Checkout, NumberOfPeople, StartOver, UpdateSearch, NoThanks, RoomOverview
                    });
        }
    }
}
