using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Shared;
using HotelBot.Extensions;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Main
{
    public class MainDialog: RouterDialog
    {
        private readonly StateBotAccessors _accessors;
        private readonly FacebookHelper _facebookHelper = new FacebookHelper();

        private readonly MainResponses _responder = new MainResponses();
        private readonly BotServices _services;


        public MainDialog(BotServices services, StateBotAccessors accessors)
            : base(nameof(MainDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new BookARoomDialog(_services, _accessors));

        }


        protected override async Task RouteAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check dispatch result
            if (dc.Context.Activity.Text != null)
            {
                var dispatchResult = await _services.DispatchRecognizer.RecognizeAsync<HotelDispatch>(dc.Context, cancellationToken);
                var intent = dispatchResult.TopIntent().intent;


                if (intent == HotelDispatch.Intent.l_HotelBot)
                {
                    // If dispatch result is hotelbot luis model
                    _services.LuisServices.TryGetValue("hotelbot", out var luisService);

                    if (luisService == null) throw new ArgumentNullException(nameof(luisService));

                    var result = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);

                    var hotelBotIntent = result?.TopIntent().intent;

                    // switch on general intents
                    switch (hotelBotIntent)
                    {
                        // delegates  -  reflection 
                        case HotelBotLuis.Intent.Cancel:
                        {
                            // send cancelled response
                            await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Cancelled);

                            // Cancel any active dialogs on the stack
                            await dc.CancelAllDialogsAsync();
                            break;
                        }

                        case HotelBotLuis.Intent.Help:
                        {

                            await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Help);
                            break;
                        }
                        case HotelBotLuis.Intent.Book_A_Room:
                        {
                            await UpdateState(dc, result);
                            await dc.BeginDialogAsync(nameof(BookARoomDialog));
                            break;
                        }
                        case HotelBotLuis.Intent.Get_Directions:
                        {

                            await _facebookHelper.SendLocationQuickReply(dc.Context);
                            break;
                        }
                        case HotelBotLuis.Intent.Get_Location:
                        {
                            await _facebookHelper.SendDirectionsWithoutOrigin(dc.Context, null);
                            break;
                        }
                        case HotelBotLuis.Intent.Call_Us:
                        {
                            await _facebookHelper.SendCallMessage(dc.Context);
                            break;
                        }
                        case HotelBotLuis.Intent.None:
                        default:
                        {
                            // No intent was identified, send confused message
                            await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);
                            break;
                        }
                    }
                }

                // TODO: refactor with classes --> create qnaname member
                else if (intent.ToString().StartsWith("q_"))
                {
                    var intentString = intent.ToString();
                    var qnaName = intentString.Substring(2);
                    _services.QnaServices.TryGetValue(qnaName, out var qnaService);

                    if (qnaService == null) throw new Exception("The specified QnA Maker Service could not be found in your Bot Services configuration.");

                    var answers = await qnaService.GetAnswersAsync(dc.Context);

                    if (answers != null && answers.Any()) await dc.Context.SendActivityAsync(answers[0].Answer);
                }

                else
                {
                    // If dispatch intent does not map to configured models, send "confused" response.
                    await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);
                }

            }


        }

        protected override async Task OnEventAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
        }

        protected override async Task CompleteAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // The active dialog's stack ended with a complete status
            await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Completed);
        }



        //todo: refactor
        private async Task UpdateState(DialogContext dc, HotelBotLuis luisResult)
        {
            var bookARoomState = await _accessors.BookARoomStateAccessor.GetAsync(dc.Context, () => new BookARoomState());

            switch (luisResult.TopIntent().intent)
            {
                case HotelBotLuis.Intent.Book_A_Room:
                    if (luisResult.HasEntityWithPropertyName(EntityNames.Email))
                        bookARoomState.Email = luisResult.Entities.email.First();
                    if (luisResult.HasEntityWithPropertyName(EntityNames.Number))
                        bookARoomState.NumberOfPeople = luisResult.Entities.number.First();
                    if (luisResult.HasEntityWithPropertyName(EntityNames.Datetime))
                        if (luisResult.Entities.datetime.First().Type == "date")
                        {
                            var dateTimeSpecs = luisResult.Entities.datetime.First();
                            var firstExpression = dateTimeSpecs.Expressions.First();
                            bookARoomState.ArrivalDate = new TimexProperty(firstExpression);
                        }

                    break;
            }
        }
    }
}
