using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.UpdateStateChoice
{
    public class UpdateStateChoicePrompt: ComponentDialog
    {
        private static readonly FetchAvailableRoomsResponses _responder = new FetchAvailableRoomsResponses();
        private readonly StateBotAccessors _accessors;

        public UpdateStateChoicePrompt(StateBotAccessors accessors): base(nameof(UpdateStateChoicePrompt))
        {
            InitialDialogId = nameof(UpdateStateChoicePrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
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
                        FetchAvailableRoomsResponses.ResponseIds.UpdatePrompt),
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



        // this prompt only resumes after a interruption prompt has ended.
        // the only interruptions handle state directly, on resume we should end the dialog
        // ie --> would you change something --> yes i want to update my arrival date to x --> skips updatestateprompt confirmation
        // --> this prompt resumes --> overrides into an endDialog.

        // the underlying luis can trigger and updatestate prompt, state would be set directly.
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
