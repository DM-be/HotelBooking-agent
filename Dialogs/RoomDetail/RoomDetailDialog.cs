using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Shared.RecognizerDialogs.RoomDetail;
using HotelBot.Models.DTO;
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
                FetchSelectedRoomDetail, ReplyImages, PromptActions, OnAction
            };

            AddDialog(new WaterfallDialog(InitialDialogId, roomDetailWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new BookARoomDialog(services, accessors));

        }

        public async Task<DialogTurnResult> FetchSelectedRoomDetail(WaterfallStepContext sc, CancellationToken cancellationToken)


        {
            var roomAction = (RoomAction) sc.Options;
            var requestHandler = new RequestHandler();
            _selectedRoomDetailDto = await requestHandler.FetchRoomDetail(roomAction.Id);
            return await sc.NextAsync();
        }


        public async Task<DialogTurnResult> ReplyImages(WaterfallStepContext sc, CancellationToken cancellationToken)


        {
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendExtraInfo, _selectedRoomDetailDto);
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendGeneralDescription, _selectedRoomDetailDto);
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, _selectedRoomDetailDto);
            return await sc.NextAsync();
        }


        public async Task<DialogTurnResult> PromptActions(WaterfallStepContext sc, CancellationToken cancellationToken)
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
                            "Book this room"
                           
                        })
                },
                cancellationToken);
        }

        public async Task<DialogTurnResult> OnAction(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var choice = sc.Result as FoundChoice;

            switch (choice.Value)
            {
                case "View other rooms":
                    return await sc.ReplaceDialogAsync(nameof(BookARoomDialog));
                    
                case "Book this room":
                    // start booking process\
                    return null;
                



            }

            return null;
        }
    }
}
