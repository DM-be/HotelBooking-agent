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
                SendOptionalIntroductionAsync, AskForNumberOfPeopleAsync, AskForArrivalDateAsync, AskForLeavingDateAsync, PromptFetchRoomsConfirmationPromptAsync,
                ProcessFetchRoomsConfirmationPromptAsync, RespondToContinueOrUpdateAsync,
                RespondToNewRequestAsync
            };

            AddDialog(new WaterfallDialog(InitialDialogId, fetchAvailableRoomsWaterfallSteps));
            AddDialog(new ArrivalDatePromptDialog(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmFetchRoomsPrompt(accessors));
            AddDialog(new DepartureDatePromptDialog(accessors));
            AddDialog(new NumberOfPeoplePromptDialog(accessors));
            AddDialog(new ContinueOrUpdatePrompt(accessors));
            AddDialog(new FetchAvailableRoomsIntroductionPrompt());
            AddDialog(new RoomOverviewDialog(services, accessors));
            AddDialog(new UpdateStateChoicePrompt());
        }


        public async Task<DialogTurnResult> SendOptionalIntroductionAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());
            if (userProfile != null && userProfile.SendFetchAvailableRoomsIntroduction)
            {
                userProfile.SendFetchAvailableRoomsIntroduction = false;
                return await sc.BeginDialogAsync(nameof(FetchAvailableRoomsIntroductionPrompt));
            }
            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> AskForNumberOfPeopleAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.NumberOfPeople != null)
            {
                sc.Context.TurnState[CachedStateKey] = state;
                return await sc.NextAsync();
            }
            return await sc.BeginDialogAsync(nameof(NumberOfPeoplePromptDialog));
        }

        public async Task<DialogTurnResult> AskForArrivalDateAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.ArrivalDate != null)
            {
                sc.Context.TurnState[CachedStateKey] = state;
                return await sc.NextAsync();
            }
            return await sc.BeginDialogAsync(nameof(ArrivalDatePromptDialog));
        }

        public async Task<DialogTurnResult> AskForLeavingDateAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.LeavingDate != null)
            {
                sc.Context.TurnState[CachedStateKey] = state;
                return await sc.NextAsync();
            }
            return await sc.BeginDialogAsync(nameof(DepartureDatePromptDialog));
        }

        public async Task<DialogTurnResult> PromptFetchRoomsConfirmationPromptAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            DialogOptions dialogOptions = null;
            if (sc.Options != null)
            {
                dialogOptions = (DialogOptions) sc.Options;
                if (dialogOptions.SkipConfirmation) return await sc.NextAsync(dialogOptions.SkipConfirmation);
            }

            return await sc.BeginDialogAsync(nameof(ConfirmFetchRoomsPrompt), dialogOptions);
        }

        public async Task<DialogTurnResult> ProcessFetchRoomsConfirmationPromptAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var confirmed = (bool) sc.Result;
            if (confirmed)
            {
                var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
                await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.HoldOnChecking);
                Thread.Sleep(500); // dummy sleep for presentation
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

        public async Task<DialogTurnResult> RespondToContinueOrUpdateAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
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
                        await _accessors.FetchAvailableRoomsStateAccessor.SetAsync(sc.Context, emptyState);
                            await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.StartOver);
                            var dialogOptions = new DialogOptions
                        {
                            SkipConfirmation = false // start over and prompt for confirmation again 
                        };
                        return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                    }
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



        public async Task<DialogTurnResult> RespondToNewRequestAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
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
    
            public const string RoomOverview = "Booking overview";

            public static readonly ReadOnlyCollection<string> Choices =
                new ReadOnlyCollection<string>(
                    new []
                    {
                        Checkin, Checkout, NumberOfPeople, StartOver, UpdateSearch, RoomOverview
                    });
        }
    }
}
