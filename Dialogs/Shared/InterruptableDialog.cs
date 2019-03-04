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
                await dc.RepromptDialogAsync().ConfigureAwait(false);
                return EndOfTurn;
            }

            // set to waiting when waiting for a response
            else if (status == InterruptionStatus.Waiting)
            {
                // Stack is already waiting for a response, shelve inner stack
                return EndOfTurn;
            }
            // status is no action when sending "no"
            return await base.OnContinueDialogAsync(dc, cancellationToken);
        }

        protected abstract Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken);

    }
}
