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
        private static FetchAvailableRoomsResponses _responder;

        public FetchAvailableRoomsIntroductionPrompt()
            : base(nameof(FetchAvailableRoomsIntroductionPrompt))
        {

            _responder = new FetchAvailableRoomsResponses();
            InitialDialogId = nameof(FetchAvailableRoomsIntroductionPrompt);
            var fetchAvailableRoomsWaterfallSteps = new WaterfallStep []
            {
                SendIntroAndPromptUnderstoodAsync, ProcessChoiceAsync
            };
            AddDialog(new WaterfallDialog(InitialDialogId, fetchAvailableRoomsWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

        }

        public async Task<DialogTurnResult> SendIntroAndPromptUnderstoodAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
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
                            FetchAvailableRoomsIntroductionPromptChoices.Ok,
                            FetchAvailableRoomsIntroductionPromptChoices.Understand,
                        })
                },
                cancellationToken);


        }

        public async Task<DialogTurnResult> ProcessChoiceAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var foundChoice = sc.Result as FoundChoice;

            if (foundChoice != null)
            {
                switch (foundChoice.Value)
                {
                    case FetchAvailableRoomsIntroductionPromptChoices.Ok:
                        await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendStart);
                        return await sc.EndDialogAsync();
                    case FetchAvailableRoomsIntroductionPromptChoices.Understand:
                        await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.UnderstandNLU);
                        await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.UnderstandExample);
                        await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendStart);
                        return await sc.EndDialogAsync();
                }

            }

            // not castable and not recognized --> loop dialog
            await sc.Context.SendActivityAsync("Sorry I didn't understand");
            return await sc.ReplaceDialogAsync(InitialDialogId);
        }

        public class FetchAvailableRoomsIntroductionPromptChoices {
            public const string Ok = "OK 👌";
            public const string Understand = "Understand? 🙃";
        }

    }





}
