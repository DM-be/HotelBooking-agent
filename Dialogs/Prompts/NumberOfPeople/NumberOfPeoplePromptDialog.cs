using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Models.Wrappers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.NumberOfPeople
{
    public class NumberOfPeoplePromptDialog: ComponentDialog
    {
        private static readonly NumberOfPeopleResponses _responder = new NumberOfPeopleResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators(); // implement checking on max min number of people --> x hotel only has y size rooms etc

        public NumberOfPeoplePromptDialog(StateBotAccessors accessors)
            : base(nameof(NumberOfPeoplePromptDialog))
        {
            InitialDialogId = nameof(NumberOfPeoplePromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForNumberOfPeopleWaterfallSteps = new WaterfallStep []
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
                    RetryPrompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, NumberOfPeopleResponses.ResponseIds.RetryNumberOfPeoplePrompt),
                });

        }

        private async Task<DialogTurnResult> FinishNumberOfPeoplePromptDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var numberOfPeople = (int) sc.Result;
            bool updated = false;

            if (sc.Options != null)
            {
                var dialogOptions = (DialogOptions)sc.Options;
                updated = dialogOptions.UpdatedNumberOfPeople;
            }

            var _state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
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
