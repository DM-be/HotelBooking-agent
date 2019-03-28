using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Cancel;
using HotelBot.Dialogs.Shared.Prompts.UpdateState;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.RecognizerDialogs
{

    public class BookARoomRecognizerDialog: InterruptableDialog

    {
        protected const string LuisResultBookARoomKey = "LuisResult_BookARoom";
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public BookARoomRecognizerDialog(BotServices services, StateBotAccessors accessors, string dialogId)
            : base(dialogId)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new CancelDialog(_accessors));
            AddDialog(new UpdateStatePrompt(accessors));
        }

        protected override async Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // check luis intent
            _services.LuisServices.TryGetValue("hotelbot", out var luisService);
            if (luisService == null) throw new Exception("The specified LUIS Model could not be found in your Bot Services configuration.");
            var luisResult = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);
            var intent = luisResult.TopIntent().intent;
            var isChoicePrompt = dc.ActiveDialog.Id == nameof(ChoicePrompt);
            // Only triggers interruption if confidence level is high
            if (luisResult.TopIntent().score > 0.75 && !isChoicePrompt)
            {
                // Add the luis result (intent and entities) for further processing in the derived dialog
                var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(dc.Context, () => new BookARoomState());
                bookARoomState.LuisResults[LuisResultBookARoomKey] = luisResult;

                switch (intent)
                {
                    case HotelBotLuis.Intent.Cancel:
                    {
                        return await OnCancel(dc);
                    }
                    case HotelBotLuis.Intent.Help:
                    {
                        // todo: provide contextual help
                        return await OnHelp(dc);
                    }
                    default:
                    {
                        var isDateUpdateIntent = intent.IsUpdateDateIntent();
                        return await OnUpdate(dc, isDateUpdateIntent);
                    }
                }
            }

            // call the non overriden continue dialog in componentdialog
            return InterruptionStatus.NoAction;
        }

        protected virtual async Task<InterruptionStatus> OnCancel(DialogContext dc)
        {
            if (dc.ActiveDialog.Id != nameof(CancelDialog))
            {
                // Don't start restart cancel dialog
                await dc.BeginDialogAsync(nameof(CancelDialog));

                // Signal that the dialog is waiting on user response
                return InterruptionStatus.Waiting;
            }

            // Else, continue
            return InterruptionStatus.NoAction;
        }


        protected virtual async Task<InterruptionStatus> OnHelp(DialogContext dc)
        {
            var view = new BookARoomResponses();
            await view.ReplyWith(dc.Context, BookARoomResponses.ResponseIds.Help);

            // Signal the conversation was interrupted and should immediately continue (calls reprompt)
            return InterruptionStatus.Interrupted;
        }

        protected virtual async Task<InterruptionStatus> OnUpdate(DialogContext dc, bool isUpdateDate)
        {
            // do not restart this running dialog
            if (dc.ActiveDialog.Id != nameof(UpdateStatePrompt))
            {
                // example: in dialog number prompt --> i want to update my email, this prompt is a dialog pushed on the stack
                // --> old comment: to handle this interruption and start the correct new waterfall, we need to cancel the stack: bookaroomdialog>numberprompt
                // --> old comment: await dc.CancelAllDialogsAsync(); // removes entire stack
                // begin our own new dialogwaterfall step

                await dc.BeginDialogAsync(nameof(UpdateStatePrompt), isUpdateDate);

                return InterruptionStatus.Waiting;
            }

            return InterruptionStatus.NoAction;
        }
    }

    public class EntityNames
    {
        public const string Email = "email";
        public const string Number = "number";
        public const string Datetime = "datetime";
    }

}
