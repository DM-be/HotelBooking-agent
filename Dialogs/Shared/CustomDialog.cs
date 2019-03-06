using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Cancel;
using HotelBot.Dialogs.Main;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Luis;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Graph;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Recognizers.Text.DateTime;

namespace HotelBot.Dialogs.Shared
{
    /// <summary>
    ///  currently supports room booking intents only
    /// --> refactor name or fully generic?
    /// </summary>

    public class CustomDialog : InterruptableDialog
    {
        protected const string LuisResultBookARoomKey = "LuisResult_BookARoom";

        // Fields
        private readonly BotServices _services;

        private readonly StateBotAccessors _accessors;
        private readonly CancelResponses _responder = new CancelResponses();

        public CustomDialog(BotServices services, StateBotAccessors accessors, string dialogId)
            : base(dialogId)
        {
            _services = services;
            _accessors = accessors;
            AddDialog(new CancelDialog());
            AddDialog(new ConfirmPrompt("confirm"));
            var confirmWaterFallSteps = new WaterfallStep[]
            {
                PromptConfirm,
                EndConfirm
            };

            AddDialog(new WaterfallDialog("confirmwaterfall", confirmWaterFallSteps));

            var recheckDateWaterFallSteps = new WaterfallStep []
            {
                PromptRecheckDate, EndRecheckDate,
            };

            AddDialog(new WaterfallDialog("recheckdatewaterfall", recheckDateWaterFallSteps));

        }



        protected override async Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // check luis intent
            _services.LuisServices.TryGetValue("HotelBot", out var luisService);

            if (luisService == null)
            {
                throw new Exception("The specified LUIS Model could not be found in your Bot Services configuration.");
            }
            else
            {
                var luisResult = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);
                var intent = luisResult.TopIntent().intent;

                // Only triggers interruption if confidence level is high
                if (luisResult.TopIntent().score > 0.7)
                {
                    // Add the luis result (intent and entities) for further processing in the derived dialog
                    dc.Context.TurnState.Add(LuisResultBookARoomKey, luisResult);

                    switch (intent)
                    {
                        case HotelBotLuis.Intent.cancel:
                            {
                                return await OnCancel(dc);
                            }

                        case HotelBotLuis.Intent.help:
                            {
                                // todo: provide contextual help
                                return await OnHelp(dc);
                            }
                        case HotelBotLuis.Intent.Update_email:
                        case HotelBotLuis.Intent.Update_ArrivalDate:
                        case HotelBotLuis.Intent.Update_Leaving_Date:
                        case HotelBotLuis.Intent.Update_Number_Of_People:
                            {
                                return await OnUpdate(dc);
                            }
                    }
                }
            }

            return InterruptionStatus.NoAction;
        }

        protected virtual async Task<InterruptionStatus> OnCancel(DialogContext dc)
        {
            if (dc.ActiveDialog.Id != nameof(CancelDialog))
            {
                // Don't start restart cancel dialog
                await dc.BeginDialogAsync(nameof(CancelDialog));

                // Signal that the dialog is waiting on user response
                return InterruptionStatus.Waiting;
            }

            // Else, continue
            return InterruptionStatus.NoAction;
        }

        protected virtual async Task<InterruptionStatus> OnHelp(DialogContext dc)
        {
            var view = new MainResponses();
            await view.ReplyWith(dc.Context, MainResponses.ResponseIds.Help);

            // Signal the conversation was interrupted and should immediately continue
            return InterruptionStatus.Interrupted;
        }

        protected virtual async Task<InterruptionStatus> OnUpdate(DialogContext dc)
        {
            // do not restart running dialog
            if (dc.ActiveDialog.Id != "confirmwaterfall")
            {
                await dc.BeginDialogAsync("confirmwaterfall");
                return InterruptionStatus.Waiting;
            }

            return InterruptionStatus.NoAction;
        }


        public async Task<DialogTurnResult> PromptRecheckDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(BookARoomDialog.DialogIds.ArrivalDateTimePrompt, new PromptOptions()
            {
                Prompt = MessageFactory.Text("That is not a date i can understand, please resend date"),
            });
        }

        public async Task<DialogTurnResult> EndRecheckDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var date = sc.Result;
            sc.Context.TurnState.Add("correctDate", date);
            return await sc.ReplaceDialogAsync("confirmwaterfall");
        }





        public async Task<DialogTurnResult> PromptConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {


            // added to initial step only
            sc.Context.TurnState.TryGetValue(LuisResultBookARoomKey, out var value);
            var luisResult = value as HotelBotLuis;

            if (luisResult.HasEntityWithPropertyName(EntityNames.ArrivalDate))
            {
                if (luisResult.Entities.datetime.First().Type != "date")
                {
                    return await sc.ReplaceDialogAsync("recheckdatewaterfall");
                }

            }

            // set values for waterfallsteps
            sc.Values.Add(LuisResultBookARoomKey, luisResult);

            var _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            var view = new BookARoomResponses();


            // todo refactor
            dynamic data = new dynamic[2];
            data[0] = luisResult;
            data[1] = _state;

            // TODO: check for empty entities here


            

            return await sc.PromptAsync(
                "confirm",
                new PromptOptions
                {
                    Prompt = await view.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.UpdateText, data),
                });
        }

        public async Task<DialogTurnResult> EndConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            // replace the current dialog with itself --> in this case the bookaroomdialog will be restarted
            // creates a loop --> skips waterfall steps when state is filled in
            sc.Values.TryGetValue(LuisResultBookARoomKey, out var value);
            var luisResult = value as HotelBotLuis;
            var confirmed = (bool)sc.Result;
            if (confirmed)
            {
                UpdateState(luisResult, sc);
            }
            return await sc.ReplaceDialogAsync(InitialDialogId);
        }

        private async void UpdateState(HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            var _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            switch (luisResult.TopIntent().intent)
            {
                case HotelBotLuis.Intent.Update_email:
                    if (luisResult.HasEntityWithPropertyName(EntityNames.Email))
                    {
                        _state.Email = luisResult.Entities.email.First();
                    }
                    else // intent matched but no matching entity
                    {
                        // set to null to allow for waterfall to reprompt for the email 
                        _state.Email = null;
                    }
                    break;
                case HotelBotLuis.Intent.Update_Number_Of_People:
                {
                    if (luisResult.HasEntityWithPropertyName(EntityNames.NumberOfPeople))
                    {
                        _state.NumberOfPeople = luisResult.Entities.number.First();
                    }
                    else
                    {
                        _state.NumberOfPeople = null;
                    }
                    break;
                }
                case HotelBotLuis.Intent.Update_ArrivalDate:
                {
                    if (luisResult.HasEntityWithPropertyName(EntityNames.ArrivalDate))
                    {
                        //todo: do not allow changing date to "daterange" type, only allow "date";
                        var spec = luisResult.Entities.datetime.First().Expressions.First();
                        var resolution = DateTimeRecognizer.RecognizeDateTime(spec, Culture.English);

                        var results = DateTimeRecognizer.RecognizeDateTime(spec, Culture.English);
                        var dateTimeResolutions = new List<DateTimeResolution>();


                        var values = (List<Dictionary<string, string>>)results[0].Resolution["values"];
                        foreach (var value in values)
                        {
                            dateTimeResolutions.Add(ReadResolution(value));
                        }

                        var test = dateTimeResolutions;

                        //  var res = luisResult.ConvertDateTimeSpec(spec);
                        // _state.ArrivalDate = luisResult.Entities.datetime.First();
                    }
                    else
                    {
                        _state.ArrivalDate = null;
                    }
                    break;
                }
                case HotelBotLuis.Intent.Update_Leaving_Date:
                    break;
            }

            await _accessors.BookARoomStateAccessor.SetAsync(sc.Context, _state);
        }

        private DateTimeResolution ReadResolution(IDictionary<string, string> resolution)
        {
            var result = new DateTimeResolution();

            if (resolution.TryGetValue("timex", out var timex))
            {
                result.Timex = timex;
            }

            if (resolution.TryGetValue("value", out var value))
            {
                result.Value = value;
            }

            if (resolution.TryGetValue("start", out var start))
            {
                result.Start = start;
                result.Start = start;
            }

            if (resolution.TryGetValue("end", out var end))
            {
                result.End = end;
            }

            return result;
        }
    }

    public class EntityNames
    {
        public const string Email = "email";
        public const string NumberOfPeople = "number";
        public const string ArrivalDate = "datetime";
        public const string LeavingDate = "datetime";
    }

}