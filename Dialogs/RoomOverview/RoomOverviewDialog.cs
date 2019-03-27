using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Shared.CustomDialog;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.RoomOverview
{

    public class RoomOverviewDialog: RoomOverviewRecognizerDialog
    {

        private static BookARoomResponses _responder;
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();
        private readonly BotServices _services;


        public RoomOverviewDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomOverviewDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(RoomOverviewDialog);


            // send overview
            // send prompt asking to modify, update or confirm  (also implement in luis)
            // when confirmed --> send link to do "payment" --> no sql set backend validated boolean to true after payment via api? 
            var sendOverview = new WaterfallStep []
            {
                ShowOverview, PromptModify
            };
            AddDialog(new WaterfallDialog(InitialDialogId, sendOverview));


        }


        public async Task<DialogTurnResult> ShowOverview(WaterfallStepContext sc, CancellationToken cancellationToken)

        {

            return null;
        }

        public async Task<DialogTurnResult> PromptModify(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            return null;

        }
    }
}
