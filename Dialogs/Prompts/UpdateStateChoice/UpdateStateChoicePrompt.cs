
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.UpdateStateChoice
{
    public class UpdateStateChoicePrompt: ComponentDialog
    {
        private static readonly FetchAvailableRoomsResponses _responder = new FetchAvailableRoomsResponses();

        public UpdateStateChoicePrompt(): base(nameof(UpdateStateChoicePrompt))
        {
            InitialDialogId = nameof(UpdateStateChoicePrompt);
            var updateStateChoiceWaterfallSteps = new WaterfallStep []
            {
                PromptUpdateStateChoices, EndWithResult
            };

            AddDialog(new WaterfallDialog(InitialDialogId, updateStateChoiceWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }


        private async Task<DialogTurnResult> PromptUpdateStateChoices(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        FetchAvailableRoomsResponses.ResponseIds.UpdatePrompt), // OWN RESPONSES!
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.Checkin,
                            FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.Checkout,
                            FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.NumberOfPeople

                        })
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> EndWithResult(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.EndDialogAsync(
                sc.Result);
        }

        public class Choices {

        }



        // this prompt only resumes after a interruption prompt has ended.
        // the only interruptions handle state directly, on resume we should end the dialog
        //  state is auto updated, no confirmation needed, see recognizer dialog
        // --> this prompt resumes --> overrides into an endDialog.
         
        // the underlying luis can trigger an updatestate prompt, state would be set directly.
        // the updatestate prompt ends and this resumedialog function is called.
        // the only updates to state this dialog can do is the 3 options prompted, because this current prompt is a statechoiceprompt, confirmation will also be skipped.
        // calling resume should end the dialog and skip this entirely, we pass a new found choice to the underlying dialog (fetchavailableroomsdialog)


        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null,
            CancellationToken cancellationToken = new CancellationToken())
        {

            return dc.EndDialogAsync();
        }
    }
}
