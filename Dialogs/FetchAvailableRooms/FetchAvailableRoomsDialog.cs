using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.ConfirmFetchRooms;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
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
                AskForNumberOfPeople, AskForArrivalDate, AskForLeavingDate, PromptConfirm, ProcessConfirmPrompt, RespondToContinueOrUpdate, RespondToNewRequest
            };
            AddDialog(new WaterfallDialog(InitialDialogId, fetchAvailableRoomsWaterfallSteps));
            AddDialog(new ArrivalDatePromptDialog(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmFetchRoomsPrompt(accessors));
            AddDialog(new DepartureDatePromptDialog(accessors));
            AddDialog(new NumberOfPeoplePromptDialog(accessors));
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

        public async Task<DialogTurnResult> PromptConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            DialogOptions dialogOptions = null;
            if (sc.Options != null)
            {
                dialogOptions = (DialogOptions) sc.Options;
                if (dialogOptions.SkipConfirmation)
                {
                    return await sc.NextAsync(true);
                }
            }

            return await sc.BeginDialogAsync(nameof(ConfirmFetchRoomsPrompt), dialogOptions);
        }

        public async Task<DialogTurnResult> ProcessConfirmPrompt(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var confirmed = (bool) sc.Result;
            if (confirmed)
            {
                // send book a room cards
                var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
                await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendRoomsCarousel, state);
            }
            else
            {
                var choice = new FoundChoice
                {
                    Value = "New request",
                };
                return await sc.NextAsync(choice);
            }


            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, FetchAvailableRoomsResponses.ResponseIds.ContinueOrUpdate),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            "New request",
                            "Update request",
                            "No thanks",
                        })
                },
                cancellationToken);
        }

        public async Task<DialogTurnResult> RespondToContinueOrUpdate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var choice = sc.Result as FoundChoice;

            switch (choice.Value)
            {
                case "Update request":

                    return await sc.PromptAsync(
                        nameof(ChoicePrompt),
                        new PromptOptions
                        {
                            Prompt = await _responder.RenderTemplate(
                                sc.Context,
                                sc.Context.Activity.Locale,
                                FetchAvailableRoomsResponses.ResponseIds.UpdatePrompt),
                            Choices = ChoiceFactory.ToChoices(
                                new List<string>
                                {
                                    "Checkin",
                                    "Checkout",
                                    "Number Of People"
                                })
                        },
                        cancellationToken);
                case "New request":
                {
                    var emptyState = new FetchAvailableRoomsState();
                    _accessors.FetchAvailableRoomsStateAccessor.SetAsync(sc.Context, emptyState);
                    await sc.Context.SendActivityAsync("Ok, let's start over");
                    return await sc.ReplaceDialogAsync(InitialDialogId);
                }
                case "No thanks":
                    await sc.Context.SendActivityAsync("You're welcome");
                    return await sc.EndDialogAsync();
                    break;
            }

            return null;
        }



        public async Task<DialogTurnResult> RespondToNewRequest(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var choice = sc.Result as FoundChoice;
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            var dialogOptions = new DialogOptions
            {
                SkipConfirmation = true,
                Rerouted = false
            };
            switch (choice.Value)
            {
                case "Checkin":
                    state.ArrivalDate = null;
                    return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                case "Checkout":
                    state.LeavingDate = null;
                    return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);
                case "Number Of People":
                    state.NumberOfPeople = null;
                    return await sc.ReplaceDialogAsync(InitialDialogId, dialogOptions);

            }

            return null;
        }
    }
}
