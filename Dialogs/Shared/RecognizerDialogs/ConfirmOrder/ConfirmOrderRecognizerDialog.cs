using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Cancel;
using HotelBot.Models.LUIS;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.RecognizerDialogs.ConfirmOrder
{
    public class ConfirmOrderRecognizerDialog: InterruptableDialog
    {
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public ConfirmOrderRecognizerDialog(BotServices services, StateBotAccessors accessors, string dialogId): base(dialogId)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new CancelDialog(_accessors));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

        }

        protected override async Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken)
        {

            _services.LuisServices.TryGetValue("hotelbot", out var luisService);

            if (luisService == null) throw new Exception("The specified LUIS Model could not be found in your Bot Services configuration.");

            var luisResult = await luisService.RecognizeAsync<HotelBotLuis>(dc.Context, cancellationToken);
            var intent = luisResult.TopIntent().intent;


            // Only triggers interruption if confidence level is high
            if (luisResult.TopIntent().score > 0.80)
                switch (intent)
                {
                    case HotelBotLuis.Intent.Cancel:
                    {
                        return await OnCancelAsync(dc);
                    }

                    case HotelBotLuis.Intent.Help:
                    {
                        // todo: provide contextual help
                        return await OnHelpAsync(dc);
                    }

                }

            // call the non overriden continue dialog in componentdialog
            return InterruptionStatus.NoAction;
        }



        protected virtual async Task<InterruptionStatus> OnCancelAsync(DialogContext dc)
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


        protected virtual async Task<InterruptionStatus> OnHelpAsync(DialogContext dc)
        {
            //todo implement contextual help
            // Signal the conversation was interrupted and should immediately continue
            return InterruptionStatus.Interrupted;
        }
    }
}
