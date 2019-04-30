using System.Threading;
using System.Threading.Tasks;
using HotelBot.Models.Wrappers;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared
{
    public abstract class InterruptableDialog: ComponentDialog
    {
        public InterruptableDialog(string dialogId): base(dialogId)
        {
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var status = await OnDialogInterruptionAsync(dc, cancellationToken);

            if (status == InterruptionStatus.Interrupted)
            {
                // Resume the waiting dialog after interruption
                await dc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
                return EndOfTurn;
            }

            // set to waiting when waiting for a response

            if (status == InterruptionStatus.Waiting) return EndOfTurn;

            if (status == InterruptionStatus.Route)
            {

                var dialogResult = dc.Context.TurnState.Get<DialogResult>("dialogResult");

                return await dc.EndDialogAsync(dialogResult);
                // return new DialogTurnResult(DialogTurnStatus.Waiting);

            }

            // status is no action when sending "no"
            return await base.OnContinueDialogAsync(dc, cancellationToken);
        }

        protected abstract Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken);
    }
}
