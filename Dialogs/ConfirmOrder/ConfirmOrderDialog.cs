using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Shared.RecognizerDialogs.ConfirmOrder;
using HotelBot.Services;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.ConfirmOrder
{
    public class ConfirmOrderDialog: ConfirmOrderRecognizerDialog
    {

        private static ConfirmOrderResponses _responder = new ConfirmOrderResponses();
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public ConfirmOrderDialog(BotServices services, StateBotAccessors accessors): base(services, accessors, nameof(ConfirmOrderDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(ConfirmOrderDialog);
            var confirmOrderWaterfallSteps = new WaterfallStep []
            {
                InitialStep
            };
            AddDialog(new WaterfallDialog(InitialDialogId, confirmOrderWaterfallSteps));

        }



        public async Task<DialogTurnResult> InitialStep(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var userProfile = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());
            var fullName = userProfile.FacebookProfileData.Name;

            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.UseFacebookName, fullName);
            return EndOfTurn;
        }
    }
}
