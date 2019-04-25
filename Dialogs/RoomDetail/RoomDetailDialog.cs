using System;
using System.Collections.Generic;
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
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.RoomDetail
{



    // todo: also send introduction? or assume help knowledge from other dialog?  
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

            //check on rerouted --> avoid replace dialog sending another get request 
            if (dialogOptions.Rerouted)
            {
                state.RoomDetailDto = new RoomDetailDto();
                state.RoomDetailDto = await requestHandler.FetchRoomDetail(dialogOptions.RoomAction.Id);
                if (dialogOptions.RoomAction.Action == "info")
                {
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendDescription, state.RoomDetailDto);
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, state.RoomDetailDto);
                    await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendLowestRate, state.RoomDetailDto);
                }

            }

            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> PromptRoomChoices(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.BeginDialogAsync(nameof(RoomDetailChoicesPrompt));
        }



        //public async Task<DialogTurnResult> ProcessChoice(WaterfallStepContext sc, CancellationToken cancellationToken)
        //{
        //    var state = await _accessors.RoomDetailStateAccessor.GetAsync(sc.Context, () => new RoomDetailState());
        //    var choice = sc.Result as FoundChoice;
        //    switch (choice.Value)
        //    {
        //        case RoomDetailChoices.ViewOtherRooms:
        //            var dialogOptions = new DialogOptions
        //            {
        //                Rerouted = true,
        //                SkipConfirmation = false,
        //                SkipIntroduction = true
        //            };
        //            var dialogResult = new DialogResult
        //            {
        //                PreviousOptions = dialogOptions,
        //                TargetDialog = nameof(FetchAvailableRoomsDialog)
        //            };
        //            return await sc.EndDialogAsync(dialogResult);
        //        case RoomDetailChoices.ShowRates:
        //            // start show rates prompt


        //            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendRates, state.RoomDetailDto);
        //            var dialogOpts = sc.Options as DialogOptions;
        //            dialogOpts.Rerouted = false;
        //            dialogOpts.Looped = true;

        //            return await sc.ReplaceDialogAsync(InitialDialogId, dialogOpts);
        //        case RoomDetailChoices.NoThanks:
        //            return await sc.EndDialogAsync();

        //    }

        //    return null;

        //}


        public class RoomDetailChoices
        {
            public const string ViewOtherRooms = "New search";
            public const string ShowRates = "View rates";
            public const string NoThanks = "No thanks";
        }
    }
}
