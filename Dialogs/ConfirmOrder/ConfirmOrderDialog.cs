using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.RoomOverview;
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
            var confirmOrderWaterfallSteps = new WaterfallStep []
            {
                InitialStep, ProcessNamePrompt, PromptEmail, ProcessEmail, PromptNumber, ProcessNumber, SendConfirmation
            };
            AddDialog(new WaterfallDialog(InitialDialogId, confirmOrderWaterfallSteps));
            AddDialog(new EmailPromptDialog(accessors));


        }



        public async Task<DialogTurnResult> InitialStep(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            if (sc.Options != null) return await sc.NextAsync();


            //setup state object and overwrite roomoverviewstate
            var roomOverviewState = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            var confirmOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            confirmOrderState.RoomOverviewState = roomOverviewState;


            // TODO: move quickreplies into confirmorderresponses?
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());
            var fullName = userProfile.FacebookProfileData.Name;
            var facebookHelper = new FacebookHelper();
            await facebookHelper.SendFullNameQuickReply(sc.Context, fullName);
            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        public async Task<DialogTurnResult> ProcessNamePrompt(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            if (sc.Options != null) return await sc.NextAsync();

            var state = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            var name = (string) sc.Result;
            state.FullName = name;
            var firstName = name.Split(' ')[0];

            //    if (confirmed) return await sc.NextAsync();
            // prompt name PROMPT and save to state 
            return await sc.NextAsync(firstName);
        }

        public async Task<DialogTurnResult> PromptEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            if (sc.Options != null) return await sc.NextAsync();
            var firstName = sc.Result;
            return await sc.BeginDialogAsync(nameof(EmailPromptDialog), firstName);
        }

        public async Task<DialogTurnResult> ProcessEmail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            if (sc.Options != null) return await sc.NextAsync();

            var state = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());

            var email = sc.Result as string; //always valid email
            state.Email = email;

            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> PromptNumber(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            if (sc.Options != null) return await sc.NextAsync();
            var facebookHelper = new FacebookHelper();
            await facebookHelper.SendPhoneNumberQuickReply(sc.Context);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        public async Task<DialogTurnResult> ProcessNumber(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            // needs validation 
            if (sc.Options != null) return await sc.NextAsync();
            var phoneNumber = sc.Result as string;
            var roomOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            roomOrderState.Number = phoneNumber;


            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.SendPaymentCard, roomOrderState);
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
            // end and propose location, wifi password, number etc? give more info after confirmation of payment
            return EndOfTurn;

        }
    }
}
