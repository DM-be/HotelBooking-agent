using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.ContinueOrUpdatePrompt
{
    public class ContinueOrUpdatePrompt: ComponentDialog
    {
        private static readonly ContinueOrUpdatePromptResponses _responder = new ContinueOrUpdatePromptResponses();
        private readonly StateBotAccessors _accessors;

        public ContinueOrUpdatePrompt(StateBotAccessors accessors): base(nameof(ContinueOrUpdatePrompt))
        {
            InitialDialogId = nameof(ContinueOrUpdatePrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var continueOrUpdateWaterfallSteps = new WaterfallStep []
            {
                PromptContinueOrUpdateAsync, EndWithResultAsync
            };

            AddDialog(new WaterfallDialog(InitialDialogId, continueOrUpdateWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        }


        private async Task<DialogTurnResult> PromptContinueOrUpdateAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var state = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());

            var choices = new List<string>
            {

                FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.StartOver,
                FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.UpdateSearch,
            };

            if (state.SelectedRooms != null && state.SelectedRooms.Count != 0)
                choices.Add(FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.RoomOverview);

            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        ContinueOrUpdatePromptResponses.ResponseIds.SendRandomContinueOrUpdatePrompt),
                    Choices = ChoiceFactory.ToChoices(
                        choices)
                },
                cancellationToken);
        }


        private async Task<DialogTurnResult> EndWithResultAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.EndDialogAsync(
                sc.Result);
        }


        // when we are in this step of the dialog we can also use LUIS to update our search
        // it will call a updatestateprompt with a yes or no response
        // we assume that when state is updated, a user will want to see new results

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (result != null)
            {
                var updated = (bool)result;
                if (updated) return await dc.EndDialogAsync();
            }

            // the non overriden function in case the state property was not updated--> reprompts this dialog. 
            await RepromptDialogAsync(dc.Context, dc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }
    }
}
