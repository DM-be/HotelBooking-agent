
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Shared;
using HotelBot.Dialogs.SlotFillingDialog;
using HotelBot.Middleware;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.CodeAnalysis;
using Microsoft.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json.Linq;


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
            AddDialog(new BookARoomDialog(_services, _accessors));
           

            var fullname_slots = new List<SlotDetails>
            {
                new SlotDetails("first", "text", "Please enter your first name."),
                new SlotDetails("last", "text", "Please enter your last name."),
            };
            AddDialog(new SlotFillingDialog.SlotFillingDialog(services, _accessors, fullname_slots));
            AddDialog(new TextPrompt("text"));
          

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
                        throw new ArgumentNullException(nameof(luisService));
                    }
                    else
                    {
                        var result = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);

                        var hotelBotIntent = result?.TopIntent().intent;

                        // switch on general intents
                        switch (hotelBotIntent)
                        {
                            // delegates  -  reflection 
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
                                    await dc.BeginDialogAsync(nameof(SlotFillingDialog.SlotFillingDialog));
                                    //await _responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Help);
                                    break;
                                }
                            case HotelBotLuis.Intent.book_a_room:
                            {
                                
                                await dc.BeginDialogAsync(nameof(BookARoomDialog));
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

                // TODO: refactor with classes --> create qnaname member
                else if (intent.ToString().StartsWith("q_"))
                {
                    var intentString = intent.ToString();
                    var qnaName = intentString.Substring(2);
                    _services.QnaServices.TryGetValue(qnaName, out var qnaService);

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

        private void FromLuisResults(RecognizerResult luisResults)
        {
            string [] allEntities =
            {
                "datetime"
            };

            foreach (var entity in allEntities)
            {
                var value = luisResults.Entities.SelectTokens(entity).FirstOrDefault();
                if (value == null)
                {
                    continue;
                }

                object property = null;
                var val = value.First();
                if (val.Type == JTokenType.Object)
                {
                    var obj = (JObject) val;
                    if (obj["type"].ToString() == "datetime")
                    {
                        property = val;
                    }
                }

                // continue for each entity



            }
        }
    }
}
