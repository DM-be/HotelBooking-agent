using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.ConfirmOrder;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.ContinueOrAddMoreRooms
{
    public class ContinueOrAddMoreRoomsPrompt: ComponentDialog
    {
        private static readonly RoomOverviewResponses _responder = new RoomOverviewResponses();
        private readonly StateBotAccessors _accessors;

        public ContinueOrAddMoreRoomsPrompt(StateBotAccessors accessors): base(nameof(ContinueOrAddMoreRoomsPrompt))
        {
            InitialDialogId = nameof(ContinueOrAddMoreRoomsPrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var ContinueOrAddMoreRoomsWaterfallSteps = new WaterfallStep []
            {
                PromptContinueOrAddMoreRooms, EndWithResult
            };

            AddDialog(new WaterfallDialog(InitialDialogId, ContinueOrAddMoreRoomsWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }

        //todo: refactor
        private async Task<DialogTurnResult> PromptContinueOrAddMoreRooms(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var roomOverviewState = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());

            var confirmOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());


            var templateId = RoomOverviewResponses.ResponseIds.CompleteOverview;
            var choices = new List<string>
            {
                RoomOverviewDialog.RoomOverviewChoices.AddARoom,
            };

            if (roomOverviewState.SelectedRooms.Count == 0)
            {
                templateId = RoomOverviewResponses.ResponseIds.NoSelectedRooms;
                choices = new List<string>
                {
                    RoomOverviewDialog.RoomOverviewChoices.FindRoom,

                };
            }
            if (confirmOrderState.PaymentConfirmed)
            {
                templateId = RoomOverviewResponses.ResponseIds.ConfirmedPaymentOverview;
                choices = new List<string> {
                       RoomOverviewDialog.RoomOverviewChoices.CancelRoom,
                        RoomOverviewDialog.RoomOverviewChoices.Receipt
                };
            }

            var convertedChoices = ChoiceFactory.ToChoices(choices);

            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        templateId, roomOverviewState),
                    RetryPrompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        RoomOverviewResponses.ResponseIds.RepromptUnconfirmed),
                    Choices = convertedChoices
                },
                cancellationToken);
        }


        private async Task<DialogTurnResult> EndWithResult(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            

            return await sc.EndDialogAsync(
                sc.Result);
        }
    }
}
