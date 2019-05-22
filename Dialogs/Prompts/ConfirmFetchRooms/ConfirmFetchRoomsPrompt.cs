using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Prompts.ConfirmFetchRooms
{
    public class ConfirmFetchRoomsPrompt: ComponentDialog
    {
        private static readonly FetchAvailableRoomsResponses _responder = new FetchAvailableRoomsResponses();
        private readonly StateBotAccessors _accessors;

        public ConfirmFetchRoomsPrompt(StateBotAccessors accessors): base(nameof(ConfirmFetchRoomsPrompt))
        {
            InitialDialogId = nameof(ConfirmFetchRoomsPrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var confirmFetchRoomsWaterfallSteps = new WaterfallStep []
            {
                AskForConfirmationAsync, FinishConfirmationAsync
            };

            AddDialog(new WaterfallDialog(InitialDialogId, confirmFetchRoomsWaterfallSteps));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

        }


        private async Task<DialogTurnResult> AskForConfirmationAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            Activity template = null;
            var stateInCache = sc.Context.TurnState.Get<FetchAvailableRoomsState>("cachedState");
            if (stateInCache != null && stateInCache.IsComplete())
                template = await _responder.RenderTemplate(
                    sc.Context,
                    sc.Context.Activity.Locale,
                    FetchAvailableRoomsResponses.ResponseIds.CachedOverview,
                    state);
            else
                template = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, FetchAvailableRoomsResponses.ResponseIds.Overview, state);


            return await sc.PromptAsync(
                nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = template
                });
        }

        private async Task<DialogTurnResult> FinishConfirmationAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.EndDialogAsync((bool) sc.Result);
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
