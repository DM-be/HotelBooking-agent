using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Main.Delegates;
using HotelBot.Dialogs.RoomDetail;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Dialogs.Shared.RouterDialog;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Main
{
    public class MainDialog: RouterDialog
    {
        private const string HotelBotLuisKey = "hotelbot";
        private readonly StateBotAccessors _accessors;
        private readonly FacebookHelper _facebookHelper = new FacebookHelper();
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
                        DelegateAction(dc, _responder, _facebookHelper, _accessors, result);
                    else
                        await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);

                }
                else if (intent.IsQnAIntent())
                {
                    var qnaServiceName = intent.ConvertToQnAServiceName();
                    _services.QnaServices.TryGetValue(qnaServiceName, out var qnaService);
                    if (qnaService == null) throw new ArgumentNullException(nameof(qnaService));
                    var answers = await qnaService.GetAnswersAsync(dc.Context);
                    if (answers != null && answers.Any()) await dc.Context.SendActivityAsync(answers.First().Answer);
                }
                else
                {
                    await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);
                }

            }
        }


        protected override async Task CompleteAsync(DialogContext dc, dynamic Result, CancellationToken cancellationToken = default(CancellationToken))
        {
            // propagates to routerdialog in the await continueasync
            await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Completed);
        }
    }
}
