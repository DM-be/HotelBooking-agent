using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.ContinueOrAddMoreRooms
{
    public class ContinueOrAddMoreRoomsPrompt: ComponentDialog
    {
        private static readonly RoomOverviewResponses _responder = new RoomOverviewResponses();
        private readonly StateBotAccessors _accessors;

        public ContinueOrAddMoreRoomsPrompt(StateBotAccessors accessors) : base(nameof(ContinueOrAddMoreRoomsPrompt))
        {
            InitialDialogId = nameof(ContinueOrAddMoreRoomsPrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var ContinueOrAddMoreRoomsWaterfallSteps = new WaterfallStep[]
            {
                PromptContinueOrAddMoreRooms, EndWithResult
            };

            AddDialog(new WaterfallDialog(InitialDialogId, ContinueOrAddMoreRoomsWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }

        private async Task<DialogTurnResult> PromptContinueOrAddMoreRooms(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        RoomOverviewResponses.ResponseIds.ContinueOrAddMoreRooms),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            RoomOverviewDialog.RoomOverviewChoices.AddAnotherRoom,
                            RoomOverviewDialog.RoomOverviewChoices.ContinueToPayment,
                        })
                },
                cancellationToken);
        }


        private async Task<DialogTurnResult> EndWithResult(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.EndDialogAsync(
                sc.Result);
        }
    }
}
