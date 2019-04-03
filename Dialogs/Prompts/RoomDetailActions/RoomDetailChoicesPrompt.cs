using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.RoomDetail;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.RoomDetailActions
{
    public class RoomDetailChoicesPrompt : ComponentDialog
    {
        private static readonly FetchAvailableRoomsResponses _responder = new FetchAvailableRoomsResponses();
        private readonly StateBotAccessors _accessors;

        public RoomDetailChoicesPrompt(StateBotAccessors accessors) : base(nameof(RoomDetailChoicesPrompt))
        {
            InitialDialogId = nameof(RoomDetailChoicesPrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var RoomDetailActionsWaterfallsteps = new WaterfallStep[]
            {
                PromptChoices, ProcessChoice
            };

            AddDialog(new WaterfallDialog(InitialDialogId, RoomDetailActionsWaterfallsteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }


        private async Task<DialogTurnResult> PromptChoices(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Ask me questions about the room or pick an option"),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            "View other rooms",
                            "Rates",
                            "Pictures"

                        })
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessChoice(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.RoomDetailStateAccessor.GetAsync(sc.Context, () => new RoomDetailState());
            var choice = sc.Result as FoundChoice;
            switch (choice.Value)
            {
                case "View other rooms":
                    
                    return await sc.ReplaceDialogAsync(nameof(FetchAvailableRoomsDialog));

                case "Rates":
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendRates, state.RoomDetailDto);
                    return await sc.ReplaceDialogAsync(InitialDialogId);
                case "Pictures":
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, state.RoomDetailDto);
                    return await sc.ReplaceDialogAsync(InitialDialogId);
            }

            return null;
        }
    }
}
