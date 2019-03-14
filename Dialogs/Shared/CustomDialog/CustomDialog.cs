using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Cancel;
using HotelBot.Dialogs.Shared.InterruptableDialog;
using HotelBot.Dialogs.Shared.Prompts;
using HotelBot.Extensions;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Shared.CustomDialog
{
    /// <summary>
    ///     currently supports room booking intents only
    ///     --> refactor name or fully generic?
    /// </summary>
    public class CustomDialog: InterruptableDialog.InterruptableDialog

    {

        protected const string LuisResultBookARoomKey = "LuisResult_BookARoom";

        private readonly StateBotAccessors _accessors;
        private readonly CancelResponses _responder = new CancelResponses();

        // Fields
        private readonly BotServices _services;

        public CustomDialog(BotServices services, StateBotAccessors accessors, string dialogId)
            : base(dialogId)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new CancelDialog(_accessors));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            var confirmUpdateStepsDate = new WaterfallStep []
            {
                ValidateTimeStep, PromptConfirmDate, EndConfirm
            };


            var confirmUpdateSteps = new WaterfallStep []
            {
                PromptConfirm, EndConfirm
            };

            AddDialog(new WaterfallDialog(DialogIds.DateConfirmWaterfall, confirmUpdateStepsDate));
            AddDialog(new WaterfallDialog(DialogIds.ConfirmWaterfall, confirmUpdateSteps));
            AddDialog(new ValidateDateTimePrompt());


        }

        protected override async Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // check luis intent
            _services.LuisServices.TryGetValue("hotelbot", out var luisService);

            if (luisService == null) throw new Exception("The specified LUIS Model could not be found in your Bot Services configuration.");

            var luisResult = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);
            var intent = luisResult.TopIntent().intent;

            // Only triggers interruption if confidence level is high
            if (luisResult.TopIntent().score > 0.7)
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
                    case HotelBotLuis.Intent.Update_Number_Of_People:
                    case HotelBotLuis.Intent.Update_email:
                    {
                        return await OnUpdate(dc, DialogIds.ConfirmWaterfall);
                    }
                    case HotelBotLuis.Intent.Update_ArrivalDate:
                    case HotelBotLuis.Intent.Update_Leaving_Date:

                    {
                        return await OnUpdate(dc, DialogIds.DateConfirmWaterfall);
                    }
                }
            }

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

            // Signal the conversation was interrupted and should immediately continue
            return InterruptionStatus.Interrupted;
        }

        protected virtual async Task<InterruptionStatus> OnUpdate(DialogContext dc, string dialogId)
        {
            // do not restart this running dialog
            if (dc.ActiveDialog.Id != dialogId)
            {
                // example: in dialog number prompt --> i want to update my email, this prompt is a dialog pushed on the stack
                // to handle this interruption and start the correct new waterfall, we need to cancel the stack: bookaroomdialog>numberprompt

                await dc.CancelAllDialogsAsync(); // removes entire stack

                // begin our own new dialogwaterfall step
                await dc.BeginDialogAsync(dialogId);

                return InterruptionStatus.Waiting;
            }

            return InterruptionStatus.NoAction;
        }

        // only time validation is needed when an intent matches for example ""
        public async Task<DialogTurnResult> ValidateTimeStep(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            bookARoomState.LuisResults.TryGetValue(LuisResultBookARoomKey, out var luisResult);
            TimexProperty timexProperty;
            if (sc.Options != null)
            {
                // timex was get and set via a prompt in another dialog and passed as options (such as a validatedatetimeprompt)
                timexProperty = sc.Options as TimexProperty;
                return await sc.NextAsync(timexProperty);
            }

            if (luisResult.HasEntityWithPropertyName(EntityNames.Datetime))
            {
                if (luisResult.Entities.datetime.First().Type != "date") // not of type date --> not clear what day arriving etc
                    return await sc.BeginDialogAsync(nameof(ValidateDateTimePrompt));
                // else the timexproperty can be parsed from the entities in the intent
                var dateTimeSpecs = luisResult.Entities.datetime.First();
                var firstExpression = dateTimeSpecs.Expressions.First();
                timexProperty = new TimexProperty(firstExpression);
                return await sc.NextAsync(timexProperty);
            }

            // intent to update arrival or leaving date but without entity also needs a validation for date.
            return await sc.BeginDialogAsync(nameof(ValidateDateTimePrompt));


        }

        public async Task<DialogTurnResult> PromptConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            //bookARoomState.LuisResults.TryGetValue(LuisResultBookARoomKey, out var luisResult);
            // attach the full state to the turnstate to allow for dynamic template rendering.
            sc.Context.TurnState["bookARoomState"] = bookARoomState;
            bookARoomState.LuisResults.TryGetValue(LuisResultBookARoomKey, out var luisResult);
            var intent = luisResult.TopIntent().intent.ToString();
            var view = new BookARoomResponses();

            return await sc.PromptAsync(
                nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = await view.RenderTemplate(sc.Context, sc.Context.Activity.Locale, intent)
                });
        }

        public async Task<DialogTurnResult> PromptConfirmDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            if (sc.Result != null)
            {
                bookARoomState.TimexResults["tempTimex"] = sc.Result as TimexProperty; // for use in updatestate
                sc.Context.TurnState["tempTimex"] = sc.Result as TimexProperty; // for use in reply t
            }

            // attach the full state to the turnstate to allow for dynamic template rendering.
            sc.Context.TurnState["bookARoomState"] = bookARoomState;
            var view = new BookARoomResponses();
            return await sc.PromptAsync(
                nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = await view.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.UpdateText)
                });
        }

        public async Task<DialogTurnResult> EndConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var confirmed = (bool) sc.Result;
            if (confirmed) UpdateState(sc);

            // end the current dialog (this waterfall) and start a new book a room dialog in its place

            return await sc.ReplaceDialogAsync(nameof(BookARoomDialog), cancellationToken);
        }

        private async void UpdateState(WaterfallStepContext sc)
        {
            var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            bookARoomState.LuisResults.TryGetValue(LuisResultBookARoomKey, out var luisResult);
            switch (luisResult.TopIntent().intent)
            {
                case HotelBotLuis.Intent.Update_email:
                    if (luisResult.HasEntityWithPropertyName(EntityNames.Email))
                        bookARoomState.Email = luisResult.Entities.email.First();
                    else // intent matched but no matching entity
                        bookARoomState.Email = null;
                    break;
                case HotelBotLuis.Intent.Update_Number_Of_People:
                {
                    if (luisResult.HasEntityWithPropertyName(EntityNames.Number))
                        bookARoomState.NumberOfPeople = luisResult.Entities.number.First();
                    else
                        bookARoomState.NumberOfPeople = null;
                    break;
                }
                case HotelBotLuis.Intent.Update_ArrivalDate:
                {
                    if (bookARoomState.TimexResults.TryGetValue("tempTimex", out var arrivingTimexProperty))
                    {
                        bookARoomState.ArrivalDate = arrivingTimexProperty;
                        bookARoomState.TimexResults.Clear();
                    }
                    else
                    {
                        bookARoomState.ArrivalDate = null;
                    }

                    break;
                }
                case HotelBotLuis.Intent.Update_Leaving_Date:
                    if (bookARoomState.TimexResults.TryGetValue("tempTimex", out var leavingTimexProperty))
                    {

                        bookARoomState.LeavingDate = leavingTimexProperty;
                        bookARoomState.TimexResults.Clear();
                    }
                    else
                    {
                        bookARoomState.ArrivalDate = null;
                    }

                    break;
            }

            await _accessors.BookARoomStateAccessor.SetAsync(sc.Context, bookARoomState);
        }
    }

    public class EntityNames
    {
        public const string Email = "email";
        public const string Number = "number";
        public const string Datetime = "datetime";
    }

    public class DialogIds
    {
        public const string ConfirmWaterfall = "confirmWaterfall";
        public const string DateConfirmWaterfall = "dateConfirmWaterfall";
    }

}
