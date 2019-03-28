using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.NumberOfPeople
{
    public class NumberOfPeoplePromptDialog: ComponentDialog
    {
        private static readonly NumberOfPeopleResponses _responder = new NumberOfPeopleResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();

        public NumberOfPeoplePromptDialog(StateBotAccessors accessors)
            : base(nameof(NumberOfPeoplePromptDialog))
        {
            InitialDialogId = nameof(NumberOfPeoplePromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForNumberOfPeopleWaterfallSteps = new WaterfallStep[]
            {
                PromptForNumberOfPeople, FinishNumberOfPeoplePromptDialog
            };

            AddDialog(new WaterfallDialog(InitialDialogId, askForNumberOfPeopleWaterfallSteps));
            AddDialog(new NumberPrompt<int>(DialogIds.NumberOfPeoplePrompt));

        }

        private async Task<DialogTurnResult> PromptForNumberOfPeople(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                DialogIds.NumberOfPeoplePrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, NumberOfPeopleResponses.ResponseIds.NumberOfPeoplePrompt),
                    RetryPrompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, NumberOfPeopleResponses.ResponseIds.NumberOfPeoplePrompt)
                });

        }

        private async Task<DialogTurnResult> FinishNumberOfPeoplePromptDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var numberOfPeople = (int) sc.Result;
            var updated = false;
            if (sc.Options != null) updated = (bool)sc.Options; // usually true

            var _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            _state.NumberOfPeople = numberOfPeople;

            if (updated)
                await _responder.ReplyWith(sc.Context, NumberOfPeopleResponses.ResponseIds.HaveUpdatedNumberOfPeople, numberOfPeople);
            else
                await _responder.ReplyWith(sc.Context, NumberOfPeopleResponses.ResponseIds.HaveNumberOfPeople, numberOfPeople);

            return await sc.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string NumberOfPeoplePrompt = "numberOfPeoplePrompt";
        }




    }
}
