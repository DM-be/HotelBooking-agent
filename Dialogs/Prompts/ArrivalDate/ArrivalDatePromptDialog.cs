using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
using HotelBot.Dialogs.Shared.PromptValidators;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Prompts.ArrivalDate
{
    public class ArrivalDatePromptDialog: ComponentDialog
    {
        private static readonly ArrivalDateResponses _responder = new ArrivalDateResponses();
        private readonly StateBotAccessors _accessors;
        private readonly PromptValidators _promptValidators = new PromptValidators();


        public ArrivalDatePromptDialog(StateBotAccessors accessors): base(nameof(ArrivalDatePromptDialog))
        {
            InitialDialogId = nameof(ArrivalDatePromptDialog);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            var askForArrivalDateWaterfallSteps = new WaterfallStep []
            {
                PromptForArrivalDate, FinishArrivalDatePromptDialog
            };

            AddDialog(new WaterfallDialog(InitialDialogId, askForArrivalDateWaterfallSteps));
            AddDialog(new DateTimePrompt(DialogIds.ArrivalDatePrompt, _promptValidators.DateValidatorAsync));
        }


        private async Task<DialogTurnResult> PromptForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            return await sc.PromptAsync(
                DialogIds.ArrivalDatePrompt,
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, ArrivalDateResponses.ResponseIds.ArrivalDatePrompt),
                    RetryPrompt = await _responder.RenderTemplate(

                        sc.Context,
                        sc.Context.Activity.Locale,
                        ArrivalDateResponses.ResponseIds.RetryArrivalDatePrompt)
                });
        }


        private async Task<DialogTurnResult> FinishArrivalDatePromptDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var updated = false;
            if (sc.Options != null) updated = (bool) sc.Options; // usually true

            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var timexProp = new TimexProperty(resolution.Timex);
            var arrivalDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);


            var _state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            _state.ArrivalDate = timexProp;

            if (updated)
                await _responder.ReplyWith(sc.Context, ArrivalDateResponses.ResponseIds.HaveUpdatedArrivalDate, arrivalDateAsNaturalLanguage);
            else
                await _responder.ReplyWith(sc.Context, ArrivalDateResponses.ResponseIds.HaveArrivalDate, arrivalDateAsNaturalLanguage);

            return await sc.EndDialogAsync();
        }



        private class DialogIds
        {
            public const string ArrivalDatePrompt = "arrivalDatePrompt";
        }
    }
}
