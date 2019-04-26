using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.RoomDetail;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.RoomDetailChoices
{
    public class RoomDetailChoicesPrompt: ComponentDialog
    {
        //todo: refactor into own responses!
        private static readonly RoomDetailResponses _responder = new RoomDetailResponses();
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public RoomDetailChoicesPrompt(BotServices services, StateBotAccessors accessors): base(nameof(RoomDetailChoicesPrompt))
        {
            InitialDialogId = nameof(RoomDetailChoicesPrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            var RoomDetailActionsWaterfallsteps = new WaterfallStep []
            {
                PromptChoices, ProcessChoice
            };

            AddDialog(new WaterfallDialog(InitialDialogId, RoomDetailActionsWaterfallsteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new FetchAvailableRoomsDialog(services, accessors));

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
                            "Pictures",
                            "Exit"
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
                    var dialogOptions = new DialogOptions
                    {
                        Rerouted = true,
                        SkipConfirmation = false,
                        SkipIntroduction = true
                    };
                    // clear state and run fetchavailableroomsdialog again
                    await _accessors.RoomDetailStateAccessor.SetAsync(sc.Context, new RoomDetailState());
                    var dialogResult = new DialogResult
                    {
                        PreviousOptions = dialogOptions,
                        TargetDialog = nameof(FetchAvailableRoomsDialog)
                    };
                    return await sc.EndDialogAsync(dialogResult);


                 //   return await sc.ReplaceDialogAsync(nameof(FetchAvailableRoomsDialog), dialogOptions);

                case "Rates":
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendRates, state.RoomDetailDto);
                    return await sc.ReplaceDialogAsync(InitialDialogId);
                case "Pictures":
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, state.RoomDetailDto);
                    return await sc.ReplaceDialogAsync(InitialDialogId);
                case "Exit":
                    // end and prompt and end on waterfall above. 
                    return await sc.EndDialogAsync();
            }

            return null;
        }
    }
}
