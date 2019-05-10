﻿using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Cancel;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.FetchAvailableRoomsIntroduction;
using HotelBot.Dialogs.Prompts.UpdateState;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.RecognizerDialogs.FetchAvailableRooms
{

    public class FetchAvailableRoomsRecognizerDialog: InterruptableDialog

    {
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public FetchAvailableRoomsRecognizerDialog(BotServices services, StateBotAccessors accessors, string dialogId)
            : base(dialogId)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new CancelDialog(_accessors));
            AddDialog(new UpdateStatePrompt(accessors));
        }

        protected override async Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var text = dc.Context.Activity.Text;
            if (FetchAvailableRoomsDialog.FetchAvailableRoomsChoices.Choices.Contains(text)) return InterruptionStatus.NoAction;

            var skipRecognize = dc.ActiveDialog.Id == nameof(FetchAvailableRoomsIntroductionPrompt); 
            if (skipRecognize) return InterruptionStatus.NoAction;

            _services.LuisServices.TryGetValue("hotelbot", out var luisService);
            if (luisService == null) throw new Exception("The specified LUIS Model could not be found in your Bot Services configuration.");
            var luisResult = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);
            var intent = luisResult.TopIntent().intent;


            if (luisResult.TopIntent().score > 0.80)
                switch (intent)
                {
                    case HotelBotLuis.Intent.Cancel:
                    {
                        return await OnCancel(dc);
                    }
                    case HotelBotLuis.Intent.Help:
                    {
                        return await OnHelp(dc);
                    }
                    case HotelBotLuis.Intent.Update_ArrivalDate:
                    case HotelBotLuis.Intent.Update_Leaving_Date:
                    case HotelBotLuis.Intent.Update_email:
                    case HotelBotLuis.Intent.Update_Number_Of_People:
                    {

                        var dialogOptions = new DialogOptions
                        {
                            LuisResult = luisResult,
                            SkipConfirmation = dc.IsUpdateStateChoicePrompt()
                        };
                        return await OnUpdate(dc, dialogOptions);
                    }
                }

            // call the non overriden continue dialog in componentdialog
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
            var view = new FetchAvailableRoomsResponses();
            await view.ReplyWith(dc.Context, FetchAvailableRoomsResponses.ResponseIds.Help);

            // Signal the conversation was interrupted and should immediately continue (calls reprompt)
            return InterruptionStatus.Interrupted;
        }

        protected virtual async Task<InterruptionStatus> OnUpdate(DialogContext dc, DialogOptions options)
        {
            if (dc.ActiveDialog.Id != nameof(UpdateStatePrompt))
            {
                await dc.BeginDialogAsync(nameof(UpdateStatePrompt), options);

                return InterruptionStatus.Waiting;
            }

            return InterruptionStatus.NoAction;
        }
    }

}
