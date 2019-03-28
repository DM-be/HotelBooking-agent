using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Shared.RecognizerDialogs;
using HotelBot.Dialogs.Shared.RecognizerDialogs.Delegates;
using HotelBot.Extensions;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Shared.Prompts.UpdateState
{
    public class UpdateStatePrompt: ComponentDialog
    {
        protected const string LuisResultBookARoomKey = "LuisResult_BookARoom";
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;
        private readonly UpdateStateHandler _updateStateHandler = new UpdateStateHandler();

        public UpdateStatePrompt(BotServices services, StateBotAccessors accessors): base(nameof(UpdateStatePrompt))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(UpdateStatePrompt);

            var updateStateWaterfallSteps = new WaterfallStep []
            {
                ValidateTimeStep, PromptConfirm, EndConfirm
            };
            AddDialog(new WaterfallDialog(InitialDialogId, updateStateWaterfallSteps));
            AddDialog(new ValidateDateTimePrompt());
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new EmailPromptDialog(accessors));
        }



        public async Task<DialogTurnResult> ValidateTimeStep(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var isUpdateDate = (bool) sc.Options;
            if (!isUpdateDate) return await sc.NextAsync();

            var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            bookARoomState.LuisResults.TryGetValue(LuisResultBookARoomKey, out var luisResult);
            TimexProperty timexProperty;
            if (sc.Options != null)
            {
                // timex was get and set via a prompt in another dialog and passed as options (such as a validatedatetimeprompt)
                timexProperty = sc.Options as TimexProperty;
                return await sc.NextAsync(timexProperty, cancellationToken);
            }

            if (luisResult.HasEntityWithPropertyName(EntityNames.Datetime))
            {
                if (luisResult.Entities.datetime.First().Type != "date") // not of type date --> not clear what day arriving etc
                    return await sc.BeginDialogAsync(nameof(ValidateDateTimePrompt));
                // else the timexproperty can be parsed from the entities in the intent
                var dateTimeSpecs = luisResult.Entities.datetime.First();
                var firstExpression = dateTimeSpecs.Expressions.First();
                timexProperty = new TimexProperty(firstExpression);
                return await sc.NextAsync(timexProperty, cancellationToken);
            }

            // intent to update arrival or leaving date but without entity also needs a validation for date.
            return await sc.BeginDialogAsync(nameof(ValidateDateTimePrompt));
        }


        public async Task<DialogTurnResult> PromptConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            bookARoomState.LuisResults.TryGetValue(LuisResultBookARoomKey, out var luisResult);
            var intent = luisResult.TopIntent().intent.ToString();
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
                    Prompt = await view.RenderTemplate(sc.Context, sc.Context.Activity.Locale, intent)
                });
        }

        public async Task<DialogTurnResult> EndConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var confirmed = (bool) sc.Result;
            if (confirmed) return await UpdateState(sc);

            return await sc.EndDialogAsync("test");
        }


        private async Task<DialogTurnResult> UpdateState(WaterfallStepContext sc)
        {

            var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            bookARoomState.LuisResults.TryGetValue(LuisResultBookARoomKey, out var luisResult);
            var intent = luisResult.TopIntent().intent;
            if (_updateStateHandler.UpdateStateHandlerDelegates.TryGetValue(intent, out var DelegateStateUpdate))
            {
                var result = await DelegateStateUpdate(bookARoomState, luisResult, sc);
                await _accessors.BookARoomStateAccessor.SetAsync(sc.Context, bookARoomState);
                return result;
            }
            // return correct sub dialog depending on delegate

            return null;


        }
    }
}
