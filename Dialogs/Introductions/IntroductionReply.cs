using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.Introductions
{
    public class IntroductionReply: ComponentDialog
    {

        private static FetchAvailableRoomsResponses _responder;
        private readonly StateBotAccessors _accessors;

        public IntroductionReply(BotServices services, StateBotAccessors accessors)
            : base(nameof(IntroductionReply))
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _responder = new FetchAvailableRoomsResponses();
            InitialDialogId = nameof(IntroductionReply);
            var fetchAvailableRoomsWaterfallSteps = new WaterfallStep []
            {
                SendIntroAndPromptUnderstood, ProcessChoice
            };
            AddDialog(new WaterfallDialog(InitialDialogId, fetchAvailableRoomsWaterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

        }

        public async Task<DialogTurnResult> SendIntroAndPromptUnderstood(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendIntroduction);

            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        FetchAvailableRoomsResponses.ResponseIds.SendMoreInfo),
                    Choices = ChoiceFactory.ToChoices(
                        new List<string>
                        {
                            "OK",
                            "Help"

                        })
                },
                cancellationToken);


        }

        public async Task<DialogTurnResult> ProcessChoice(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var foundChoice = new FoundChoice
            {
                Value = (string) sc.Result
            };


            switch (foundChoice.Value)
            {
                case "OK":
                    await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendStart);
                    return await sc.EndDialogAsync();
                case "Help":
                    await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.Help);
                    await _responder.ReplyWith(sc.Context, FetchAvailableRoomsResponses.ResponseIds.SendStart);
                    return await sc.EndDialogAsync();
            }

            // todo: dialogturnresult instead?

            return await sc.EndDialogAsync();
        }
    }





}
