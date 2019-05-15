using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Prompts.FetchAvailableRoomsIntroduction
{
    public class FetchAvailableRoomsIntroductionPrompt: ComponentDialog
    {
        //todo: implement own separate responder
        private static FetchAvailableRoomsResponses _responder;

        public FetchAvailableRoomsIntroductionPrompt()
            : base(nameof(FetchAvailableRoomsIntroductionPrompt))
        {

            _responder = new FetchAvailableRoomsResponses();
            InitialDialogId = nameof(FetchAvailableRoomsIntroductionPrompt);
            var fetchAvailableRoomsWaterfallSteps = new WaterfallStep []
            {
                SendIntroAndPromptUnderstood, ProcessChoice
            };
            AddDialog(new WaterfallDialog(InitialDialogId, fetchAvailableRoomsWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

        }

        public async Task<DialogTurnResult> SendIntroAndPromptUnderstood(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            

            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        FetchAvailableRoomsResponses.ResponseIds.IntroductionMistakes),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            "OK 👌",
                            "Understand? 🙃"

                        })
                },
                cancellationToken);


        }

        public async Task<DialogTurnResult> ProcessChoice(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var foundChoice = sc.Result as FoundChoice;

            if (foundChoice != null)
            {
                switch (foundChoice.Value)
                {
                    case "OK 👌":
                        await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendStart);
                        return await sc.EndDialogAsync();
                    case "Understand? 🙃":
                        await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.UnderstandNLU);
                        await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.UnderstandExample);
                        // maybe sleep to give time to read?
                        await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendStart);
                        return await sc.EndDialogAsync();
                }

            }

            // not castable and not recognized --> loop dialog
            await sc.Context.SendActivityAsync("Sorry I didn't understand");
            // loop
            return await sc.ReplaceDialogAsync(InitialDialogId);
        }
    }





}
