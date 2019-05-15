using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.RoomDetailChoices;
using HotelBot.Dialogs.Shared.RecognizerDialogs.RoomDetail;
using HotelBot.Models.DTO;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.RoomDetail
{



    // todo: also send introduction? or assume help knowledge from other dialog?  
    public class RoomDetailDialog: RoomDetailRecognizerDialog
    {
        private readonly StateBotAccessors _accessors;
        private readonly RoomDetailResponses _responder = new RoomDetailResponses();
        private readonly BotServices _services;

        public RoomDetailDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomDetailDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(RoomDetailDialog);

            var roomDetailWaterfallSteps = new WaterfallStep []
            {
                FetchSelectedRoomDetail, PromptRoomChoices
            };

            AddDialog(new WaterfallDialog(InitialDialogId, roomDetailWaterfallSteps));
            AddDialog(new FetchAvailableRoomsDialog(services, accessors));
            AddDialog(new RoomDetailChoicesPrompt(services, accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

        }

        public async Task<DialogTurnResult> FetchSelectedRoomDetail(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var dialogOptions = sc.Options as DialogOptions;
            var requestHandler = new RequestHandler();
            var state = await _accessors.RoomDetailStateAccessor.GetAsync(sc.Context, () => new RoomDetailState());
            state.RoomDetailDto = new RoomDetailDto();
            state.RoomDetailDto = await requestHandler.FetchRoomDetail(dialogOptions.RoomAction.RoomId);
            bool addRatesToChoices;
            if (dialogOptions.RoomAction.Action == RoomAction.Actions.Info)
            {
                await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendDescription, state.RoomDetailDto);
                await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, state.RoomDetailDto);
                await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendLowestRate, state.RoomDetailDto);
                addRatesToChoices = true;
                return await sc.NextAsync(addRatesToChoices);
            }

            addRatesToChoices = false;
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendRates, state.RoomDetailDto);
            return await sc.NextAsync(addRatesToChoices);
        }

        public async Task<DialogTurnResult> PromptRoomChoices(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            bool addRatesToChoices = (bool) sc.Result;
            return await sc.BeginDialogAsync(nameof(RoomDetailChoicesPrompt), addRatesToChoices);
        }

        public class RoomDetailChoices
        {
            public const string ViewOtherRooms = "View other rooms";
            public const string Rates = "Rates";
            public const string Pictures = "Pictures";
            public const string BookingOverview = "Booking overview";
        }
    }
}
