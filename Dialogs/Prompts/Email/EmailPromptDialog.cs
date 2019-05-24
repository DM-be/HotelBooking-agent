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

        public EmailPromptDialog(StateBotAccessors accessors)
            : base(nameof(EmailPromptDialog))
        {
            InitialDialogId = nameof(EmailPromptDialog);
            if (accessors == null)
            {
                throw new ArgumentNullException(nameof(accessors));
            }
            var askForEmailWaterfallSteps = new WaterfallStep []
            {
                AskForEmailAsync, FinishEmailDialogAsync
            };

            AddDialog(new WaterfallDialog(InitialDialogId, askForEmailWaterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

        }

        private async Task<DialogTurnResult> AskForEmailAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var firstName = sc.Options;
            return await sc.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, EmailResponses.ResponseIds.SendEmailQuickReplyWithName, firstName)
            });

        }

        private async Task<DialogTurnResult> FinishEmailDialogAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
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
