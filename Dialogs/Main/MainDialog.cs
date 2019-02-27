
using System;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Shared;
using HotelBot.Middleware;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.CodeAnalysis;
using Microsoft.CognitiveServices.ContentModerator.Models;


namespace HotelBot.Dialogs.Main
{
    public class MainDialog : RouterDialog
    {
        private BotServices _services;
        private StateBotAccessors _accessors;
      
        private MainResponses _responder = new MainResponses();
        private FacebookHelper _facebookHelper = new FacebookHelper();

        public MainDialog(BotServices services, StateBotAccessors accessors)
            : base(nameof(MainDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            // TODO: add dialogs based on intent here, ie get location dialog or book a room dialog
        }

        protected override async Task OnStartAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var view = new MainResponses();
            await view.ReplyWith(dc.Context, MainResponses.ResponseIds.Intro);
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
                    _services.LuisServices.TryGetValue("HotelBot", out var luisService);

                    if (luisService == null)
                    {
                        throw new Exception("The specified LUIS Model could not be found in your Bot Services configuration.");
                    }
                    else
                    {
                        var result = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);

                        var hotelBotIntent = result?.TopIntent().intent;

                        // switch on general intents
                        switch (hotelBotIntent)
                        {
                            case HotelBotLuis.Intent.cancel:
                                {
                                    // send cancelled response
                                    await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Cancelled);

                                    // Cancel any active dialogs on the stack
                                    await dc.CancelAllDialogsAsync();
                                    break;
                                }

                            case HotelBotLuis.Intent.help:
                                {
                                    // send help response
                                   
                                    await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Help);
                                    break;
                                }
                            case HotelBotLuis.Intent.book_a_room:
                            {

                                await dc.Context.SendActivityAsync("book a room intent");
                                break;
                            }
                            case HotelBotLuis.Intent.get_directions:
                            {
                          
                                 await _facebookHelper.SendLocationQuickReply(dc.Context);
                                 break;
                            }
                            case HotelBotLuis.Intent.call_us:
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
                }
                else if (intent == HotelDispatch.Intent.q_HotelBotTest_qna_en_US)
                {
                    _services.QnaServices.TryGetValue("HotelBotTest_qna_en_US", out var qnaService);

                    if (qnaService == null)
                    {
                        throw new Exception("The specified QnA Maker Service could not be found in your Bot Services configuration.");
                    }
                    else
                    {
                        var answers = await qnaService.GetAnswersAsync(dc.Context);

                        if (answers != null && answers.Count() > 0)
                        {
                            await dc.Context.SendActivityAsync(answers[0].Answer);
                        }
                    }
                }
                else if (intent == HotelDispatch.Intent.q_HotelBotTest_qna_nl_BE)
                {
                    _services.QnaServices.TryGetValue("HotelBotTest_qna_nl_BE", out var qnaService);

                    if (qnaService == null)
                    {
                        throw new Exception("The specified QnA Maker Service could not be found in your Bot Services configuration.");
                    }
                    else
                    {
                        var answers = await qnaService.GetAnswersAsync(dc.Context);

                        if (answers != null && answers.Count() > 0)
                        {
                            await dc.Context.SendActivityAsync(answers[0].Answer);
                        }
                    }
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
            // TODO: check for quick reply here?
            
           
        }

        protected override async Task CompleteAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // The active dialog's stack ended with a complete status
            await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Completed);
        }
    }
}
