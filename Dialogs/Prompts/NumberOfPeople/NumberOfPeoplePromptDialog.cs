using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Models.Wrappers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.NumberOfPeople
{
    public class NumberOfPeoplePromptDialog: ComponentDialog
    {
        private static readonly NumberOfPeopleResponses _responder = new NumberOfPeopleResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators; // implement checking on max min number of people --> x hotel only has y size rooms etc

        public NumberOfPeoplePromptDialog(StateBotAccessors accessors)
            : base(nameof(NumberOfPeoplePromptDialog))
        {
            InitialDialogId = nameof(NumberOfPeoplePromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForNumberOfPeopleWaterfallSteps = new WaterfallStep []
            {
                PromptForNumberOfPeopleAsync, FinishNumberOfPeoplePromptDialogAsync
            };
            _promptValidators = new PromptValidators(accessors);
            AddDialog(new WaterfallDialog(InitialDialogId, askForNumberOfPeopleWaterfallSteps));
            AddDialog(new NumberPrompt<int>(DialogIds.NumberOfPeoplePrompt));

        }

        private async Task<DialogTurnResult> PromptForNumberOfPeopleAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            if (state.NumberOfPeople != null) {
                return await sc.EndDialogAsync();
            }

            return await sc.PromptAsync(
                DialogIds.NumberOfPeoplePrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, NumberOfPeopleResponses.ResponseIds.NumberOfPeoplePrompt),
                    RetryPrompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, NumberOfPeopleResponses.ResponseIds.RetryNumberOfPeoplePrompt),
                });

        }

        private async Task<DialogTurnResult> FinishNumberOfPeoplePromptDialogAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var numberOfPeople = (int) sc.Result;
            bool updated = false;

            if (sc.Options != null)
            {
                var dialogOptions = (DialogOptions)sc.Options;
                updated = dialogOptions.UpdatedNumberOfPeople; //todo: fix this --> this wont be set, only here, read only object 
            }

            var _state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            _state.NumberOfPeople = numberOfPeople;

            if (updated)
                await _responder.ReplyWith(sc.Context, NumberOfPeopleResponses.ResponseIds.HaveUpdatedNumberOfPeople, numberOfPeople);
            else
                await _responder.ReplyWith(sc.Context, NumberOfPeopleResponses.ResponseIds.HaveNumberOfPeople, numberOfPeople);


            return await sc.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string NumberOfPeoplePrompt = "numberOfPeoplePrompt";
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
