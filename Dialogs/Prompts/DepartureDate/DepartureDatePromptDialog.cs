using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Prompts.DepartureDate
{
    public class DepartureDatePromptDialog: ComponentDialog
    {
        private static readonly DepartureDateResponses _responder = new DepartureDateResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();

        public DepartureDatePromptDialog(StateBotAccessors accessors): base(nameof(DepartureDatePromptDialog))
        {
            InitialDialogId = nameof(DepartureDatePromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForDepartureDateWaterfallSteps = new WaterfallStep []
            {
                PromptForDepartureDate, FinishDepartureDatePromptDialog
            };

            AddDialog(new WaterfallDialog(InitialDialogId, askForDepartureDateWaterfallSteps));
            AddDialog(new DateTimePrompt(DialogIds.DepartureDatePrompt, _promptValidators.DateValidatorAsync));
        }

        private async Task<DialogTurnResult> PromptForDepartureDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                DialogIds.DepartureDatePrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, DepartureDateResponses.ResponseIds.DepartureDatePrompt),
                    RetryPrompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        DepartureDateResponses.ResponseIds.RetryDepartureDatePrompt)
                });
        }

        private async Task<DialogTurnResult> FinishDepartureDatePromptDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var updated = false;
            if (sc.Options != null) updated = (bool) sc.Options; // usually true

            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var timexProp = new TimexProperty(resolution.Timex);
            var departureDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);

            var _state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            _state.LeavingDate = timexProp;

            if (updated)
                await _responder.ReplyWith(sc.Context, DepartureDateResponses.ResponseIds.HaveUpdatedDepartureDate, departureDateAsNaturalLanguage);
            else
                await _responder.ReplyWith(sc.Context, DepartureDateResponses.ResponseIds.HaveDepartureDate, departureDateAsNaturalLanguage);

            return await sc.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string DepartureDatePrompt = "departureDatePrompt";
        }
    }
}
