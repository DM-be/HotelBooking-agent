using System.Threading;
using System.Threading.Tasks;
using HotelBot.Models.Facebook;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.LocationPrompt
{
    public class LocationPromptDialog: ComponentDialog
    {

        private readonly LocationResponses _responder = new LocationResponses();

        public LocationPromptDialog()
            : base(nameof(LocationPromptDialog))
        {
            InitialDialogId = nameof(LocationPromptDialog);
            var askForEmailWaterfallSteps = new WaterfallStep []
            {
                AskForLocationAsync, ProcessNavigationOrLocationPromptAsync
            };
            AddDialog(new WaterfallDialog(InitialDialogId, askForEmailWaterfallSteps));

        }

        private async Task<DialogTurnResult> AskForLocationAsync(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            var coordinates = sc.Options as FacebookPayloadCoordinates;
            if (coordinates != null) return await sc.NextAsync();

            await _responder.ReplyWith(sc.Context, LocationResponses.ResponseIds.SendLocationQuickReply);
            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        private async Task<DialogTurnResult> ProcessNavigationOrLocationPromptAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var coordinates = sc.Options as FacebookPayloadCoordinates;
            await _responder.ReplyWith(sc.Context, LocationResponses.ResponseIds.SendNavigation, coordinates);
            return await sc.EndDialogAsync();
        }
    }
}
