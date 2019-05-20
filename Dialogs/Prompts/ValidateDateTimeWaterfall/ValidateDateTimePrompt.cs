using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using HotelBot.Models.Wrappers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.ValidateDateTimeWaterfall
{


    public class ValidateDateTimePrompt: ComponentDialog
    {

        private readonly PromptValidators _promptValidators = new PromptValidators();
        private readonly ValidateDateTimeResponses _responder = new ValidateDateTimeResponses();
        private readonly StateBotAccessors _accessors;

        /// <summary>
        ///     Custom and reusable component dialog that validates a datetime, ends the dialog and returns a timexproperty
        ///     <param name="accessors">

        ///     </param>
        /// </summary>
        public ValidateDateTimePrompt(StateBotAccessors accessors) : base(nameof(ValidateDateTimePrompt))
        {

            InitialDialogId = nameof(ValidateDateTimePrompt);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), _promptValidators.DateValidatorAsync));
            var validateDateWaterFallSteps = new WaterfallStep []
            {
                PromptValidateDate, EndWithValidatedDate
            };

            AddDialog(new WaterfallDialog(InitialDialogId, validateDateWaterFallSteps));
        }

        public async Task<DialogTurnResult> PromptValidateDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            string responseId;
            var dialogOptions = sc.Options as DialogOptions;
            if (dialogOptions.LuisResult.TopIntent().intent.Equals(HotelBotLuis.Intent.Update_ArrivalDate))
            {
                responseId = ValidateDateTimeResponses.ResponseIds.MatchedIntentNoEntityArrival;
            }
            else {
                responseId = ValidateDateTimeResponses.ResponseIds.MatchedIntentNoEntityDeparture;
            }
            return await sc.PromptAsync(
                nameof(DateTimePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, responseId) 
                });
        }

        public async Task<DialogTurnResult> EndWithValidatedDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var timexProperty = (sc.Result as IList<DateTimeResolution>).First().ConvertToTimex();
            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            state.TempTimexProperty = timexProperty;
            return await sc.EndDialogAsync(true, cancellationToken); // skip confirm because we already prompt for validation

        }
    }
}
