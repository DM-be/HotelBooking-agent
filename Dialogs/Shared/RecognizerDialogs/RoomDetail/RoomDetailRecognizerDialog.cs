using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Cancel;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.UpdateState;
using HotelBot.Models.LUIS;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.RecognizerDialogs.RoomDetail
{
    public class RoomDetailRecognizerDialog: InterruptableDialog
    {
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public RoomDetailRecognizerDialog(BotServices services, StateBotAccessors accessors, string dialogId)
            : base(dialogId)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new CancelDialog(_accessors));
            AddDialog(new UpdateStatePrompt(accessors));
        }

        protected override async Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken)
        {

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
                        // todo: provide contextual help
                        return await OnHelpAsync(dc);
                    }
                    case HotelBotLuis.Intent.Book_A_Room:
                    {
                        return await OnRerouteAsync(dc, intent);
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


        //todo: expand with other intents, show previous room etc
        protected virtual async Task<InterruptionStatus> OnRerouteAsync(DialogContext dc, HotelBotLuis.Intent intent)
        {

            if (intent == HotelBotLuis.Intent.Book_A_Room)
            {
                await dc.CancelAllDialogsAsync();
                dc.Context.TurnState.Add(TargetDialogKey, nameof(FetchAvailableRoomsDialog));
                return InterruptionStatus.Route;
            }
            
            return InterruptionStatus.NoAction;
        }



        protected virtual async Task<InterruptionStatus> OnHelpAsync(DialogContext dc)
        {
            var view = new FetchAvailableRoomsResponses();
            await view.ReplyWith(dc.Context, FetchAvailableRoomsResponses.ResponseIds.UnderstandNLU);

            // Signal the conversation was interrupted and should immediately continue (calls reprompt)
            return InterruptionStatus.Interrupted;
        }
    }
}
