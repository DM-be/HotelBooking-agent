using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Shared.RecognizerDialogs.ConfirmOrder;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.ConfirmOrder
{
    public class ConfirmOrderDialog: ConfirmOrderRecognizerDialog
    {

        private static readonly ConfirmOrderResponses _responder = new ConfirmOrderResponses();
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public ConfirmOrderDialog(BotServices services, StateBotAccessors accessors): base(services, accessors, nameof(ConfirmOrderDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(ConfirmOrderDialog);
            // name -> email -> adress -> pay button


            var confirmOrderWaterfallSteps = new WaterfallStep []
            {
                InitialStep, ProcessNamePrompt, PromptEmail, ProcessEmail, PromptNumber
            };
            AddDialog(new WaterfallDialog(InitialDialogId, confirmOrderWaterfallSteps));
            AddDialog(new EmailPromptDialog(accessors));


        }



        public async Task<DialogTurnResult> InitialStep(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var userProfile = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());
            var fullName = userProfile.FacebookProfileData.Name;
            return await sc.PromptAsync(
                nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        ConfirmOrderResponses.ResponseIds.UseFacebookName,
                        fullName)
                });

        }

        public async Task<DialogTurnResult> ProcessNamePrompt(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var confirmed = (bool) sc.Result;
            if (confirmed) return await sc.NextAsync();
            // prompt name PROMPT and save to state 
            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> PromptEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.BeginDialogAsync(nameof(EmailPromptDialog));
        }

        public async Task<DialogTurnResult> ProcessEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var email = sc.Result; //always valid email
            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> PromptNumber(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var facebookHelper = new FacebookHelper();
            await facebookHelper.SendPhoneNumberQuickReply(sc.Context);
            return EndOfTurn;
        }
    }
}
