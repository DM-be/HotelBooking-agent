using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Models.Facebook;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.LocationPrompt
{
    public class LocationPromptDialog: ComponentDialog
    {

        private readonly StateBotAccessors _accessors;
        private readonly LocationResponses _responder = new LocationResponses();

        public LocationPromptDialog(StateBotAccessors accessors)
            : base(nameof(LocationPromptDialog))
        {
            InitialDialogId = nameof(LocationPromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForEmailWaterfallSteps = new WaterfallStep[]
            {
                AskForLocation, ProcessNavigationOrLocationPrompt
            };

            // navigate or just get location
            AddDialog(new WaterfallDialog(InitialDialogId, askForEmailWaterfallSteps));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));

        }


        private async Task<DialogTurnResult> AskForLocation(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            var coordinates = sc.Options as FacebookPayloadCoordinates;
            if (coordinates != null)
            {
                return await sc.NextAsync();
            }

            await _responder.ReplyWith(sc.Context, LocationResponses.ResponseIds.SendLocationQuickReply);
            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        private async Task<DialogTurnResult> ProcessNavigationOrLocationPrompt(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var coordinates = sc.Options as FacebookPayloadCoordinates;
            await _responder.ReplyWith(sc.Context, LocationResponses.ResponseIds.SendNavigation, coordinates);
            return await sc.EndDialogAsync();
        }
    }
}
