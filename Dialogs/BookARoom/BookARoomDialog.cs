using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Shared;
using HotelBot.Dialogs.Shared.CustomDialog;
using HotelBot.Dialogs.Shared.Validators;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomDialog: CustomDialog
    {
        private static BookARoomResponses _responder;
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;
        private BookARoomState _state;
        private TranslatorHelper _translatorHelper = new TranslatorHelper();
        private Validators _validators = new Validators();

        public BookARoomDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(BookARoomDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _responder = new BookARoomResponses();
            InitialDialogId = nameof(BookARoomDialog);


            var bookARoom = new WaterfallStep []
            {
                AskForEmail, AskForNumberOfPeople, AskForArrivalDate, AskForLeavingDate, FinishBookARoomDialog
            };
            AddDialog(new WaterfallDialog(InitialDialogId, bookARoom));
            AddDialog(new DateTimePrompt(DialogIds.ArrivalDateTimePrompt, _validators.DateValidatorAsync));
            AddDialog(new DateTimePrompt(DialogIds.LeavingDateTimePrompt, _validators.DateValidatorAsync));
            AddDialog(new TextPrompt(DialogIds.EmailPrompt, _validators.EmailValidatorAsync));
            AddDialog(new NumberPrompt<int>(DialogIds.NumberOfPeopleNumberPrompt));
        }



        // first step --> intent checking and entity gathering was done in the general book a room intent
        public async Task<DialogTurnResult> AskForEmail(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            // property was gathered by LUIS or replaced manually after a confirm prompt
            if (_state.Email != null)
            {
                // skip to next step and send a reply with the email
                await _responder.ReplyWith(
                    sc.Context,
                    BookARoomResponses.ResponseIds.HaveEmailMessage,
                    _state.Email);
                return await sc.NextAsync();
            }

            // else prompt for email
            return await sc.PromptAsync(
                DialogIds.EmailPrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.EmailPrompt)
                });
        }


        // step 
        public async Task<DialogTurnResult> AskForNumberOfPeople(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (sc.Result != null)
            {
                _state.Email = (string) sc.Result;
                await _responder.ReplyWith(
                    sc.Context,
                    BookARoomResponses.ResponseIds.HaveEmailMessage,
                    _state.Email);
            }

            if (_state.NumberOfPeople != null)
            {
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, _state.NumberOfPeople);
                return await sc.NextAsync();
            }

            return await sc.PromptAsync(
                DialogIds.NumberOfPeopleNumberPrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.NumberOfPeoplePrompt)
                });

        }

        public async Task<DialogTurnResult> AskForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (sc.Result != null)
            {
                _state.NumberOfPeople = (int) sc.Result;
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, _state.NumberOfPeople);
            }

            if (_state.ArrivalDate != null) return await sc.NextAsync();

            return await sc.PromptAsync(
                DialogIds.ArrivalDateTimePrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, Culture.Dutch, BookARoomResponses.ResponseIds.ArrivalDatePrompt)
                });

        }

        public async Task<DialogTurnResult> AskForLeavingDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (sc.Result != null)
            {
                var resolution = (sc.Result as IList<DateTimeResolution>).First();
                var timexProp = new TimexProperty(resolution.Timex);
                var arrivalDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);
                _state.ArrivalDate = timexProp;
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveArrivalDate, arrivalDateAsNaturalLanguage);
            }


            if (_state.LeavingDate != null)
            {

                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveLeavingDate, _state.LeavingDate);
                return await sc.NextAsync();
            }

            return await sc.PromptAsync(
                DialogIds.LeavingDateTimePrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.LeavingDatePrompt)
                });

        }

        public async Task<DialogTurnResult> FinishBookARoomDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (sc.Result != null)
            {
                var resolution = (sc.Result as IList<DateTimeResolution>).First();
                var timexProp = new TimexProperty(resolution.Timex);
                _state.LeavingDate = timexProp;
                 await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveLeavingDate, _state.LeavingDate);
            }
            // send webview for booking here


            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveEmailMessage, _state.Email);
            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, _state.NumberOfPeople);
            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveArrivalDate, _state.ArrivalDate.ToNaturalLanguage(DateTime.Now));
            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveLeavingDate, _state.LeavingDate.ToNaturalLanguage(DateTime.Now));


            await sc.Context.SendActivityAsync("end of dialog, emptying result");
            // clear state as a test

            var bookARoomEmpty = new BookARoomState();
            await _accessors.BookARoomStateAccessor.SetAsync(sc.Context, bookARoomEmpty);
            return await sc.EndDialogAsync();
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
