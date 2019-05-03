using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Dialogs.Shared.RecognizerDialogs.ConfirmOrder;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.ConfirmOrder
{
    public class ConfirmOrderDialog: ConfirmOrderRecognizerDialog
    {

        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public ConfirmOrderDialog(BotServices services, StateBotAccessors accessors) : base(services, accessors, nameof(ConfirmOrderDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(ConfirmOrderDialog);
            var confirmOrderWaterfallSteps = new WaterfallStep []
            {
                initialStep
            };
            AddDialog(new WaterfallDialog(InitialDialogId, confirmOrderWaterfallSteps));

        }



        public async Task<DialogTurnResult> initialStep(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            return null;
        }
    }
}
