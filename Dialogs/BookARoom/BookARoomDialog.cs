using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Custom;
using HotelBot.Dialogs.Shared;
using HotelBot.Dialogs.SlotFillingDialog;
using HotelBot.Extensions;

using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Graph;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using HotelBot.Custom;
using Microsoft.Bot.Builder.AI.Luis;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomDialog : CustomDialog
    {
        private static BookARoomResponses _responder = new BookARoomResponses();
        private StateBotAccessors _accessors;
        private BookARoomState _state;
        private readonly BotServices _services;
        private TranslatorHelper _translatorHelper = new TranslatorHelper();

        public BookARoomDialog(BotServices botServices, StateBotAccessors accessors)
            : base(botServices, accessors, nameof(BookARoomDialog))
        {
            _services = botServices;
            _accessors = accessors;
            InitialDialogId = nameof(BookARoomDialog);
      

            var bookARoom = new WaterfallStep[]
            {
                AskForEmail,
                AskForNumberOfPeople,
                AskForArrivalDate,
                AskForLeavingDate,
                FinishBookARoomDialog,
            };
            AddDialog(new WaterfallDialog(InitialDialogId, bookARoom));
            AddDialog(new CustomDateTimePrompt(DialogIds.ArrivalDateTimePrompt, DateValidatorAsync, Culture.Dutch));
            AddDialog(new DateTimePrompt(DialogIds.LeavingDateTimePrompt, DateValidatorAsync));
            AddDialog(new TextPrompt(DialogIds.EmailPrompt));
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
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveEmailMessage, new { _state.Email });
                return await sc.NextAsync();
            }
            // else prompt for email
            return await sc.PromptAsync(DialogIds.EmailPrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.EmailPrompt),
            });
        }


    // step 
        public async Task<DialogTurnResult> AskForNumberOfPeople(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());

            if (_state.NumberOfPeople != null)
            {
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, _state.NumberOfPeople);
                return await sc.NextAsync();
            }

            if (sc.Result != null)
            {
                _state.Email = (string) sc.Result;
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveEmailMessage, new { _state.Email });
            }

            return await sc.PromptAsync(DialogIds.NumberOfPeopleNumberPrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.NumberOfPeoplePrompt),
            });

        }

        public async Task<DialogTurnResult> AskForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());

            if (_state.ArrivalDate != null)
            {
                return await sc.NextAsync();
            }

            if (sc.Result != null)
            {
                _state.NumberOfPeople = (int) sc.Result;
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, _state.NumberOfPeople);
            }

            return await sc.PromptAsync(DialogIds.ArrivalDateTimePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, Culture.Dutch, BookARoomResponses.ResponseIds.ArrivalDatePrompt),
            });

        }

        public async Task<DialogTurnResult> AskForLeavingDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());

            if (_state.LeavingDate != null)
            {

                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveLeavingDate, _state.LeavingDate);
                return await sc.NextAsync();
            }

            if (sc.Result != null)
            {
                var resolution = (sc.Result as IList<DateTimeResolution>).First();
                
                var timexProp = new TimexProperty(resolution.Timex);
                var arrivalDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);
               // _state.ArrivalDate = arrivalDateAsNaturalLanguage;
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveArrivalDate, arrivalDateAsNaturalLanguage);
            }

            return await sc.PromptAsync(DialogIds.LeavingDateTimePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.LeavingDatePrompt),
            });

        }

        public async Task<DialogTurnResult> FinishBookARoomDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            // send webview for booking here
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());


            if (sc.Result != null)
            {
                var resolution = (sc.Result as IList<DateTimeResolution>).First();
                var timexProp = new TimexProperty(resolution.Timex);
                var leavingDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);
            //    _state.LeavingDate = leavingDateAsNaturalLanguage;
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveLeavingDate, leavingDateAsNaturalLanguage);
            }



            await sc.Context.SendActivityAsync("end of dialog, emptying result");
            // clear state as a test

            var bookARoomEmpty = new BookARoomState();
            _accessors.BookARoomStateAccessor.SetAsync(sc.Context, bookARoomEmpty);
            return await sc.EndDialogAsync();
        }


        
        private async Task<bool> DateValidatorAsync(
            PromptValidatorContext<IList<DateTimeResolution>> promptContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check whether the input could be recognized as an integer.
            if (!promptContext.Recognized.Succeeded)
            {

                await _responder.ReplyWith(promptContext.Context, BookARoomResponses.ResponseIds.NotRecognizedDate);
                return false;
            }
            // TODO: translate in real time here?


            var earliest = DateTime.Now.AddHours(1.0);
            var value = promptContext.Recognized.Value.FirstOrDefault(v =>
                DateTime.TryParse(v.Value ?? v.Start, out var time) && DateTime.Compare(earliest, time) <= 0);
            if (value != null)
            {
                promptContext.Recognized.Value.Clear();
                promptContext.Recognized.Value.Add(value);
                
                return true;
            }

            await _responder.ReplyWith(promptContext.Context, BookARoomResponses.ResponseIds.IncorrectDate);
            return false;
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
