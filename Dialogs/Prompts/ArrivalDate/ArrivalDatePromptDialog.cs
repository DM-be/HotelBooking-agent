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

namespace HotelBot.Dialogs.Prompts.ArrivalDate
{
    public class ArrivalDatePromptDialog: ComponentDialog
    {
        private static readonly ArrivalDateResponses _responder = new ArrivalDateResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();


        public ArrivalDatePromptDialog(StateBotAccessors accessors): base(nameof(ArrivalDatePromptDialog))
        {
            InitialDialogId = nameof(ArrivalDatePromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForArrivalDateWaterfallSteps = new WaterfallStep []
            {
                PromptForArrivalDate, FinishArrivalDatePromptDialog
            };

            AddDialog(new WaterfallDialog(InitialDialogId, askForArrivalDateWaterfallSteps));
            AddDialog(new DateTimePrompt(DialogIds.ArrivalDatePrompt, _promptValidators.DateValidatorAsync));
        }


        private async Task<DialogTurnResult> PromptForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.ArrivalDate != null)
            {
                return await sc.EndDialogAsync();
            }

            return await sc.PromptAsync(
                DialogIds.ArrivalDatePrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, ArrivalDateResponses.ResponseIds.ArrivalDatePrompt),
                });
        }


        private async Task<DialogTurnResult> FinishArrivalDatePromptDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            bool updated = false;

            if (sc.Options != null)
            {
                var dialogOptions = (DialogOptions)sc.Options;
                updated = dialogOptions.UpdatedArrivalDate;
            }
            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var timexProp = new TimexProperty(resolution.Timex);
            var arrivalDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);


            var _state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            _state.ArrivalDate = timexProp;

            if (updated)
                await _responder.ReplyWith(sc.Context, ArrivalDateResponses.ResponseIds.HaveUpdatedArrivalDate, arrivalDateAsNaturalLanguage);
            else
                await _responder.ReplyWith(sc.Context, ArrivalDateResponses.ResponseIds.HaveArrivalDate, arrivalDateAsNaturalLanguage);

            return await sc.EndDialogAsync();
        }



        private class DialogIds
        {
            public const string ArrivalDatePrompt = "arrivalDatePrompt";
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
