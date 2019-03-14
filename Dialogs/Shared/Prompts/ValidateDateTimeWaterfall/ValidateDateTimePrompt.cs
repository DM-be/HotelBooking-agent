using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Extensions;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.Prompts
{


    // TODO: add datetime validator here
    public class ValidateDateTimePrompt: ComponentDialog
    {
        // todo: rename validators 
        private readonly Validators.Validators _validators = new Validators.Validators();

        /// <summary>
        ///     Custom and reusable component dialog that validates a datetime, replaces the given replacing dialog and provides it
        ///     with a correct timexproperty.
        ///     <param name="replacingDialogId">
        ///         Dialogid of the replacing dialog, will recieven a valid timexproperty in its
        ///         options.
        ///     </param>
        /// </summary>
        public ValidateDateTimePrompt(): base(nameof(ValidateDateTimePrompt))
        {

            InitialDialogId = nameof(ValidateDateTimePrompt);
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), _validators.DateValidatorAsync));
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
