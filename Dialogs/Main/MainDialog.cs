using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.ConfirmOrder;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Main.Delegates;
using HotelBot.Dialogs.Prompts.LocationPrompt;
using HotelBot.Dialogs.RoomDetail;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Dialogs.Shared.RouterDialog;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using HotelBot.Services;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Main
{
    public class MainDialog: RouterDialog
    {
        private const string HotelBotLuisKey = "hotelbot";
        private readonly  StateBotAccessors _accessors;
        private readonly MainResponses _responder = new MainResponses();
        private readonly BotServices _services;
        public IntentHandler _intentHandler = new IntentHandler();


        public MainDialog(BotServices services, StateBotAccessors accessors)
            : base(nameof(MainDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new FetchAvailableRoomsDialog(_services, _accessors));
            AddDialog(new RoomOverviewDialog(_services, _accessors));
            AddDialog(new RoomDetailDialog(_services, accessors));
            AddDialog(new ConfirmOrderDialog(_services, accessors));
            AddDialog(new LocationPromptDialog(accessors));
        }

        protected override async Task RouteAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc.Context.Activity.Text != null)
            {
                var dispatchResult = await _services.DispatchRecognizer.RecognizeAsync<HotelDispatch>(dc.Context, cancellationToken);
                var intent = dispatchResult.TopIntent().intent;
                if (intent == HotelDispatch.Intent.l_HotelBot)
                {

                    _services.LuisServices.TryGetValue(HotelBotLuisKey, out var luisService);
                    if (luisService == null) throw new ArgumentNullException(nameof(luisService));
                    var result = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);
                    var hotelBotIntent = result.TopIntent().intent;

                    if (_intentHandler.MainIntentHandlerDelegates.TryGetValue(hotelBotIntent, out var DelegateAction))
                        DelegateAction(dc, _responder, _accessors, result);
                    else
                    {
                        await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);
                        await SendQuickRepliesBasedOnState(dc.Context, _accessors, _responder);
                    }

                }
                else if (intent.IsQnAIntent())
                {
                    var qnaServiceName = intent.ConvertToQnAServiceName();
                    _services.QnaServices.TryGetValue(qnaServiceName, out var qnaService);
                    if (qnaService == null) throw new ArgumentNullException(nameof(qnaService));
                    var answers = await qnaService.GetAnswersAsync(dc.Context);
                    if (answers != null && answers.Any())
                    {
                        await dc.Context.SendActivityAsync(answers.First().Answer);
                        await SendQuickRepliesBasedOnState(dc.Context, _accessors, _responder);
                    }
                }
                else
                {
                    await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);
                    await SendQuickRepliesBasedOnState(dc.Context, _accessors, _responder);
                }

            }
        }

        protected override async Task OnStartAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(innerDc.Context, () => new UserProfile());

            await _responder.ReplyWith(innerDc.Context, MainResponses.ResponseIds.GreetingWithName, userProfile.FacebookProfileData.First_Name);
            await _responder.ReplyWith(innerDc.Context, MainResponses.ResponseIds.SendGettingStartedQuickReplies);

        }

        protected override async Task CompleteAsync(DialogContext dc, dynamic Result, CancellationToken cancellationToken = default(CancellationToken))
        {
            await SendQuickRepliesBasedOnState(dc.Context, _accessors, _responder);

        }

        public static async Task SendQuickRepliesBasedOnState(ITurnContext context, StateBotAccessors accessors, MainResponses responder, dynamic data = null)
        {
            var roomOverviewState = await accessors.RoomOverviewStateAccessor.GetAsync(context, () => new RoomOverviewState());
            var confirmOrderState = await accessors.ConfirmOrderStateAccessor.GetAsync(context, () => new ConfirmOrderState());

            if (confirmOrderState.PaymentConfirmed)
            {
                await responder.ReplyWith(context, MainResponses.ResponseIds.ConfirmedPaymentQuickReplies, data);
                return;
            }

            if (roomOverviewState.SelectedRooms.Count > 0)
            {
                await responder.ReplyWith(context, MainResponses.ResponseIds.UnconfirmedPaymentQuickReplies, data);
                return;
            }

            await responder.ReplyWith(context, MainResponses.ResponseIds.EmptyRoomOverviewStateQuickReplies, data);
        }

        protected override async Task PaymentConfirmedAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            await SendReceiptAfterPayment(dc.Context, _accessors, _responder);


        }


        public static async Task SendReceiptAfterPayment(ITurnContext context, StateBotAccessors accessors, MainResponses responder)
        {
            var confirmOrderState = await accessors.ConfirmOrderStateAccessor.GetAsync(context, () => new ConfirmOrderState());
            var userProfile = await accessors.UserProfileAccessor.GetAsync(context, () => new UserProfile());
            confirmOrderState.PaymentConfirmed = true;
            var data = new dynamic[2];
            data[0] = confirmOrderState;
            data[1] = userProfile;
            await responder.ReplyWith(context, MainResponses.ResponseIds.SendReceipt, data);
            await responder.ReplyWith(context, MainResponses.ResponseIds.AfterConfirmation);
        }
    }
}
