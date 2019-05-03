using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Cancel;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Models.LUIS;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.CustomDialog
{

    public class RoomOverviewRecognizerDialog: InterruptableDialog

    {


        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public RoomOverviewRecognizerDialog(BotServices services, StateBotAccessors accessors, string dialogId)
            : base(dialogId)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new CancelDialog(_accessors));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));


        }

        protected override async Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var text = dc.Context.Activity.Text;
            if (RoomOverviewDialog.RoomOverviewChoices.Choices.Contains(text)) return InterruptionStatus.NoAction;

            _services.LuisServices.TryGetValue("hotelbot", out var luisService);

            if (luisService == null) throw new Exception("The specified LUIS Model could not be found in your Bot Services configuration.");

            var luisResult = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);
            var intent = luisResult.TopIntent().intent;


            // Only triggers interruption if confidence level is high
            if (luisResult.TopIntent().score > 0.75)
                switch (intent)
                {
                    case HotelBotLuis.Intent.Cancel:
                    {
                        return await OnCancel(dc);
                    }

                    case HotelBotLuis.Intent.Help:
                    {
                        // todo: provide contextual help
                        return await OnHelp(dc);
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
            // todo: update to room overview help
            var view = new FetchAvailableRoomsResponses();
            await view.ReplyWith(dc.Context, FetchAvailableRoomsResponses.ResponseIds.Help);

            // Signal the conversation was interrupted and should immediately continue
            return InterruptionStatus.Interrupted;
        }
    }




    public class DialogIds
    {
        public const string ConfirmWaterfall = "confirmWaterfall";
        public const string DateConfirmWaterfall = "dateConfirmWaterfall";
    }

}
