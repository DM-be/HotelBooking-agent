using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Custom;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Shared;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace HotelBot.Dialogs.SlotFillingDialog
{
    //TODO: implement interruptable
    public class SlotFillingDialog : CustomDialog
    {

        private const string SlotName = "slot";
        private const string PersistedValues = "values";
        private readonly StateBotAccessors _accessors;



        private readonly List<SlotDetails> _slots;
        public SlotFillingDialog(BotServices botServices, StateBotAccessors accessors, List<SlotDetails> slots)
            : base(botServices, nameof(SlotFillingDialog))
        {
            _slots = slots ?? throw new ArgumentNullException(nameof(slots));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(SlotFillingDialog);
            AddDialog(new TextPrompt("Email"));
        }

       

        /// <summary>
        /// This helper function contains the core logic of this dialog. The main idea is to compare the state we have gathered with the
        /// list of slots we have been asked to fill. When we find an empty slot we execute the corresponding prompt.
        /// </summary>
        /// <param name="dialogContext">A handle on the runtime.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A DialogTurnResult indicating the state of this dialog to the caller.</returns>
        private async Task<DialogTurnResult> RunPromptAsync(DialogContext dialogContext, CancellationToken cancellationToken)
        {

            // reflection to get all string properties with getters/setters

            dialogContext.Dialogs.Add(new TextPrompt("Email"));

            var stateProperty = await _accessors.BookARoomStateAccessor.GetAsync(dialogContext.Context, () => new BookARoomState());

            foreach (PropertyInfo pinfo in stateProperty.GetType().GetProperties())
            {
                object value = pinfo.GetValue(stateProperty, null);

                if (value == null)
                {
                    var unfilledSlotName = pinfo.Name;

                    return await dialogContext.BeginDialogAsync(unfilledSlotName, new PromptOptions(), cancellationToken);
                }
            }

            
                // No more slots to fill so end the dialog.
                return await dialogContext.EndDialogAsync();
        }


        private async Task<DialogTurnResult> FinishDialog(DialogContext dialogContext, CancellationToken cancellationToken)
        {

            return await dialogContext.EndDialogAsync();
        }


        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            // Don't do anything for non-message activities.
            if (dialogContext.Context.Activity.Type != ActivityTypes.Message)
            {
                return await dialogContext.EndDialogAsync(new Dictionary<string, object>());
            }

            // Run prompt
            return await RunPromptAsync(dialogContext, cancellationToken);
        }

        /// <summary>
        /// Continue is called to run an existing dialog. It will return the state of the current dialog. If there is no dialog it will return Empty.
        /// </summary>
        /// <param name="dialogContext">A handle on the runtime.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A DialogTurnResult indicating the state of this dialog to the caller.</returns>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dialogContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            // Don't do anything for non-message activities.
            if (dialogContext.Context.Activity.Type != ActivityTypes.Message)
            {
                return EndOfTurn;
            }

            // Run next step with the message text as the result.
            return await RunPromptAsync(dialogContext, cancellationToken);
        }



    }
}
