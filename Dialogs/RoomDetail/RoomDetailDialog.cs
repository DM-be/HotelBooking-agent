using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.RoomDetailActions;
using HotelBot.Dialogs.Shared.RecognizerDialogs.RoomDetail;
using HotelBot.Models.DTO;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.RoomDetail
{
    public class RoomDetailDialog: RoomDetailRecognizerDialog
    {
        private readonly StateBotAccessors _accessors;
        private readonly RoomDetailResponses _responder = new RoomDetailResponses();
        private readonly BotServices _services;
        private RoomDetailDto _selectedRoomDetailDto;


        public RoomDetailDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomDetailDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(RoomDetailDialog);

            var roomDetailWaterfallSteps = new WaterfallStep []
            {
                FetchSelectedRoomDetail, PromptRoomChoices, ProcessChoice
            };

            AddDialog(new WaterfallDialog(InitialDialogId, roomDetailWaterfallSteps));
            AddDialog(new FetchAvailableRoomsDialog(services, accessors));
            AddDialog(new RoomDetailChoicesPrompt(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }

        public async Task<DialogTurnResult> FetchSelectedRoomDetail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var roomAction = (RoomAction) sc.Options;
            var requestHandler = new RequestHandler();
            var state = await _accessors.RoomDetailStateAccessor.GetAsync(sc.Context, () => new RoomDetailState());
            state.RoomDetailDto = new RoomDetailDto();
            state.RoomDetailDto = await requestHandler.FetchRoomDetail(roomAction.Id);
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendDescription, state.RoomDetailDto);
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, state.RoomDetailDto);
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendLowestRate, state.RoomDetailDto);
            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> PromptRoomChoices(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("View more rates or book this room"),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            RoomDetailChoices.Book,
                            RoomDetailChoices.ShowRates,
                            RoomDetailChoices.ViewOtherRooms

                        })
                },
                cancellationToken);
        }



        public async Task<DialogTurnResult> ProcessChoice(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.RoomDetailStateAccessor.GetAsync(sc.Context, () => new RoomDetailState());
            var choice = sc.Result as FoundChoice;
            var roomAction = new RoomAction
            {
                Id = state.RoomDetailDto.Id,
                Action = "any"
            };
            switch (choice.Value)
            {
                case RoomDetailChoices.ViewOtherRooms:
                    var dialogOptions = new DialogOptions
                    {
                        Rerouted = true,
                        SkipConfirmation = false,
                        
                    };
                    var dialogResult = new DialogResult
                    {
                        PreviousOptions = dialogOptions,
                        TargetDialog = nameof(FetchAvailableRoomsDialog)
                    };
                    return await sc.EndDialogAsync(dialogResult);
                case RoomDetailChoices.ShowRates:
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendRates, state.RoomDetailDto);
                    return await sc.ReplaceDialogAsync(InitialDialogId, roomAction);
                case RoomDetailChoices.Book:
                {
                    // send Booking object to bookingdialog --> empty rate but with roomdetaildto
                    return null;
                }
            }

            return null;

        }


        public class RoomDetailChoices
        {
            public const string ViewOtherRooms = "New search";
            public const string ShowRates = "View rates";
            public const string Book = "Book";
        }
    }
}
