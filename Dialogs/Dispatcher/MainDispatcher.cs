using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Main;
using HotelBot.Services;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.BotBuilderSamples;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;

namespace HotelBot.Dialogs.Dispatcher
{
    /// <summary>
    /// The <see cref="MainDialog"/> is first dialog that runs after a user begins a conversation.
    /// </summary>
    /// <remarks>
    /// The <see cref="MainDialog"/> responsibility is to:
    /// - Start message.
    ///   Display the initial message the user sees when they begin a conversation.
    /// - Help.
    ///   Provide the user about the commands the bot can process.
    /// - Start other dialogs to perform more complex operations.
    ///   Begin the <see cref="GreetingDialog"/> if the user greets the bot, which will
    ///   prompt the user for name and city.
    /// </remarks>
    public class MainDispatcher : ComponentDialog
    {
 


        private BotServices _services;
        private StateBotAccessors _accessors;

        private readonly DialogSet _dialogs;

        public MainDispatcher(BotServices services, StateBotAccessors accessors)
                    : base(nameof(MainDispatcher))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            // Add dialogs
            _dialogs = new DialogSet(_accessors.DialogStateAccessor);
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await MainDispatchAsync(innerDc);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await MainDispatchAsync(innerDc);
        }

        // This method examines the incoming turn property to determine:
        // 1. If the requested operation is permissible - e.g. if user is in middle of a dialog,
        //     then an out of order reply should not be allowed.
        // 2. Calls any outstanding dialogs to continue.
        // 3. If results is no-match from outstanding dialog .OR. if there are no outstanding dialogs,
        //    decide which child dialog should begin and start it.
        protected async Task<DialogTurnResult> MainDispatchAsync(DialogContext innerDc)
        {
            var context = innerDc.Context;

            // Get on turn property through the property accessor.
            
 
            // Evaluate if the requested operation is possible/ allowed.
            var activeDialog = (innerDc.ActiveDialog != null) ? innerDc.ActiveDialog.Id : string.Empty;
         

            // Continue outstanding dialogs.
            var dialogTurnResult = await innerDc.ContinueDialogAsync();

            // This will only be empty if there is no active dialog in the stack.
            // Removing check for dialogTurnStatus here will break successful cancellation of child dialogs.
            // E.g. who are you -> cancel -> yes flow.
            if (!context.Responded && dialogTurnResult != null && dialogTurnResult.Status != DialogTurnStatus.Complete)
            {
                // No one has responded so start the right child dialog.
               
             //  dialogTurnResult = await BeginChildDialogAsync(innerDc, _accessors);
            }

            if (dialogTurnResult == null)
            {
                return await innerDc.EndDialogAsync();
            }

            // Examine result from dc.continue() or from the call to beginChildDialog().
            switch (dialogTurnResult.Status)
            {
                case DialogTurnStatus.Complete:
                    // The active dialog finished successfully. Ask user if they need help with anything else.
                    await context.SendActivityAsync("Is there anything else I can help you with ?");
                    break;

                case DialogTurnStatus.Waiting:
                    // The active dialog is waiting for a response from the user, so do nothing
                    break;

                case DialogTurnStatus.Cancelled:
                    // The active dialog's stack has been canceled
                    await innerDc.CancelAllDialogsAsync();
                    break;
            }

            return dialogTurnResult;
        }

      
      

        
    }
}
