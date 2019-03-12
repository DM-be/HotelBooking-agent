using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Shared.Prompts
{


    // TODO: add datetime validator here
    public class ValidateDateTimePrompt: ComponentDialog
    {

        /// <summary>
        ///     Custom and reusable component dialog that validates a datetime, replaces the given replacing dialog and provides it
        ///     with a correct
        ///     timexproperty.
        ///     <param name="replacingDialogId">
        ///         Dialogid of the replacing dialog, will recieven a valid timexproperty in its
        ///         options.
        ///     </param>
        /// </summary>
        public ValidateDateTimePrompt(string replacingDialogId): base(nameof(ValidateDateTimePrompt))
        {
            if (replacingDialogId == null) throw new ArgumentNullException(nameof(replacingDialogId));
            InitialDialogId = nameof(ValidateDateTimePrompt);
            ReplacingDialogId = replacingDialogId;
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt)));
            var validateDateWaterFallSteps = new WaterfallStep []
            {
                PromptValidateDate, ReplaceWithValidatedDate
            };

            AddDialog(new WaterfallDialog(InitialDialogId, validateDateWaterFallSteps));
        }

        private string ReplacingDialogId { get; }

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

        public async Task<DialogTurnResult> ReplaceWithValidatedDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            // ends and calls resume() on parent dialog.
            return await sc.EndDialogAsync(new TimexProperty(resolution.Timex), cancellationToken);

        }
    }
}
