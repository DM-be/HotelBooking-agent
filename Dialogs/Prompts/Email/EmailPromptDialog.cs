using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.Email
{
    public class EmailPromptDialog: ComponentDialog
    {
        private static readonly EmailResponses _responder = new EmailResponses();
        private readonly StateBotAccessors _accessors;

        public EmailPromptDialog(StateBotAccessors accessors)
            : base(nameof(EmailPromptDialog))
        {
            InitialDialogId = nameof(EmailPromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForEmailWaterfallSteps = new WaterfallStep []
            {
                AskForEmail, FinishEmailDialog
            };

            AddDialog(new WaterfallDialog(InitialDialogId, askForEmailWaterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

        }


        private async Task<DialogTurnResult> AskForEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var firstName = sc.Options;
            return await sc.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, EmailResponses.ResponseIds.SendEmailQuickReplyWithName, firstName)
            });

        }

        private async Task<DialogTurnResult> FinishEmailDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {


            var email = (string) sc.Result;
            if (!PromptValidators.IsValidEmailAddress(email))
            {
                await _responder.ReplyWith(sc.Context, PromptValidatorResponses.ResponseIds.InvalidEmail);
                return await sc.ReplaceDialogAsync(InitialDialogId);
            }

            return await sc.EndDialogAsync(email);
        }
    }
}
