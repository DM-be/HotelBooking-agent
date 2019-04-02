using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.Email
{
    public class EmailPromptDialog: ComponentDialog
    {
        private static readonly EmailResponses _responder = new EmailResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();

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

            var email = (string) sc.Result; // is never null because of validation
            var updated = false;
            if (sc.Options != null) updated = (bool) sc.Options; // usually true

            var _state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            _state.Email = email;

            if (updated)
                await _responder.ReplyWith(sc.Context, EmailResponses.ResponseIds.HaveUpdatedEmail, email);
            else
                await _responder.ReplyWith(sc.Context, EmailResponses.ResponseIds.HaveEmail, email);

            return await sc.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string EmailPrompt = "emailPrompt";
        }
    }
}
