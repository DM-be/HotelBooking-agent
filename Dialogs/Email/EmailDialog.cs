using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Cancel;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Email
{
    public class EmailDialog : ComponentDialog
    {
        private static readonly EmailResponses _responder = new EmailResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();

        public EmailDialog(StateBotAccessors accessors)
            : base(nameof(EmailDialog))
        {
            InitialDialogId = nameof(CancelDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForEmailWaterfallSteps = new WaterfallStep[]
            {
                AskForEmail, FinishEmailDialog
            };

            AddDialog(new WaterfallDialog(InitialDialogId, askForEmailWaterfallSteps));
            AddDialog(new TextPrompt(DialogIds.EmailPrompt, _promptValidators.EmailValidatorAsync));
        }


        private async Task<DialogTurnResult> AskForEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                DialogIds.EmailPrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, EmailResponses.ResponseIds.EmailPrompt)
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> FinishEmailDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.EndDialogAsync((string) sc.Result);
        }

        protected override async Task<DialogTurnResult> EndComponentAsync(DialogContext outerDc, object result, CancellationToken cancellationToken)
        {
            var email = (string) result;

            var _state = await _accessors.BookARoomStateAccessor.GetAsync(outerDc.Context, () => new BookARoomState());
            _state.Email = email;

            await _responder.ReplyWith(outerDc.Context, EmailResponses.ResponseIds.HaveEmail, email);

            // End this component. Will trigger reprompt/resume on outer stack
            return await outerDc.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string EmailPrompt = "emailPrompt";
        }
    }
}
