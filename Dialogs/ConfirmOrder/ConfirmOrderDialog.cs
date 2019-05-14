using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Dialogs.Shared.RecognizerDialogs.ConfirmOrder;
using HotelBot.Models.Wrappers;
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
            var confirmOrderWaterfallSteps = new WaterfallStep []
            {
                InitialStep, ProcessNamePrompt, PromptEmail, ProcessEmail, PromptNumber, ProcessNumber, SendConfirmation
            };
            AddDialog(new WaterfallDialog(InitialDialogId, confirmOrderWaterfallSteps));
            AddDialog(new EmailPromptDialog(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }



        public async Task<DialogTurnResult> InitialStep(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var dialogOptions = sc.Options as DialogOptions;
            if (dialogOptions != null)
                if (dialogOptions.ConfirmedPayment)
                    return await sc.NextAsync();


            //setup state object and overwrite roomoverviewstate
            var roomOverviewState = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            var confirmOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            confirmOrderState.RoomOverviewState = roomOverviewState;

            // TODO: move quickreplies into confirmorderresponses?
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());
            var fullName = userProfile.FacebookProfileData.Name;
            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.SendFullNameQuickReply, fullName);
            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        //TODO: add name validation
        public async Task<DialogTurnResult> ProcessNamePrompt(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var dialogOptions = sc.Options as DialogOptions;
            if (dialogOptions != null)
                if (dialogOptions.ConfirmedPayment)
                    return await sc.NextAsync();
            var state = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            var name = (string) sc.Result;
            state.FullName = name;
            var firstName = name.Split(' ')[0];
            return await sc.NextAsync(firstName);
        }

        public async Task<DialogTurnResult> PromptEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var dialogOptions = sc.Options as DialogOptions;
            if (dialogOptions != null)
                if (dialogOptions.ConfirmedPayment)
                    return await sc.NextAsync();
            var firstName = sc.Result;
            return await sc.BeginDialogAsync(nameof(EmailPromptDialog), firstName);
        }

        public async Task<DialogTurnResult> ProcessEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var dialogOptions = sc.Options as DialogOptions;
            if (dialogOptions != null)
                if (dialogOptions.ConfirmedPayment)
                    return await sc.NextAsync();
            var state = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            var email = sc.Result as string; //always valid because of emailpromptdialog
            state.Email = email;
            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> PromptNumber(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var dialogOptions = sc.Options as DialogOptions;
            if (dialogOptions != null)
                if (dialogOptions.ConfirmedPayment)
                    return await sc.NextAsync();
            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.SendPhoneNumberQuickReply);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        public async Task<DialogTurnResult> ProcessNumber(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            // needs validation 
            var dialogOptions = sc.Options as DialogOptions;
            if (dialogOptions != null)
                if (dialogOptions.ConfirmedPayment)
                    return await sc.NextAsync();
            var phoneNumber = sc.Result as string;
            var roomOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            roomOrderState.Number = phoneNumber;

            await sc.Context.SendActivityAsync("Thank you for your information.");

            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.SendPaymentCard, roomOrderState);
            await sc.Context.SendActivityAsync("Tap pay to complete your booking with us.");
            return EndOfTurn;

        }

        public async Task<DialogTurnResult> SendConfirmation(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            // needs validation 

            var roomOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            var userProfileState = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());

            dynamic [] data =
            {
                roomOrderState, userProfileState
            };
            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.SendReceipt, data);
            roomOrderState.PaymentConfirmed = true;
            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.AfterConfirmation);
            return await sc.EndDialogAsync();
        }
    }
}
