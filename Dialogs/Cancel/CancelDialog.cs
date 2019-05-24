using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Cancel
{
    public class CancelDialog: ComponentDialog
    {
        private static readonly CancelResponses _responder = new CancelResponses();
        private readonly StateBotAccessors _accessors;

        public CancelDialog(StateBotAccessors accessors)
            : base(nameof(CancelDialog))
        {
            InitialDialogId = nameof(CancelDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var cancel = new WaterfallStep []
            {
                AskToCancel, FinishCancelDialog
            };

            AddDialog(new WaterfallDialog(InitialDialogId, cancel));
            AddDialog(new ConfirmPrompt(DialogIds.CancelPrompt));
        }

        private async Task<DialogTurnResult> AskToCancel(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                DialogIds.CancelPrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, CancelResponses.ResponseIds.CancelPrompt)
                });
        }

        private async Task<DialogTurnResult> FinishCancelDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.EndDialogAsync((bool) sc.Result);
        }

        protected override async Task<DialogTurnResult> EndComponentAsync(DialogContext outerDc, object result, CancellationToken cancellationToken)
        {
            var doCancel = (bool) result;

            if (doCancel)
            {
                // If user chose to cancel
                await _responder.ReplyWith(outerDc.Context, CancelResponses.ResponseIds.CancelConfirmedMessage);
                // todo: only clear relevant state --> check per dialog
                await _accessors.UserState.ClearStateAsync(outerDc.Context); // only for testing, clear only relevant state
                await _accessors.ConversationState.ClearStateAsync(outerDc.Context, cancellationToken).ConfigureAwait(false); // only for testing, clear only relevant state
                return await outerDc.CancelAllDialogsAsync();
            }

            // else if user chose not to cancel
            await _responder.ReplyWith(outerDc.Context, CancelResponses.ResponseIds.CancelDeniedMessage);

            // End this component. Will trigger reprompt/resume on outer stack
            return await outerDc.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string CancelPrompt = "cancelPrompt";
        }
    }
}
