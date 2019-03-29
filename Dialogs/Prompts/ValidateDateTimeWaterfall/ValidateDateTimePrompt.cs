using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Extensions;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Prompts.ValidateDateTimeWaterfall
{


    // TODO: add datetime validator here
    public class ValidateDateTimePrompt: ComponentDialog
    {

        private readonly Shared.PromptValidators.PromptValidators _promptValidators = new Shared.PromptValidators.PromptValidators();

        /// <summary>
        ///     Custom and reusable component dialog that validates a datetime, ends the dialog and returns a timexproperty
        ///     <param name="replacingDialogId">
        ///         Dialogid of the replacing dialog, will recieve a valid timexproperty in options parameter
        ///     </param>
        /// </summary>
        public ValidateDateTimePrompt(): base(nameof(ValidateDateTimePrompt))
        {

            InitialDialogId = nameof(ValidateDateTimePrompt);
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), _promptValidators.DateValidatorAsync));
            var validateDateWaterFallSteps = new WaterfallStep []
            {
                PromptValidateDate, EndWithValidatedDate
            };

            AddDialog(new WaterfallDialog(InitialDialogId, validateDateWaterFallSteps));
        }

        public async Task<DialogTurnResult> PromptValidateDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var view = new ValidateDateTimeResponses();
            return await sc.PromptAsync(
                nameof(DateTimePrompt),
                new PromptOptions
                {
                    Prompt = await view.RenderTemplate(sc.Context, sc.Context.Activity.Locale, ValidateDateTimeResponses.ResponseIds.IncorrectFormatPrompt)
                });
        }

        public async Task<DialogTurnResult> EndWithValidatedDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var timexProperty = (sc.Result as IList<DateTimeResolution>).First().ConvertToTimex();
            // ends and calls resume() on parent dialog.
            return await sc.EndDialogAsync(timexProperty, cancellationToken);

        }
    }
}
