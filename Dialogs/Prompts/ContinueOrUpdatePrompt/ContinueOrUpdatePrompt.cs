using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.ContinueOrUpdatePrompt
{
    public class ContinueOrUpdatePrompt: ComponentDialog
    {
        private static readonly FetchAvailableRoomsResponses _responder = new FetchAvailableRoomsResponses();
        private readonly StateBotAccessors _accessors;

        public ContinueOrUpdatePrompt(StateBotAccessors accessors): base(nameof(ContinueOrUpdatePrompt))
        {
            InitialDialogId = nameof(ContinueOrUpdatePrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var continueOrUpdateWaterfallSteps = new WaterfallStep []
            {
                PromptContinueOrUpdate, EndWithResult
            };

            AddDialog(new WaterfallDialog(InitialDialogId, continueOrUpdateWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }


        private async Task<DialogTurnResult> PromptContinueOrUpdate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        FetchAvailableRoomsResponses.ResponseIds.ContinueOrUpdate),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.StartOver,
                            FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.ChangeSearch,
                            FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.NoThanks
                        })
                },
                cancellationToken);
        }


        private async Task<DialogTurnResult> EndWithResult(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.EndDialogAsync(
                sc.Result);
        }


        // end the dialog on resume with nevermind choice --> loops the calling waterfalldialog. 

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // a confirm dialog always sets this to yes or no depending on the recognized result.
            var text = dc.Context.Activity.Text;
            if (text == "Yes")
            {

                var foundChoice = new FoundChoice
                {
                    Value = FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.Nevermind
                };
                return await dc.EndDialogAsync(foundChoice);
            }

            // the non overriden function in case of a no response --> reprompts this dialog. 
            await RepromptDialogAsync(dc.Context, dc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }
    }
}
