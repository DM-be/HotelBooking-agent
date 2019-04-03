using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.ConfirmFetchRooms;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
using HotelBot.Dialogs.Shared.RecognizerDialogs.FetchAvailableRooms;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
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
                AskForNumberOfPeople, AskForArrivalDate, AskForLeavingDate, PromptConfirm, ProcessConfirmPrompt, UpdateStateLoop
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
            return await sc.BeginDialogAsync(nameof(ConfirmFetchRoomsPrompt));
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


            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, FetchAvailableRoomsResponses.ResponseIds.ContinueOrUpdate),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            "New request",
                            "No thanks",
                        })
                },
                cancellationToken);
        }

        public async Task<DialogTurnResult> UpdateStateLoop(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            var choice = sc.Result as FoundChoice;

            switch (choice.Value)
            {
                case "New request":
                    return null; // send update prompt here 
                    break;
                case "No thanks":
                    await sc.Context.SendActivityAsync("You're welcome");
                    return await sc.EndDialogAsync();
                    break;
            }

            return await sc.ReplaceDialogAsync(InitialDialogId, null);
        }
    }
}
