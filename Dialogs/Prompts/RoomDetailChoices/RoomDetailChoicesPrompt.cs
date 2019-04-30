using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.RoomDetailChoices.Resources;
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

                    Prompt = MessageFactory.Text(RoomDetailChoicesStrings.PROMPT_CHOICES),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            RoomDetailDialog.RoomDetailChoices.Rates,
                            RoomDetailDialog.RoomDetailChoices.Pictures,
                            RoomDetailDialog.RoomDetailChoices.ShowMeOtherRooms,
                            RoomDetailDialog.RoomDetailChoices.NoThanks
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
                case RoomDetailDialog.RoomDetailChoices.ShowMeOtherRooms:
                    var dialogOptions = new DialogOptions
                    {
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

                case RoomDetailDialog.RoomDetailChoices.Rates:
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendRates, state.RoomDetailDto);
                    return await sc.ReplaceDialogAsync(InitialDialogId);
                case RoomDetailDialog.RoomDetailChoices.Pictures:
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, state.RoomDetailDto);
                    return await sc.ReplaceDialogAsync(InitialDialogId);
                case RoomDetailDialog.RoomDetailChoices.NoThanks:
                    // end and prompt and end on waterfall above
                    await sc.Context.SendActivityAsync("You're welcome.");
                    return await sc.EndDialogAsync();
            }

            return null;
        }
    }
}
