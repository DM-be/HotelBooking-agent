using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.RoomDetail;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.SelectRatePrompt
{


    // todo: remove if not necessary? 
    public class SelectRatePrompt: ComponentDialog
    {
        private static SelectRateResponses _responder;
        private readonly StateBotAccessors _accessors;

        public SelectRatePrompt(StateBotAccessors accessors): base(nameof(SelectRatePrompt))
        {
            InitialDialogId = nameof(SelectRatePrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var selectRateWaterfallSteps = new WaterfallStep []
            {
                SendRates, PromptContinueOptions
            };
            AddDialog(new WaterfallDialog(InitialDialogId, selectRateWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        }


        public async Task<DialogTurnResult> SendRates(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.RoomDetailStateAccessor.GetAsync(sc.Context, () => new RoomDetailState());
            await _responder.ReplyWith(sc.Context, SelectRateResponses.ResponseIds.SendRates, state);
            return await sc.NextAsync();

        }

        public async Task<DialogTurnResult> PromptContinueOptions(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            return null;

        }
    }
}
