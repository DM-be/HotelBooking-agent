using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Models.Wrappers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Prompts.DepartureDate
{
    public class DepartureDatePromptDialog: ComponentDialog
    {
        private static readonly DepartureDateResponses _responder = new DepartureDateResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators;

        public DepartureDatePromptDialog(StateBotAccessors accessors): base(nameof(DepartureDatePromptDialog))
        {
            InitialDialogId = nameof(DepartureDatePromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForDepartureDateWaterfallSteps = new WaterfallStep []
            {
                PromptForDepartureDate, FinishDepartureDatePromptDialog
            };
            _promptValidators = new PromptValidators(accessors);
            AddDialog(new WaterfallDialog(InitialDialogId, askForDepartureDateWaterfallSteps));
            AddDialog(new DateTimePrompt(DialogIds.DepartureDatePrompt, _promptValidators.DateValidatorAsync));
        }

        private async Task<DialogTurnResult> PromptForDepartureDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.LeavingDate != null)
            {
                return await sc.EndDialogAsync();
            }


            return await sc.PromptAsync(
                DialogIds.DepartureDatePrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, DepartureDateResponses.ResponseIds.DepartureDatePrompt),
                });
        }

        private async Task<DialogTurnResult> FinishDepartureDatePromptDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            bool updated = false;

            if (sc.Options != null)
            {
                var dialogOptions = (DialogOptions)sc.Options;
                updated = dialogOptions.UpdatedLeavingDate;
            }

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


        // default resume behavior reprompts the existing prompt 
        // state can be updated so we need to loop the dialog with itself to reflect these changes
        // (child dialogs such as updatestateprompt will call end
        // --> resume on parent stack will be called
        // --> replace dialog with itself to update state)

        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null,
            CancellationToken cancellationToken = new CancellationToken())
        {

            return dc.ReplaceDialogAsync(InitialDialogId);
        }
    }
}
