using System.Threading;
using System.Threading.Tasks;
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

           
            return await base.OnContinueDialogAsync(dc, cancellationToken);
        }

        protected abstract Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken);
    }
}
