using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.ConfirmFetchRooms;
using HotelBot.Dialogs.Prompts.ContinueOrUpdatePrompt;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.FetchAvailableRoomsIntroduction;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
using HotelBot.Dialogs.Prompts.UpdateStateChoice;
using HotelBot.Dialogs.Shared.RecognizerDialogs.FetchAvailableRooms;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.FetchAvailableRooms
{
    public class FetchAvailableRoomsDialog: FetchAvailableRoomsRecognizerDialog
    {
        private static FetchAvailableRoomsResponses _responder;
        private readonly StateBotAccessors _accessors;

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
        }


        public async Task<DialogTurnResult> SendOptionalIntroduction(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var dialogOptions = sc.Options as DialogOptions;
            if (dialogOptions.SkipIntroduction != null && dialogOptions.SkipIntroduction == false)
                return await sc.BeginDialogAsync(nameof(FetchAvailableRoomsIntroductionPrompt));
            return await sc.NextAsync();

        }

        public async Task<DialogTurnResult> AskForNumberOfPeople(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.NumberOfPeople != null) return await sc.NextAsync();

            return await sc.BeginDialogAsync(nameof(NumberOfPeoplePromptDialog));
        }

        public async Task<DialogTurnResult> AskForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.ArrivalDate != null) return await sc.NextAsync();
            return await sc.BeginDialogAsync(nameof(ArrivalDatePromptDialog));
        }

        public async Task<DialogTurnResult> AskForLeavingDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.LeavingDate != null) return await sc.NextAsync();
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

            // when user presses no --> skip this step manually with a change search foundchoice
            var foundChoice = new FoundChoice
            {
                Value = FetchAvailableRoomsChoices.ChangeSearch
            };
            return await sc.NextAsync(foundChoice);
        }
        public async Task<DialogTurnResult> RespondToContinueOrUpdate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var choice = sc.Result as FoundChoice;
            switch (choice.Value)
            {
                case FetchAvailableRoomsChoices.ChangeSearch:
                    return await sc.BeginDialogAsync(nameof(UpdateStateChoicePrompt), sc.Options);
                case FetchAvailableRoomsChoices.StartOver:
                {
                    var emptyState = new FetchAvailableRoomsState();
                    _accessors.FetchAvailableRoomsStateAccessor.SetAsync(sc.Context, emptyState);
                    await sc.Context.SendActivityAsync("Ok, let's start over");
                    var dialogOptions = new DialogOptions
                    {
                        SkipIntroduction = true,
                        Rerouted = false,

                    };
                    return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                }
                case FetchAvailableRoomsChoices.NoThanks:
                    await sc.Context.SendActivityAsync("You're welcome");
                    return await sc.EndDialogAsync();
                    break;
                //case FetchAvailableRoomsChoices.Nevermind:
                //{
                //    var dialogOptions = new DialogOptions
                //    {
                //        Rerouted = false,
                //        SkipConfirmation = true,
                //        SkipIntroduction = true

                //    };
                //    return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                //}
            }

            return null;
        }



        public async Task<DialogTurnResult> RespondToNewRequest(WaterfallStepContext sc, CancellationToken cancellationToken)
        {


            var choice = sc.Result as FoundChoice;
            var oldDialogOptions = new DialogOptions
            {
                Rerouted = false,
                SkipConfirmation = false

            };
            if (sc.Options != null) oldDialogOptions = (DialogOptions) sc.Options;

            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            var dialogOptions = new DialogOptions
            {
                SkipConfirmation = true,
                Rerouted = false,
                SkipIntroduction = true

            };
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
                case "none":
                    return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
            }

            return null;
        }


        public class FetchAvailableRoomsChoices
        {
           
            public const string Checkin = "Checkin";
            public const string Checkout = "Checkout";
            public const string NumberOfPeople = "Number of people";
            public const string StartOver = "Start over";
            public const string ChangeSearch = "Change search";
            public const string Nevermind = "Nevermind";
            public const string NoThanks = "No thanks";
            public static readonly ReadOnlyCollection<string> Choices =
                new ReadOnlyCollection<string>(new[]
                {
                    Checkin,
                    Checkout,
                    NumberOfPeople,
                    StartOver,
                    ChangeSearch,
                    Nevermind,
                    NoThanks
                });

        }
    }
}
