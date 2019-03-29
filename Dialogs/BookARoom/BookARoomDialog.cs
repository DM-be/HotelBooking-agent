using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.ConfirmFetchRooms;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Dialogs.Shared.RecognizerDialogs;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomDialog: BookARoomRecognizerDialog
    {
        private static BookARoomResponses _responder;
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();
        private readonly BotServices _services;
        private BookARoomState _state;
        private TranslatorHelper _translatorHelper = new TranslatorHelper();


        public BookARoomDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(BookARoomDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _responder = new BookARoomResponses();
            InitialDialogId = nameof(BookARoomDialog);
            var bookARoom = new WaterfallStep []
            {
                AskForEmail, AskForNumberOfPeople, AskForArrivalDate, AskForLeavingDate, PromptConfirm, ProcessConfirmPrompt, UpdateStateLoop
            };
            AddDialog(new WaterfallDialog(InitialDialogId, bookARoom));
            AddDialog(new ArrivalDatePromptDialog(accessors));
            AddDialog(new NumberOfPeoplePromptDialog(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new EmailPromptDialog(_accessors));
            AddDialog(new ConfirmFetchRoomsPrompt(accessors));
            AddDialog(new DepartureDatePromptDialog(accessors));

        }

        public async Task<DialogTurnResult> AskForEmail(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (_state.Email != null) return await sc.NextAsync();
            return await sc.BeginDialogAsync(nameof(EmailPromptDialog));
        }

        public async Task<DialogTurnResult> AskForNumberOfPeople(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (_state.NumberOfPeople != null) return await sc.NextAsync();
            return await sc.BeginDialogAsync(nameof(NumberOfPeoplePromptDialog));
        }

        public async Task<DialogTurnResult> AskForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (_state.ArrivalDate != null) return await sc.NextAsync();
            return await sc.BeginDialogAsync(nameof(ArrivalDatePromptDialog));
        }

        public async Task<DialogTurnResult> AskForLeavingDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (_state.LeavingDate != null) return await sc.NextAsync();
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
                _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.SendRoomsCarousel, _state);
                var bookARoomEmpty = new BookARoomState();
                await _accessors.BookARoomStateAccessor.SetAsync(sc.Context, bookARoomEmpty);
                return await sc.EndDialogAsync();
                // return await Task.FromResult(new DialogTurnResult(DialogTurnStatus.Waiting));
            }


            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What would you like to adjust?"),
                    RetryPrompt = MessageFactory.Text("Pick an item or tell me"),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            "Arrival",
                            "Leaving",
                            "Number of people",
                            "Email"
                        })
                },
                cancellationToken);

            // end the current dialog (this waterfall) and replace it with a bookaroomdialog
            //  await sc.EndDialogAsync(cancellationToken);
        }

        public async Task<DialogTurnResult> UpdateStateLoop(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            var choice = sc.Result as FoundChoice;

            switch (choice.Value)
            {
                case "Arrival":
                    _state.ArrivalDate = null;
                    break;
                case "Leaving":
                    _state.LeavingDate = null;
                    break;
                case "Number of people":
                    _state.NumberOfPeople = null;
                    break;
                case "Email":
                    _state.Email = null;
                    break;


            }

            return await sc.ReplaceDialogAsync(InitialDialogId, null);
        }





        public class DialogIds
        {
            public const string ArrivalDateTimePrompt = "arrivalDateTimePrompt";
            public const string LeavingDateTimePrompt = "leavingDateTimePrompt";
            public const string NumberOfPeopleNumberPrompt = "NumberOfPeople";
            public const string EmailPrompt = "Email";
        }
    }
}
