using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Shared.RecognizerDialogs;
using HotelBot.Dialogs.Shared.RecognizerDialogs.RoomDetail;
using HotelBot.Models.DTO;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.RoomDetail
{
    public class RoomDetailDialog: RoomDetailRecognizerDialog
    {
        private readonly RoomDetailResponses _responder = new RoomDetailResponses();
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;


        public RoomDetailDialog(BotServices services, StateBotAccessors accessors)
            : base(services, accessors, nameof(RoomDetailDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(RoomDetailDialog);

            var roomDetailWaterfallSteps = new WaterfallStep []
            {
                ReplyGeneralDescription, ReplyImages, PromptActions
            };

            AddDialog(new WaterfallDialog(InitialDialogId, roomDetailWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }

        public async Task<DialogTurnResult> ReplyGeneralDescription(WaterfallStepContext sc, CancellationToken cancellationToken)


        {


            var roomAction = (RoomAction) sc.Options;
            if (roomAction.Action != "info")
            {
                return await sc.NextAsync();
            }

            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendGeneralDescription, roomAction.Id);

            return await sc.NextAsync();
        }


        public async Task<DialogTurnResult> ReplyImages(WaterfallStepContext sc, CancellationToken cancellationToken)


        {
            var roomAction = (RoomAction)sc.Options;
            if (roomAction.Action != "images")
            {
                return await sc.NextAsync();
            }
            await _responder.ReplyWith(sc.Context, RoomDetailResponses.ResponseIds.SendImages, roomAction.Id);

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
                            "Pictures",
                            "Info",
                        })
                },
                cancellationToken);
        }
    }
}
