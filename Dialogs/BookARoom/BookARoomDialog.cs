using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Shared;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

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
            AddDialog(new DateTimePrompt(DialogIds.ArrivalDateTimePrompt, DateValidatorAsync));
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

            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, new { numberOfPeople });

            return await sc.PromptAsync(DialogIds.ArrivalDateTimePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.ArrivalDatePrompt),
            });

        }

        public async Task<DialogTurnResult> AskForLeavingDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());

            // todo: convert to natural language expression 
            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var arrivalDate = resolution.Value ?? resolution.Start;

            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveArrivalDate, arrivalDate);

            return await sc.PromptAsync(DialogIds.LeavingDateTimePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.LeavingDatePrompt),
            });

        }

        public async Task<DialogTurnResult> FinishBookARoomDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            // send webview for booking here
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            // todo: convert to natural language expression 
            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var leavingDate = resolution.Value ?? resolution.Start;

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
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry, I do not understand. Please enter the date or time for your booking.",
                    cancellationToken: cancellationToken);

                return false;
            }

            // Check whether any of the recognized date-times are appropriate,
            // and if so, return the first appropriate date-time.
            var earliest = DateTime.Now.AddHours(1.0);
            var value = promptContext.Recognized.Value.FirstOrDefault(v =>
                DateTime.TryParse(v.Value ?? v.Start, out var time) && DateTime.Compare(earliest, time) <= 0);

            if (value != null)
            {
                promptContext.Recognized.Value.Clear();
                promptContext.Recognized.Value.Add(value);
                return true;
            }

            await promptContext.Context.SendActivityAsync(
                "I'm sorry, incorrect time",
                cancellationToken: cancellationToken);

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
