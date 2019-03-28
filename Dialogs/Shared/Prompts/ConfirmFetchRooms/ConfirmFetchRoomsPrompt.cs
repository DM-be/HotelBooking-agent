using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Email;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.Prompts.ConfirmFetchRooms
{
    public class ConfirmFetchRoomsPrompt: ComponentDialog
    {
        private readonly StateBotAccessors _accessors;
        private static readonly BookARoomResponses _responder = new BookARoomResponses();
        private readonly PromptValidators.PromptValidators _promptValidators = new PromptValidators.PromptValidators();

        public ConfirmFetchRoomsPrompt(StateBotAccessors accessors): base(nameof(ConfirmFetchRoomsPrompt))
        {
            InitialDialogId = nameof(ConfirmFetchRoomsPrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var confirmFetchRoomsWaterfallSteps = new WaterfallStep[]
            {
                AskForConfirmation, FinishConfirmation
            };

            AddDialog(new WaterfallDialog(InitialDialogId, confirmFetchRoomsWaterfallSteps));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

        }


        private async Task<DialogTurnResult> AskForConfirmation(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            return await sc.PromptAsync(
                nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.Overview, _state)
                });
        }

        private async Task<DialogTurnResult> FinishConfirmation(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.EndDialogAsync((bool)sc.Result);
        }



       // default resume behavior reprompts the existing prompt
       // state can be updated so we need to loop the dialog with itself to reflect these changes
       // (child dialogs such as updatestateprompt will call end
       // --> resume on parent stack will be called
       // --> replace dialog with itself to update state)
        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = new CancellationToken())
        {

            return dc.ReplaceDialogAsync(InitialDialogId);
        }






    }
}
