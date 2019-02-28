using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Custom;
using HotelBot.Dialogs.Shared;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomDialog : CustomDialog
    {
        private static BookARoomResponses _responder = new BookARoomResponses();
        private StateBotAccessors _accessors;
        private BookARoomState _state;

        public BookARoomDialog(BotServices botServices, StateBotAccessors accessors)
            : base(botServices, nameof(BookARoomDialog))
        {
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

        public async Task<DialogTurnResult> AskForEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            
            return await sc.PromptAsync(DialogIds.EmailPrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.EmailPrompt),
            });
        }

        public async Task<DialogTurnResult> AskForNumberOfPeople(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            var email = _state.Email = (string) sc.Result;

            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveEmailMessage, new { email });

            return await sc.PromptAsync(DialogIds.NumberOfPeopleNumberPrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.NumberOfPeoplePrompt),
            });

        }

        public async Task<DialogTurnResult> AskForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            var numberOfPeople = _state.NumberOfPeople = (int) sc.Result;
            
            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, numberOfPeople);

            return await sc.PromptAsync(DialogIds.ArrivalDateTimePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, Culture.Dutch, BookARoomResponses.ResponseIds.ArrivalDatePrompt),
            });

        }

        public async Task<DialogTurnResult> AskForLeavingDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());

            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var timexProp = new TimexProperty(resolution.Timex);
            var arrivalDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);


            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveArrivalDate, arrivalDateAsNaturalLanguage);

            return await sc.PromptAsync(DialogIds.LeavingDateTimePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.LeavingDatePrompt),
            });

        }

        public async Task<DialogTurnResult> FinishBookARoomDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            // send webview for booking here
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var timexProp = new TimexProperty(resolution.Timex);
            var leavingDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);

            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveLeavingDate, leavingDateAsNaturalLanguage);

            await sc.Context.SendActivityAsync("end of dialog");
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

        private class DialogIds
        {

            public const string ArrivalDateTimePrompt = "arrivalDateTimePrompt";
            public const string LeavingDateTimePrompt = "leavingDateTimePrompt";
            public const string NumberOfPeopleNumberPrompt = "numberOfPeopleNumberPrompt";
            public const string EmailPrompt = "emailPrompt";
        }
    }
}
