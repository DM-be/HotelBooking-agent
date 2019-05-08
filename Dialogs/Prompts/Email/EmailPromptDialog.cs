using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.Email
{
    public class EmailPromptDialog: ComponentDialog
    {
        private static readonly PromptValidatorResponses _responder = new PromptValidatorResponses();
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

        }


        private async Task<DialogTurnResult> AskForEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var firstName = sc.Options;
            var facebookHelper = new FacebookHelper();
            await facebookHelper.SendEmailQuickReply(sc.Context, firstName);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> FinishEmailDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {


            var email = (string) sc.Result; //todo: add validation
            if (!PromptValidators.IsValidEmailAddress(email))
            {
                await _responder.ReplyWith(sc.Context, PromptValidatorResponses.ResponseIds.InvalidEmail);
                return await sc.ReplaceDialogAsync(InitialDialogId);
            }

            return await sc.EndDialogAsync(email);
        }
    }
}
